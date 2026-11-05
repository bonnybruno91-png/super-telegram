using PlanningConverter.Domain;
using PlanningConverter.Spreadsheet;
using PlanningConverter.Validation;
using Xunit;

namespace PlanningConverter.Parsing.Tests;

/// <summary>
/// Ces trois fichiers sont ceux effectivement utilisés par le secrétariat (voir DEV_NOTES.md).
/// Les valeurs attendues ci-dessous ont d'abord été vérifiées par un prototype indépendant
/// (script Python, hors solution) avant d'être portées ici — c'est ce filet à deux implémentations
/// qui donne confiance dans le moteur avant même la première exécution sur une machine Windows.
/// </summary>
public class DaySectionParserRealFilesTests
{
    private static string FixturePath(string name) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

    private static AnalysisResult AnalyzeFirstSheet(string fileName)
    {
        var factory = new NpoiSpreadsheetReaderFactory();
        using var reader = factory.Open(FixturePath(fileName));
        var sheetName = reader.ListSheets()[0].Name;
        var grid = reader.ReadSheet(sheetName);
        var raw = AnalysisPipeline.AnalyzeSheet(grid, fileName);
        return ValidationEngine.Validate(raw);
    }

    [Fact]
    public void Semaine_23_juin_produces_six_events_promotion_MK4()
    {
        var result = AnalyzeFirstSheet("EDT_MK4_SEMAINE_23_DU_LUNDI_1_AU_VENDREDI_5_JUIN_2026.xlsx");

        Assert.Equal("MK4", result.PlanningName);
        Assert.Equal(6, result.Events.Count);
        Assert.Equal(new DateOnly(2026, 6, 1), result.PeriodStart);
        Assert.Equal(new DateOnly(2026, 6, 5), result.PeriodEnd);

        var first = result.Events[0];
        Assert.Equal(new DateOnly(2026, 6, 1), first.Date);
        Assert.Equal(DayOfWeek.Monday, first.Day);
        Assert.Equal(new TimeOnly(10, 0), first.StartTime);
        Assert.Equal(new TimeOnly(11, 0), first.EndTime);
        Assert.Equal("UE18", first.Ue);
        Assert.Equal("ORAL PATHOLOGIES SANTE MENTALE", first.Title);
        Assert.Equal("DURAND Alice", first.Teacher);
        Assert.Equal("P2 IFMKDV", first.Group);
        Assert.Null(first.Room);

        // Mercredi 03 juin : section de jour présente, aucun événement — ce n'est pas une erreur (§12 étage 3).
        Assert.DoesNotContain(result.Events, e => e.Date == new DateOnly(2026, 6, 3));
    }

    [Fact]
    public void Multi_teacher_and_multi_line_cells_are_joined_not_truncated()
    {
        var result = AnalyzeFirstSheet("EDT_MK4_SEMAINE_23_DU_LUNDI_1_AU_VENDREDI_5_JUIN_2026.xlsx");

        var evaluationOral = result.Events.Single(e => e.Title == "EXAMEN ORAL" && e.Date == new DateOnly(2026, 6, 2));
        Assert.Equal("PETIT Marc / GARCIA LOPEZ Hugo", evaluationOral.Teacher);
    }

    [Fact]
    public void Personal_work_block_has_no_teacher_no_room_and_is_still_valid()
    {
        var result = AnalyzeFirstSheet("EDT_MK4_SEMAINE_23_DU_LUNDI_1_AU_VENDREDI_5_JUIN_2026.xlsx");

        var temps = result.Events.Single(e => e.Title == "TEMPS PERSONNEL" && e.Date == new DateOnly(2026, 6, 4));
        Assert.Equal(EventKind.PersonalWork, temps.Kind);
        Assert.Null(temps.Teacher);
        Assert.Null(temps.Room);
        Assert.Equal(EventStatus.Valid, temps.Status);
    }

    [Fact]
    public void Semaine_19_mai_produces_eleven_events_including_room_and_group_variants()
    {
        var result = AnalyzeFirstSheet("MK4_SEMAINE_19_DU_LUNDI_4_AU_VENDREDI_8_MAI_2026.xlsx");

        // 10 séances horodatées + la ligne "VICTOIRE 1945" sans horaire (§57) = 11.
        Assert.Equal(11, result.Events.Count);

        // Une vraie salle, sans groupe extrait.
        var geriatrie = result.Events.Single(e => e.Title!.Contains("GERIATRIE"));
        Assert.Equal("AMPHI 3 LAENNEC", geriatrie.Room);
        Assert.Null(geriatrie.Group);

        // Deux groupes sur deux lignes de la même cellule, aucune salle.
        var ecos = result.Events.Single(e => e.Title!.Contains("INFORMATION ECOS"));
        Assert.Null(ecos.Room);
        Assert.Equal("P1 IFMKDV, S1 IFMKDV", ecos.Group);
    }

    [Fact]
    public void Holiday_row_without_any_time_is_flagged_as_all_day_candidate_not_silently_dropped()
    {
        var result = AnalyzeFirstSheet("MK4_SEMAINE_19_DU_LUNDI_4_AU_VENDREDI_8_MAI_2026.xlsx");

        var victoire = result.Events.SingleOrDefault(e => e.Title == "VICTOIRE 1945");
        Assert.NotNull(victoire);
        Assert.True(victoire!.IsAllDay);
        Assert.Equal(EventKind.Closure, victoire.Kind);
        Assert.Contains(victoire.Issues, i => i.Code == IssueCode.AllDayCandidate);
        // Avertissement, pas erreur bloquante : la ligne reste visible, pas perdue.
        Assert.Equal(EventStatus.Warning, victoire.Status);
    }

    [Fact]
    public void Semaine_26_juin_has_two_empty_days_and_five_events()
    {
        var result = AnalyzeFirstSheet("EDT_MK4_SEMAINE_26_DU_LUNDI_22_AU_VENDREDI_26_JUIN_2026.xlsx");

        Assert.Equal(5, result.Events.Count);
        Assert.DoesNotContain(result.Events, e => e.Date == new DateOnly(2026, 6, 23)); // mardi, vide
        Assert.DoesNotContain(result.Events, e => e.Date == new DateOnly(2026, 6, 26)); // vendredi, vide
    }

    [Fact]
    public void Missing_header_row_is_still_handled_by_content_based_time_column_detection()
    {
        // Ce fichier n'a pas d'en-tête "Heure" en A2 (voir DEV_NOTES.md) : la colonne horaire
        // doit être retrouvée par le contenu (§21, étage 5 du plan), pas par le texte d'en-tête.
        var result = AnalyzeFirstSheet("EDT_MK4_SEMAINE_26_DU_LUNDI_22_AU_VENDREDI_26_JUIN_2026.xlsx");

        Assert.All(result.Events, e => Assert.NotNull(e.StartTime));
    }

    [Fact]
    public void No_event_ever_has_an_invented_end_time()
    {
        // Garde-fou global (I1) : sur tout le corpus, aucun événement dont l'heure de fin
        // aurait été comblée sans preuve ne doit passer en statut Valide.
        foreach (var file in new[]
        {
            "EDT_MK4_SEMAINE_23_DU_LUNDI_1_AU_VENDREDI_5_JUIN_2026.xlsx",
            "MK4_SEMAINE_19_DU_LUNDI_4_AU_VENDREDI_8_MAI_2026.xlsx",
            "EDT_MK4_SEMAINE_26_DU_LUNDI_22_AU_VENDREDI_26_JUIN_2026.xlsx",
        })
        {
            var result = AnalyzeFirstSheet(file);
            foreach (var ev in result.Events)
            {
                if (ev.IsAllDay) continue;
                Assert.True(ev.EndTime is not null || ev.HasBlockingIssue,
                    $"'{ev.Title}' ({file}) a une heure de fin absente sans être marqué bloquant.");
            }
        }
    }
}
