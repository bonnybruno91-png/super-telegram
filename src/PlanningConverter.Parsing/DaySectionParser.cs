using PlanningConverter.Domain;
using PlanningConverter.Parsing.Lexicons;
using PlanningConverter.Parsing.Normalization;

namespace PlanningConverter.Parsing;

/// <summary>
/// Moteur d'analyse pour la disposition confirmée par le corpus réel (voir DEV_NOTES.md) :
///
///   PROMOTION MK4
///   Heure | Intitulé | Intervenant | Salle          ← en-tête (optionnel : absent au fichier 3)
///   Lundi 01 Juin 2026                               ← section de jour : seule une colonne remplie
///   10:00↵11:00 | UE18 ORAL ... | NOM Prénom | P2 IFMKDV
///   Mardi 02 Juin 2026                               ← nouvelle section, réinitialise le contexte
///   ...
///
/// Contrairement au modèle de fusion de cellules envisagé au départ (§14), aucune fusion
/// n'existe dans ces fichiers : le jour est un EN-TÊTE DE BLOC, pas une valeur répétée sur
/// plusieurs lignes d'une colonne. La propagation (§15) s'applique donc au niveau du bloc,
/// pas de la cellule.
///
/// Ce moteur ne prétend pas couvrir toutes les dispositions possibles (voir plan de conception
/// §5, étage 4 : la disposition "grille", jours en colonnes, reste à construire si un autre
/// établissement l'utilise). Il couvre fidèlement celle qui a été effectivement observée.
/// </summary>
public static class DaySectionParser
{
    public static (IReadOnlyList<ScheduleEvent> Events, IReadOnlyList<TraceEntry> Trace, string? Promotion)
        Parse(CellGrid grid, string sourceFile)
    {
        var trace = new List<TraceEntry>();
        var mapping = ColumnMapper.Map(grid);
        foreach (var note in mapping.Notes) trace.Add(TraceEntry.Now("ColumnMapper", note));

        if (!mapping.IsUsable)
        {
            trace.Add(TraceEntry.Now("DaySectionParser", "Aucune colonne 'Intitulé' identifiable — feuille écartée."));
            return ([], trace, null);
        }

        var promotion = ExtractPromotion(grid, trace);

        var events = new List<ScheduleEvent>();
        DayHeaderResult? currentDay = null;

        var scanStart = (mapping.HeaderRow ?? -1) + 1;
        for (var r = scanStart; r < grid.RowCount; r++)
        {
            var timeCell = mapping.TimeColumn is { } tc ? grid.Cell(r, tc) : null;
            var titleCell = grid.Cell(r, mapping.TitleColumn!.Value);
            var teacherCell = mapping.TeacherColumn is { } tec ? grid.Cell(r, tec) : null;
            var roomCell = mapping.RoomColumn is { } rc ? grid.Cell(r, rc) : null;
            var groupCell = mapping.GroupColumn is { } gc ? grid.Cell(r, gc) : null;

            if (titleCell is null)
            {
                // Cellule non initialisée dans la grille — ne se produit pas avec un CellGrid
                // produit par le lecteur réel (toutes les cellules y sont peuplées), mais une
                // grille construite à la main (tests) pourrait laisser un trou. Traité comme
                // une ligne sans contenu exploitable plutôt que de lever une exception (§145).
                continue;
            }

            var rowEmpty = (timeCell is null || timeCell.IsEmpty) && titleCell.IsEmpty &&
                           (teacherCell is null || teacherCell.IsEmpty) && (roomCell is null || roomCell.IsEmpty) &&
                           (groupCell is null || groupCell.IsEmpty);
            if (rowEmpty)
            {
                continue; // ligne vide : ne réinitialise pas le contexte (§15)
            }

            // Priorité à la reconnaissance du texte lui-même plutôt qu'à la forme de la ligne :
            // une cellule horaire vide (ou réduite à un simple retour à la ligne, comme observé
            // sur la ligne "VICTOIRE 1945" du corpus réel) ne doit PAS suffire à faire passer un
            // véritable événement pour un en-tête de section. On ne bascule en section de jour
            // QUE si le texte de la colonne Intitulé correspond réellement à un en-tête de date.
            if (!titleCell.IsEmpty)
            {
                var header = DateParser.TryParseDayHeader(titleCell.RawText);
                if (header is not null)
                {
                    var otherColumnsEmpty =
                        (timeCell is null || timeCell.IsEmpty) &&
                        (teacherCell is null || teacherCell.IsEmpty) &&
                        (roomCell is null || roomCell.IsEmpty) && (groupCell is null || groupCell.IsEmpty);

                    currentDay = header;
                    trace.Add(TraceEntry.Now("DaySectionParser",
                        $"Ligne {r + 1} : section '{titleCell.RawText}' → {header.Date:yyyy-MM-dd}" +
                        (header.WeekdayCoherent ? "" : " (jour annoncé incohérent avec la date)") +
                        (otherColumnsEmpty ? "" : " (colonnes voisines non vides — inhabituel pour une section)")));
                    continue;
                }
            }

            events.Add(BuildEvent(grid, r, sourceFile, promotion, currentDay, timeCell, titleCell, teacherCell, roomCell, groupCell));
        }

        trace.Add(TraceEntry.Now("DaySectionParser", $"{events.Count} ligne(s) d'événement construite(s)."));
        return (events, trace, promotion);
    }

    private static string? ExtractPromotion(CellGrid grid, List<TraceEntry> trace)
    {
        for (var r = 0; r < Math.Min(grid.RowCount, 3); r++)
        {
            for (var c = 0; c < grid.ColumnCount; c++)
            {
                var cell = grid.Cell(r, c);
                if (cell is null || cell.IsEmpty) continue;
                var code = PromotionLexicon.Extract(cell.RawText);
                if (code is not null)
                {
                    trace.Add(TraceEntry.Now("DaySectionParser", $"Promotion détectée : {code} (ligne {r + 1})."));
                    return code;
                }
            }
        }
        trace.Add(TraceEntry.Now("DaySectionParser", "Aucune promotion détectée dans les premières lignes."));
        return null;
    }

    private static ScheduleEvent BuildEvent(
        CellGrid grid, int row, string sourceFile, string? promotion, DayHeaderResult? day,
        SourceCell? timeCell, SourceCell titleCell, SourceCell? teacherCell, SourceCell? roomCell, SourceCell? groupCell)
    {
        var issues = new List<EventIssue>();
        var inferred = new HashSet<string>();
        var sourceCells = new List<SourceCell> { titleCell };

        // --- Date / jour : toujours hérités du contexte de section (jamais répétés sur la ligne). ---
        DateOnly? date = day?.Date;
        DayOfWeek? weekday = date?.DayOfWeek;
        if (date is not null)
        {
            inferred.Add(nameof(ScheduleEvent.Date));
            inferred.Add(nameof(ScheduleEvent.Day));
            if (day is { WeekdayCoherent: false })
            {
                issues.Add(new EventIssue(IssueCode.WeekdayMismatch, IssueSeverity.Warning,
                    $"Le jour annoncé ({day.DeclaredDay}) ne correspond pas à la date {day.Date:dd/MM/yyyy}.",
                    Field: nameof(ScheduleEvent.Date)));
            }
        }
        else
        {
            issues.Add(new EventIssue(IssueCode.MissingDate, IssueSeverity.Blocking,
                "Aucune section de jour active pour cette ligne : impossible de déterminer la date.",
                Field: nameof(ScheduleEvent.Date)));
        }

        // --- Horaire ---
        TimeOnly? start = null, end = null;
        var isAllDay = false;
        if (timeCell is not null && !timeCell.IsEmpty)
        {
            sourceCells.Add(timeCell);
            var range = TimeRangeParser.Parse(timeCell.RawText);
            start = range.Start;
            end = range.End;
            // Diagnostic fin : la cause précise (format non reconnu vs. réellement absent) est
            // tracée en Info sans dupliquer les anomalies bloquantes ajoutées plus bas, qui, elles,
            // ne regardent que la présence ou l'absence de la valeur résolue.
            foreach (var w in range.Warnings)
            {
                issues.Add(new EventIssue(IssueCode.InvalidTime, IssueSeverity.Info, w, Field: nameof(ScheduleEvent.StartTime)));
            }
        }

        var titleForAllDayCheck = titleCell.RawText;
        if (start is null && end is null)
        {
            if (AllDayHintLexicon.LooksLikeAllDayEvent(titleForAllDayCheck))
            {
                isAllDay = true;
                inferred.Add(nameof(ScheduleEvent.IsAllDay));
                issues.Add(new EventIssue(IssueCode.AllDayCandidate, IssueSeverity.Warning,
                    "Aucun horaire indiqué ; le libellé suggère un événement pleine journée. À confirmer.",
                    Field: nameof(ScheduleEvent.IsAllDay)));
            }
            else
            {
                issues.Add(new EventIssue(IssueCode.MissingStartTime, IssueSeverity.Blocking,
                    "Aucun horaire de début identifiable.", Field: nameof(ScheduleEvent.StartTime)));
                issues.Add(new EventIssue(IssueCode.MissingEndTime, IssueSeverity.Blocking,
                    "Aucun horaire de fin identifiable.", Field: nameof(ScheduleEvent.EndTime)));
            }
        }
        else if (end is null)
        {
            // §20 : ne jamais inventer une durée par défaut sans règle explicite de modèle.
            issues.Add(new EventIssue(IssueCode.MissingEndTime, IssueSeverity.Blocking,
                "Heure de fin non détectée.", Field: nameof(ScheduleEvent.EndTime)));
        }
        else if (start is not null && end < start)
        {
            issues.Add(new EventIssue(IssueCode.EndBeforeStart, IssueSeverity.Blocking,
                $"L'heure de fin {end:HH:mm} est antérieure à l'heure de début {start:HH:mm}.",
                Field: nameof(ScheduleEvent.EndTime)));
        }

        // --- Intitulé / UE ---
        var ueSplit = UeExtractor.Split(titleCell.RawText);
        var title = ueSplit.CleanTitle;
        if (string.IsNullOrWhiteSpace(title))
        {
            issues.Add(new EventIssue(IssueCode.MissingTitle, IssueSeverity.Blocking,
                "Aucun intitulé de cours identifiable.", Field: nameof(ScheduleEvent.Title)));
        }

        var kind = ClassifyKind(title, isAllDay);

        // --- Enseignant ---
        string? teacher = null;
        if (teacherCell is not null && !teacherCell.IsEmpty)
        {
            sourceCells.Add(teacherCell);
            teacher = string.Join(" / ", teacherCell.Lines);
        }

        // --- Salle : texte tel quel (P1 IFMKDV, S3 IFMKDV, AMPHI 4 LAENNEC, SUR SITE sont des salles) ---
        string? room = null;
        if (roomCell is not null && !roomCell.IsEmpty)
        {
            sourceCells.Add(roomCell);
            room = string.Join(", ", roomCell.Lines.Select(l => l.Trim()).Where(l => l.Length > 0));
        }

        // --- Groupe : uniquement la colonne « Groupe ». Vide = séance commune à toute la promotion. ---
        string? group = null;
        if (groupCell is not null && !groupCell.IsEmpty)
        {
            sourceCells.Add(groupCell);
            group = GroupLabel.Normalize(groupCell.RawText);
        }

        var originalContent = string.Join(" | ", sourceCells.Select(c => $"{c.A1Reference}={c.RawText.Replace('\n', '·')}"));

        var uid = UidGenerator.Generate(promotion, date, start, title, group);

        return new ScheduleEvent
        {
            Id = Guid.NewGuid(),
            Uid = uid,
            SourceFile = sourceFile,
            SourceSheet = grid.SheetName,
            SourceRow = row + 1, // affiché à l'utilisateur en base 1
            SourceCells = sourceCells,
            OriginalContent = originalContent,
            Date = date,
            Day = weekday,
            StartTime = start,
            EndTime = end,
            IsAllDay = isAllDay,
            Title = string.IsNullOrWhiteSpace(title) ? null : title,
            Teacher = teacher,
            Room = room,
            Location = room,
            Ue = ueSplit.Ue,
            Group = group,
            Promotion = promotion,
            Kind = kind,
            Status = kind == EventKind.Break
                ? EventStatus.Ignored // pause déjeuner : consultable, jamais diffusée (cahier §4.4)
                : issues.Any(i => i.Severity == IssueSeverity.Blocking) ? EventStatus.Error : EventStatus.Valid,
            Confidence = 1.0, // valeur provisoire — recalculée par PlanningConverter.Validation
            Issues = issues,
            InferredFields = inferred,
        };
    }

    private static EventKind ClassifyKind(string? title, bool isAllDay)
    {
        if (BreakLexicon.IsBreak(title)) return EventKind.Break;
        if (isAllDay) return EventKind.Closure;
        var normalized = title is null ? "" : HeaderSynonymLexicon.Normalize(title);
        if (normalized.Contains("temps personnel") || normalized.Contains("travail personnel"))
            return EventKind.PersonalWork;
        if (normalized.Contains("examen") || normalized.Contains("evaluation"))
            return EventKind.Exam;
        if (normalized.Contains("soutenance"))
            return EventKind.Defense;
        return EventKind.Course;
    }
}
