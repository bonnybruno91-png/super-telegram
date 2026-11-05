using PlanningConverter.Domain;
using PlanningConverter.Export;
using Xunit;

namespace PlanningConverter.Export.Tests;

public class FileNamingTests
{
    [Fact]
    public void Builds_expected_base_name()
    {
        var name = FileNaming.BuildBaseName("MK4", new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 5));
        Assert.Equal("Planning_MK4_2026-06-01_2026-06-05", name);
    }

    [Fact]
    public void Strips_characters_forbidden_on_windows()
    {
        var name = FileNaming.BuildBaseName("MK4:Test/Bad", new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 5));
        Assert.DoesNotContain(':', name);
        Assert.DoesNotContain('/', name);
    }

    [Fact]
    public void Resolves_collision_with_numbered_suffix()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        try
        {
            File.WriteAllText(Path.Combine(dir, "Planning_MK4_2026-06-01_2026-06-05.ics"), "x");
            var resolved = FileNaming.ResolveNonColliding(dir, "Planning_MK4_2026-06-01_2026-06-05", "ics");
            Assert.EndsWith("(2).ics", resolved);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }
}

public class TitleBuilderTests
{
    private static ScheduleEvent MakeEvent(string title, string? teacher, string? ue) => new()
    {
        Id = Guid.NewGuid(), Uid = "u", SourceFile = "f", SourceSheet = "s", SourceRow = 1,
        SourceCells = [], OriginalContent = "", Title = title, Teacher = teacher, Ue = ue,
        Issues = [], InferredFields = new HashSet<string>(),
    };

    [Fact]
    public void Default_format_is_course_dash_teacher()
    {
        var ev = MakeEvent("Neurologie", "Dr Martin", "UE18");
        Assert.Equal("UE18 Neurologie — Dr Martin", TitleBuilder.Build(ev, TitleFormat.CourseDashTeacher));
    }

    [Fact]
    public void Falls_back_to_title_alone_when_teacher_missing()
    {
        var ev = MakeEvent("Neurologie", null, "UE18");
        Assert.Equal("UE18 Neurologie", TitleBuilder.Build(ev, TitleFormat.CourseDashTeacher));
    }

    [Fact]
    public void Ue_format_prefixes_the_code()
    {
        var ev = MakeEvent("Neurologie", "Dr Martin", "UE18");
        Assert.Equal("UE18 — Neurologie", TitleBuilder.Build(ev, TitleFormat.UeDashCourse));
    }
}

public class DescriptionBuilderTests
{
    [Fact]
    public void Omits_absent_fields_entirely_rather_than_blank_lines()
    {
        var ev = new ScheduleEvent
        {
            Id = Guid.NewGuid(), Uid = "u", SourceFile = "f", SourceSheet = "s", SourceRow = 1,
            SourceCells = [], OriginalContent = "", Title = "Neurologie",
            Teacher = "Dr Martin", Room = null, Ue = "UE18", Group = null,
            Issues = [], InferredFields = new HashSet<string>(),
        };

        var description = DescriptionBuilder.Build(ev);
        Assert.Equal("Enseignant : Dr Martin", description);
        Assert.DoesNotContain("Salle", description);
        Assert.DoesNotContain("Groupe", description);
    }
}

public class PlanningTextFormatterTests
{
    private static ScheduleEvent MakeEvent(DateOnly date, TimeOnly start, TimeOnly end, string title, string? teacher, string? room) => new()
    {
        Id = Guid.NewGuid(), Uid = Guid.NewGuid().ToString(), SourceFile = "f", SourceSheet = "s", SourceRow = 1,
        SourceCells = [], OriginalContent = "", Date = date, Day = date.DayOfWeek,
        StartTime = start, EndTime = end, Title = title, Teacher = teacher, Room = room,
        Status = EventStatus.Valid, Issues = [], InferredFields = new HashSet<string>(),
    };

    [Fact]
    public void Detailed_format_matches_cahier_des_charges_example()
    {
        var events = new List<ScheduleEvent>
        {
            MakeEvent(new DateOnly(2026, 9, 14), new TimeOnly(8, 30), new TimeOnly(10, 30), "Neurologie", "Dr Martin", "204"),
        };

        var text = PlanningTextFormatter.Format(events, TextFormat.Detailed);

        Assert.Contains("Lundi 14 septembre 2026", text);
        Assert.Contains("08h30 - 10h30", text);
        Assert.Contains("Neurologie", text);
        Assert.Contains("Enseignant : Dr Martin", text);
        Assert.Contains("Salle : 204", text);
    }

    [Fact]
    public void Compact_format_matches_cahier_des_charges_example()
    {
        var events = new List<ScheduleEvent>
        {
            MakeEvent(new DateOnly(2026, 9, 14), new TimeOnly(8, 30), new TimeOnly(10, 30), "Neurologie", "Dr Martin", "204"),
        };

        var text = PlanningTextFormatter.Format(events, TextFormat.Compact);

        Assert.Contains("Lundi 14 septembre", text);
        Assert.Contains("08h30-10h30 : Neurologie — Dr Martin — Salle 204", text);
    }

    [Fact]
    public void Events_on_same_day_are_grouped_under_a_single_heading()
    {
        var events = new List<ScheduleEvent>
        {
            MakeEvent(new DateOnly(2026, 9, 14), new TimeOnly(8, 30), new TimeOnly(10, 30), "Neurologie", "Dr Martin", "204"),
            MakeEvent(new DateOnly(2026, 9, 14), new TimeOnly(11, 0), new TimeOnly(12, 30), "Traumatologie", "Mme Bernard", "201"),
        };

        var text = PlanningTextFormatter.Format(events, TextFormat.Detailed);

        var occurrences = text.Split("Lundi 14 septembre 2026").Length - 1;
        Assert.Equal(1, occurrences);
    }
}

public class GroupTitleTests
{
    private static ScheduleEvent Make(string? group, string? teacher) => new()
    {
        Id = Guid.NewGuid(), Uid = "u", SourceFile = "f", SourceSheet = "s", SourceRow = 1,
        SourceCells = [], OriginalContent = "", Title = "TK PNEUMOPATHOLOGIE", Ue = "UE21",
        Teacher = teacher, Group = group, Issues = [], InferredFields = new HashSet<string>(),
    };

    [Fact]
    public void Group_ends_the_title()
    {
        Assert.Equal("UE21 TK PNEUMOPATHOLOGIE — Groupe B",
            TitleBuilder.Build(Make("Groupe B", null), TitleFormat.CourseDashTeacher));
    }

    [Fact]
    public void Group_comes_after_teacher()
    {
        Assert.Equal("UE21 TK PNEUMOPATHOLOGIE — Eva Lambert — Groupe B",
            TitleBuilder.Build(Make("Groupe B", "Eva Lambert"), TitleFormat.CourseDashTeacher));
    }

    [Fact]
    public void Common_session_has_no_group_suffix()
    {
        Assert.Equal("UE21 TK PNEUMOPATHOLOGIE",
            TitleBuilder.Build(Make(null, null), TitleFormat.CourseDashTeacher));
    }
}
