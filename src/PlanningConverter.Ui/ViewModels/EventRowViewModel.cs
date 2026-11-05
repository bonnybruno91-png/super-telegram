using PlanningConverter.Domain;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>Une séance présentée dans la liste de vérification : date et heure d'abord, puis le détail.</summary>
public sealed class EventRowViewModel
{
    public ScheduleEvent Event { get; }

    public EventRowViewModel(ScheduleEvent ev) => Event = ev;

    public Guid Id => Event.Id;

    public bool IsIgnored => Event.Status == EventStatus.Ignored;
    public bool IsIncomplete => !IsIgnored && Event.HasBlockingIssue;

    public string TimeText
    {
        get
        {
            if (Event.IsAllDay) return "Journée entière";
            var start = Event.StartTime is { } s ? s.ToString("HH:mm") : "À préciser";
            var end = Event.EndTime is { } e ? e.ToString("HH:mm") : "À préciser";
            return $"{start} – {end}";
        }
    }

    public string TitleText
    {
        get
        {
            var title = Event.Title ?? "Intitulé à vérifier";
            return string.IsNullOrWhiteSpace(Event.Ue) ? title : $"{Event.Ue} {title}";
        }
    }

    public string DetailText
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Event.Teacher)) parts.Add(Event.Teacher.Replace(" / ", ", "));
            if (!string.IsNullOrWhiteSpace(Event.Room)) parts.Add($"Salle {Event.Room}");
            if (!string.IsNullOrWhiteSpace(Event.Group)) parts.Add(Event.Group);
            return string.Join("  •  ", parts);
        }
    }

    public bool HasDetail => DetailText.Length > 0;

    /// <summary>Toujours un mot : le statut n'est jamais transmis par la seule couleur.</summary>
    public string StatusText
    {
        get
        {
            if (IsIgnored) return "Ignoré";
            if (Event.Issues.Any(i => i.Code is IssueCode.MissingEndTime)) return "Heure de fin manquante";
            if (Event.Issues.Any(i => i.Code is IssueCode.MissingStartTime)) return "Horaire manquant";
            if (Event.Issues.Any(i => i.Code is IssueCode.MissingDate)) return "Date manquante";
            if (Event.HasBlockingIssue) return "À compléter";
            return Event.Status == EventStatus.Warning ? "À vérifier" : "";
        }
    }

    public bool HasStatus => StatusText.Length > 0;

    public string IssueDetail => string.Join(" ", Event.Issues
        .Where(i => i.Severity != IssueSeverity.Info)
        .Select(i => i.Message));

    public string EditLabel => $"Modifier : {TitleText}, {TimeText}";
    public string IgnoreLabel => IsIgnored ? $"Réintégrer : {TitleText}" : $"Ignorer : {TitleText}";
    public string IgnoreButtonText => IsIgnored ? "Réintégrer" : "Ignorer";
    public string EditButtonText => IsIncomplete ? "Corriger" : "Modifier";

    /// <summary>Nom accessible complet : date, heure, intitulé, détails, statut (cahier §10).</summary>
    public string AccessibleName
    {
        get
        {
            var date = Event.Date is { } d ? Capitalize(d.ToString("dddd d MMMM", FrenchCulture)) : "Date à vérifier";
            var parts = new List<string> { date, TimeText, TitleText };
            if (HasDetail) parts.Add(DetailText.Replace("  •  ", ", "));
            if (HasStatus) parts.Add(StatusText);
            return string.Join(", ", parts) + ".";
        }
    }

    private static readonly System.Globalization.CultureInfo FrenchCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");

    public static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}

/// <summary>Un jour et ses séances.</summary>
public sealed class DayGroupViewModel
{
    public string Header { get; }
    public IReadOnlyList<EventRowViewModel> Items { get; }

    public DayGroupViewModel(string header, IReadOnlyList<EventRowViewModel> items)
    {
        Header = header;
        Items = items;
    }
}
