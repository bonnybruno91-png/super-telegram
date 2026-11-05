using PlanningConverter.Application;
using PlanningConverter.Domain;
using PlanningConverter.Spreadsheet;
using PlanningConverter.Validation;
using Xunit;

namespace PlanningConverter.Parsing.Tests;

public class AutosaveRoundTripTests
{
    private static ConversionSession Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "EDT_MK4_SEMAINE_46_AVEC_GROUPES_NOV_2026.xlsx");
        using var reader = new NpoiSpreadsheetReaderFactory().Open(path);
        var grid = reader.ReadSheet(reader.ListSheets()[0].Name);
        var result = ValidationEngine.Validate(AnalysisPipeline.AnalyzeSheet(grid, path));
        return new ConversionSession(path, "hash123", result);
    }

    [Fact]
    public void Autosave_round_trip_keeps_events_corrections_and_session_id()
    {
        var session = Load();
        var target = session.Current.Events.First(e => e.Title!.Contains("TPG"));
        session.ReplaceEvent(target.Id, target with { Room = "S9 IFMKDV" });

        var restored = ConversionSession.FromAutosaveJson(session.ToAutosaveJson());

        Assert.NotNull(restored);
        Assert.Equal(session.SessionId, restored!.SessionId);
        Assert.Equal(session.SourceFileHash, restored.SourceFileHash);
        Assert.Equal(session.Current.Events.Count, restored.Current.Events.Count);
        Assert.Contains(restored.Current.Events, e => e.Room == "S9 IFMKDV" && e.EditedByUser);
        Assert.Equal(session.Current.PeriodStart, restored.Current.PeriodStart);
        Assert.Equal(
            session.Current.Events.Select(e => (e.Date, e.StartTime, e.Title, e.Group)).ToList(),
            restored.Current.Events.Select(e => (e.Date, e.StartTime, e.Title, e.Group)).ToList());
    }
}
