using PlanningConverter.Parsing.Normalization;
using Xunit;

namespace PlanningConverter.Parsing.Tests;

public class TimeParserTests
{
    [Theory]
    [InlineData("08:30", 8, 30)]
    [InlineData("8:30", 8, 30)]
    [InlineData("08h30", 8, 30)]
    [InlineData("8h30", 8, 30)]
    [InlineData("8 H 30", 8, 30)]
    [InlineData("0830", 8, 30)]
    [InlineData("14h", 14, 0)]
    [InlineData("23:59", 23, 59)]
    [InlineData("00:00", 0, 0)]
    public void Parses_known_formats(string input, int hour, int minute)
    {
        var result = TimeParser.Parse(input);
        Assert.True(result.Success, result.Error);
        Assert.Equal(new TimeOnly(hour, minute), result.Value);
    }

    [Theory]
    [InlineData("25:00")]
    [InlineData("12:60")]
    [InlineData("bonjour")]
    [InlineData("")]
    public void Rejects_invalid_input(string input)
    {
        var result = TimeParser.Parse(input);
        Assert.False(result.Success);
    }
}

public class TimeRangeParserTests
{
    [Fact]
    public void Parses_two_line_cell_format_confirmed_by_real_files()
    {
        var result = TimeRangeParser.Parse("10:00\n11:00");
        Assert.Equal(new TimeOnly(10, 0), result.Start);
        Assert.Equal(new TimeOnly(11, 0), result.End);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData("08h30 - 10h30")]
    [InlineData("08:30-10:30")]
    [InlineData("8h30 à 10h30")]
    [InlineData("08H30 / 10H30")]
    public void Parses_single_line_with_separator(string input)
    {
        var result = TimeRangeParser.Parse(input);
        Assert.Equal(new TimeOnly(8, 30), result.Start);
        Assert.Equal(new TimeOnly(10, 30), result.End);
    }

    [Fact]
    public void Missing_end_time_is_never_invented()
    {
        var result = TimeRangeParser.Parse("14h00");
        Assert.Equal(new TimeOnly(14, 0), result.Start);
        Assert.Null(result.End);
        Assert.Contains(result.Warnings, w => w.Contains("absente"));
    }

    [Fact]
    public void Empty_cell_yields_no_time_at_all()
    {
        var result = TimeRangeParser.Parse("");
        Assert.Null(result.Start);
        Assert.Null(result.End);
    }
}

public class DateParserTests
{
    [Theory]
    [InlineData("Lundi 01 Juin 2026", 2026, 6, 1, true)]
    [InlineData("Mardi 02 Juin 2026", 2026, 6, 2, true)]
    [InlineData("Vendredi 08 Mai 2026", 2026, 5, 8, true)]
    public void Parses_real_day_header_format(string text, int year, int month, int day, bool coherent)
    {
        var result = DateParser.TryParseDayHeader(text);
        Assert.NotNull(result);
        Assert.Equal(new DateOnly(year, month, day), result!.Date);
        Assert.Equal(coherent, result.WeekdayCoherent);
    }

    [Fact]
    public void Detects_weekday_name_date_mismatch()
    {
        // 1er juin 2026 est un lundi ; on annonce mardi par erreur.
        var result = DateParser.TryParseDayHeader("Mardi 01 Juin 2026");
        Assert.NotNull(result);
        Assert.False(result!.WeekdayCoherent);
    }

    [Fact]
    public void Rejects_text_that_is_not_a_day_header()
    {
        Assert.Null(DateParser.TryParseDayHeader("TEMPS PERSONNEL"));
        Assert.Null(DateParser.TryParseDayHeader("UE18 ORAL PATHOLOGIES SANTE MENTALE"));
    }
}
