using PlanningConverter.Domain;
using PlanningConverter.Parsing.Lexicons;
using PlanningConverter.Parsing.Normalization;
using PlanningConverter.Validation;
using Xunit;

namespace PlanningConverter.Parsing.Tests;

public class ValidationRecomputeTests
{
    private static ScheduleEvent Make(string title, TimeOnly? start, TimeOnly? end, string? group = null,
        string? room = null, string? teacher = null, IReadOnlyList<EventIssue>? issues = null) => new()
    {
        Id = Guid.NewGuid(), Uid = Guid.NewGuid().ToString(), SourceFile = "f", SourceSheet = "s", SourceRow = 1,
        SourceCells = [], OriginalContent = "", Date = new DateOnly(2026, 11, 9), Day = DayOfWeek.Monday,
        StartTime = start, EndTime = end, Title = title, Group = group, Room = room, Teacher = teacher,
        Issues = issues ?? [], InferredFields = new HashSet<string>(),
    };

    private static AnalysisResult Wrap(params ScheduleEvent[] events) => new()
    {
        SourceFile = "f", SheetName = "s", Events = events, Trace = [],
    };

    [Fact]
    public void Missing_end_time_blocks_then_correction_unblocks()
    {
        var incomplete = Make("Rhumatologie", new TimeOnly(14, 0), null);
        var first = ValidationEngine.Validate(Wrap(incomplete));
        Assert.Equal(EventStatus.Error, first.Events[0].Status);
        Assert.Contains(first.Events[0].Issues, i => i.Code == IssueCode.MissingEndTime);

        var corrected = first.Events[0] with { EndTime = new TimeOnly(16, 0) };
        var second = ValidationEngine.Validate(first with { Events = [corrected] });

        Assert.DoesNotContain(second.Events[0].Issues, i => i.Severity == IssueSeverity.Blocking);
        Assert.NotEqual(EventStatus.Error, second.Events[0].Status);
    }

    [Fact]
    public void Repeated_validation_does_not_accumulate_issues()
    {
        var a = Make("Neurologie", new TimeOnly(8, 30), new TimeOnly(10, 30));
        var b = Make("Neurologie", new TimeOnly(8, 30), new TimeOnly(10, 30));
        var once = ValidationEngine.Validate(Wrap(a, b));
        var thrice = ValidationEngine.Validate(ValidationEngine.Validate(once));

        Assert.Equal(once.Events[0].Issues.Count, thrice.Events[0].Issues.Count);
    }

    [Fact]
    public void Overlap_is_ignored_for_disjoint_groups_but_raised_for_same_or_common()
    {
        var a = Make("TD A", new TimeOnly(10, 30), new TimeOnly(12, 0), group: "P1 IFMKDV");
        var b = Make("TD B", new TimeOnly(10, 30), new TimeOnly(12, 0), group: "S1 IFMKDV");
        var disjoint = ValidationEngine.Validate(Wrap(a, b));
        Assert.DoesNotContain(disjoint.Events.SelectMany(e => e.Issues), i => i.Code == IssueCode.Overlap);

        var common = Make("CM", new TimeOnly(11, 0), new TimeOnly(12, 0));
        var withCommon = ValidationEngine.Validate(Wrap(a, common));
        Assert.Contains(withCommon.Events[0].Issues, i => i.Code == IssueCode.Overlap);
    }

    [Fact]
    public void Consecutive_sessions_do_not_overlap()
    {
        var a = Make("Cours 1", new TimeOnly(8, 0), new TimeOnly(10, 0));
        var b = Make("Cours 2", new TimeOnly(10, 0), new TimeOnly(12, 0));
        var result = ValidationEngine.Validate(Wrap(a, b));
        Assert.DoesNotContain(result.Events.SelectMany(e => e.Issues), i => i.Code == IssueCode.Overlap);
    }

    [Fact]
    public void Same_time_but_different_room_is_not_a_duplicate()
    {
        var a = Make("TP", new TimeOnly(9, 0), new TimeOnly(11, 0), room: "Salle 1");
        var b = Make("TP", new TimeOnly(9, 0), new TimeOnly(11, 0), room: "Salle 2");
        var result = ValidationEngine.Validate(Wrap(a, b));
        Assert.DoesNotContain(result.Events.SelectMany(e => e.Issues), i => i.Code == IssueCode.Duplicate);
    }

    [Fact]
    public void Exact_duplicate_is_flagged()
    {
        var a = Make("TP", new TimeOnly(9, 0), new TimeOnly(11, 0), room: "Salle 1", teacher: "X");
        var b = Make("TP", new TimeOnly(9, 0), new TimeOnly(11, 0), room: "Salle 1", teacher: "X");
        var result = ValidationEngine.Validate(Wrap(a, b));
        Assert.All(result.Events, e => Assert.Contains(e.Issues, i => i.Code == IssueCode.Duplicate));
    }
}

public class LexiconTests
{
    [Theory]
    [InlineData("UE12 Congestion pulmonaire")]
    [InlineData("Ponction lombaire TP")]
    [InlineData("Repassage des gestes techniques")]
    public void Clinical_titles_containing_keyword_fragments_are_not_all_day_or_break(string title)
    {
        Assert.False(AllDayHintLexicon.LooksLikeAllDayEvent(title));
        Assert.False(BreakLexicon.IsBreak(title));
    }

    [Theory]
    [InlineData("VICTOIRE 1945")]
    [InlineData("Jour férié")]
    [InlineData("Armistice")]
    public void Named_holidays_are_recognised(string title) =>
        Assert.True(AllDayHintLexicon.LooksLikeAllDayEvent(title));

    [Theory]
    [InlineData("PAUSE DEJEUNER")]
    [InlineData("Pause")]
    public void Lunch_breaks_are_recognised(string title) => Assert.True(BreakLexicon.IsBreak(title));
}

public class DayHeaderTests
{
    [Fact]
    public void Parses_first_of_month_with_er_suffix()
    {
        var h = DateParser.TryParseDayHeader("Lundi 1er Juin 2026");
        Assert.NotNull(h);
        Assert.Equal(new DateOnly(2026, 6, 1), h!.Date);
        Assert.True(h.WeekdayCoherent);
    }

    [Fact]
    public void Parses_native_excel_rendering_of_a_date_cell()
    {
        var h = DateParser.TryParseDayHeader("lundi 01 juin 2026");
        Assert.NotNull(h);
        Assert.Equal(new DateOnly(2026, 6, 1), h!.Date);
    }
}
