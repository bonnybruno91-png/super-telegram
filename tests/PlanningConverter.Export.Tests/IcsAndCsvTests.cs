using System.Text;
using PlanningConverter.Domain;
using PlanningConverter.Export;
using Xunit;

namespace PlanningConverter.Export.Tests;

internal static class Fx
{
    public static ScheduleEvent Event(string title, DateOnly date, TimeOnly? start, TimeOnly? end,
        string? teacher = null, string? room = null, EventKind kind = EventKind.Course,
        IReadOnlyList<EventIssue>? issues = null, bool allDay = false) => new()
    {
        Id = Guid.NewGuid(), Uid = Guid.NewGuid().ToString("N") + "@planning-converter.local",
        SourceFile = "f", SourceSheet = "s", SourceRow = 1, SourceCells = [], OriginalContent = "",
        Date = date, Day = date.DayOfWeek, StartTime = start, EndTime = end, IsAllDay = allDay,
        Title = title, Teacher = teacher, Room = room, Kind = kind,
        Issues = issues ?? [], InferredFields = new HashSet<string>(),
    };

    public static ExportRequest Request(IReadOnlyList<ScheduleEvent> events, string folder) => new()
    {
        Events = events, OutputFolder = folder, Promotion = "MK4",
        PeriodStart = new DateOnly(2026, 11, 9), PeriodEnd = new DateOnly(2026, 11, 13),
        GeneratedAt = new DateTimeOffset(2026, 11, 5, 14, 30, 0, TimeSpan.FromHours(1)),
    };

    public static readonly EventIssue Blocking =
        new(IssueCode.MissingEndTime, IssueSeverity.Blocking, "fin manquante");
}

public class IcsWriterTests
{
    [Fact]
    public void Escapes_special_characters()
    {
        Assert.Equal("a\\, b\\; c\\\\ d\\ne", IcsWriter.EscapeText("a, b; c\\ d\ne"));
    }

    [Fact]
    public void Folds_long_lines_at_75_octets_without_splitting_characters()
    {
        var line = "SUMMARY:" + string.Concat(Enumerable.Repeat("é", 120));
        var folded = IcsWriter.Fold(line);
        var physical = folded.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        Assert.True(physical.Length > 1);
        Assert.All(physical, l => Assert.True(Encoding.UTF8.GetByteCount(l) <= 75));
        Assert.All(physical.Skip(1), l => Assert.StartsWith(" ", l));

        var unfolded = string.Concat(physical.Select((l, i) => i == 0 ? l : l[1..]));
        Assert.Equal(line, unfolded);
    }
}

public class IcsExporterTests
{
    [Fact]
    public void Build_contains_timezone_and_local_times_and_skips_incomplete_and_breaks()
    {
        var d = new DateOnly(2026, 11, 9);
        var events = new List<ScheduleEvent>
        {
            Fx.Event("Neurologie", d, new TimeOnly(8, 30), new TimeOnly(10, 30), "Mme Martin", "Salle 204"),
            Fx.Event("Rhumatologie", d, new TimeOnly(14, 0), null, issues: [Fx.Blocking]),
            Fx.Event("PAUSE", d, new TimeOnly(12, 0), new TimeOnly(13, 0), kind: EventKind.Break),
            Fx.Event("Armistice", new DateOnly(2026, 11, 11), null, null, kind: EventKind.Closure, allDay: true),
        };

        var selected = ExportSelection.Publishable(events);
        var ics = IcsExporter.Build(selected, Fx.Request(events, "."));

        Assert.StartsWith("BEGIN:VCALENDAR\r\n", ics);
        Assert.EndsWith("END:VCALENDAR\r\n", ics);
        Assert.Contains("BEGIN:VTIMEZONE", ics);
        Assert.Contains("DTSTART;TZID=Europe/Paris:20261109T083000", ics);
        Assert.Contains("DTEND;TZID=Europe/Paris:20261109T103000", ics);
        Assert.Contains("SUMMARY:Neurologie — Mme Martin", ics);
        Assert.Contains("LOCATION:Salle 204", ics);
        Assert.Contains("DTSTART;VALUE=DATE:20261111", ics);
        Assert.Contains("DTEND;VALUE=DATE:20261112", ics);
        Assert.DoesNotContain("Rhumatologie", ics);
        Assert.DoesNotContain("PAUSE", ics);
        Assert.Equal(2, ics.Split("BEGIN:VEVENT").Length - 1);
        Assert.Single(ics.Split("CALSCALE:").Skip(1));
        Assert.DoesNotContain("\n\n", ics.Replace("\r\n", "\n"));
    }

    [Fact]
    public async Task Export_writes_file_without_bom_and_one_event_per_publishable_session()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var d = new DateOnly(2026, 11, 9);
            var events = new List<ScheduleEvent>
            {
                Fx.Event("Neurologie", d, new TimeOnly(8, 30), new TimeOnly(10, 30)),
                Fx.Event("Rhumatologie", d, new TimeOnly(14, 0), null, issues: [Fx.Blocking]),
            };

            var result = await new IcsExporter().ExportAsync(Fx.Request(events, dir));

            Assert.Equal(1, result.EventCount);
            var bytes = await File.ReadAllBytesAsync(result.Path);
            Assert.False(bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF);
            Assert.Contains("export-2026-11-05_1430", Path.GetFileName(result.Path));
            Assert.Empty(Directory.GetFiles(dir, "*.part"));
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}

public class CsvExporterTests
{
    [Theory]
    [InlineData("=1+1", "'=1+1")]
    [InlineData("+33", "'+33")]
    [InlineData("-x", "'-x")]
    [InlineData("@SUM(A1)", "'@SUM(A1)")]
    [InlineData("Neurologie", "Neurologie")]
    public void Protects_against_formula_injection(string input, string expected) =>
        Assert.Equal(expected, CsvWriterLite.Protect(input));

    [Fact]
    public void Counts_records_even_with_multiline_quoted_fields()
    {
        var csv = "A;B\r\n\"ligne1\nligne2\";x\r\ny;\"a \"\"q\"\" b\"\r\n";
        Assert.Equal(2, CsvWriterLite.CountRecords(csv));
    }

    [Theory]
    [InlineData(CsvProfile.Generic)]
    [InlineData(CsvProfile.GoogleCalendar)]
    public async Task Export_succeeds_for_both_profiles_with_multiline_descriptions(CsvProfile profile)
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var d = new DateOnly(2026, 11, 9);
            var events = new List<ScheduleEvent>
            {
                Fx.Event("Neurologie, partie 1", d, new TimeOnly(8, 30), new TimeOnly(10, 30), "Mme Martin", "Salle 204"),
                Fx.Event("Biomécanique", d, new TimeOnly(10, 45), new TimeOnly(12, 15), "M. Bernard", "Salle 102"),
            };
            var request = Fx.Request(events, dir) with { CsvProfile = profile };

            var result = await new CsvExporter().ExportAsync(request);

            Assert.Equal(2, result.EventCount);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }
}

public class NamingAndSelectionTests
{
    [Fact]
    public void Series_name_contains_group_period_and_generation_stamp()
    {
        var name = FileNaming.BuildBaseName("MK4", new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 20),
            "Groupe A", new DateTimeOffset(2026, 11, 5, 14, 30, 0, TimeSpan.Zero));
        Assert.StartsWith("MK4_Groupe-A_2026-11-09_2026-11-20_export-2026-11-05_", name);
    }

    [Fact]
    public void Series_base_name_gets_same_suffix_for_all_extensions_on_collision()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        try
        {
            File.WriteAllText(Path.Combine(dir, "base.ics"), "x");
            var resolved = FileNaming.ResolveSeriesBaseName(dir, "base", ["ics", "txt"]);
            Assert.Equal("base (2)", resolved);
        }
        finally { Directory.Delete(dir, recursive: true); }
    }

    [Fact]
    public void Publishable_orders_by_date_then_time_and_excludes_ignored_breaks_and_blocking()
    {
        var d = new DateOnly(2026, 11, 9);
        var late = Fx.Event("B", d, new TimeOnly(14, 0), new TimeOnly(15, 0));
        var early = Fx.Event("A", d, new TimeOnly(9, 0), new TimeOnly(10, 0));
        var ignored = Fx.Event("I", d, new TimeOnly(11, 0), new TimeOnly(12, 0)) with { Status = EventStatus.Ignored };
        var broken = Fx.Event("X", d, new TimeOnly(16, 0), null, issues: [Fx.Blocking]);

        var result = ExportSelection.Publishable([late, ignored, broken, early]);

        Assert.Equal(["A", "B"], result.Select(e => e.Title!).ToArray());
        Assert.Single(ExportSelection.Excluded([late, ignored, broken, early]));
    }
}
