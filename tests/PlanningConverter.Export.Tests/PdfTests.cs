using System.Text;
using System.Text.RegularExpressions;
using PlanningConverter.Domain;
using PlanningConverter.Export;
using Xunit;

namespace PlanningConverter.Export.Tests;

public class PdfTests
{
    private static ScheduleEvent Ev(int day, int hour, string title, string? teacher = "Eva Lambert", string? group = null) => new()
    {
        Id = Guid.NewGuid(), Uid = title + day + hour, SourceFile = "f", SourceSheet = "s", SourceRow = 1,
        SourceCells = [], OriginalContent = "", Date = new DateOnly(2026, 11, day), Day = new DateOnly(2026, 11, day).DayOfWeek,
        StartTime = new TimeOnly(hour, 0), EndTime = new TimeOnly(hour + 2, 0), Title = title, Ue = "UE21",
        Teacher = teacher, Room = "S3 IFMKDV", Group = group, Issues = [], InferredFields = new HashSet<string>(),
    };

    private static byte[] Build(IReadOnlyList<ScheduleEvent> events) =>
        PdfPlanningWriter.Build(events, "MK4", new DateOnly(2026, 11, 9), new DateOnly(2026, 11, 13));

    [Fact]
    public void Produces_a_structurally_valid_pdf_with_correct_xref_offsets()
    {
        var bytes = Build([Ev(9, 8, "Rééducation (cas n°2) \\ test")]);
        var text = Encoding.Latin1.GetString(bytes);

        Assert.StartsWith("%PDF-1.4", text);
        Assert.EndsWith("%%EOF\n", text);

        var startxref = long.Parse(Regex.Match(text, @"startxref\n(\d+)").Groups[1].Value);
        Assert.StartsWith("xref", text[(int)startxref..]);

        var entries = Regex.Matches(text[(int)startxref..], @"(\d{10}) 00000 n ");
        Assert.True(entries.Count >= 7);
        var n = 1;
        foreach (Match m in entries)
        {
            var offset = int.Parse(m.Groups[1].Value);
            Assert.StartsWith($"{n} 0 obj", text[offset..]);
            n++;
        }
        Assert.True(PdfPlanningWriter.LooksLikePdf(bytes));
    }

    [Fact]
    public void Text_is_selectable_winansi_with_group_and_accents()
    {
        var bytes = Build([Ev(10, 15, "TK PNEUMOPATHOLOGIE", group: "Groupe B")]);
        var text = Encoding.Latin1.GetString(bytes);

        Assert.Contains("UE21 TK PNEUMOPATHOLOGIE \x97 Groupe B", text);  // 0x97 = tiret cadratin en WinAnsi
        Assert.Contains("Mardi 10 novembre 2026", text);
        Assert.Contains("15h00 - 17h00", text);
        Assert.Contains("Salle : S3 IFMKDV", text);
        Assert.Contains("/Lang (fr-FR)", text);
    }

    [Fact]
    public void Body_text_is_at_least_18pt()
    {
        var text = Encoding.Latin1.GetString(Build([Ev(9, 8, "Cours")]));
        var sizes = Regex.Matches(text, @"/F[12] ([\d.]+) Tf").Select(m => double.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture));
        // Seul le pied de page (numéro de page) est en dessous de 18 pt.
        Assert.Equal(1, sizes.Count(s => s < 18));
    }

    [Fact]
    public void Many_events_flow_onto_several_pages_without_splitting_an_event()
    {
        var many = Enumerable.Range(0, 40).Select(i => Ev(9 + i % 5, 8, $"COURS NUMERO {i} AVEC UN INTITULE ASSEZ LONG POUR ETRE REPLIE SUR PLUSIEURS LIGNES DANS LA PAGE")).ToList();
        var text = Encoding.Latin1.GetString(Build(many));
        var pageCount = int.Parse(Regex.Match(text, @"/Count (\d+)").Groups[1].Value);
        Assert.True(pageCount > 1);
        Assert.Equal(pageCount, Regex.Matches(text, @"/Type /Page /Parent").Count);
        Assert.Contains($"Page {pageCount} sur {pageCount}", text);
    }

    [Fact]
    public void Empty_planning_still_yields_a_valid_pdf_with_a_message()
    {
        var bytes = Build([]);
        Assert.True(PdfPlanningWriter.LooksLikePdf(bytes));
        Assert.Contains("Aucun cours", Encoding.Latin1.GetString(bytes));
    }

    [Fact]
    public async Task Exporter_writes_file_and_names_it_with_pdf_extension()
    {
        var dir = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var file = await new PdfExporter().ExportAsync(new ExportRequest
            {
                Events = [Ev(9, 8, "Cours")], OutputFolder = dir, Promotion = "MK4",
                PeriodStart = new DateOnly(2026, 11, 9), PeriodEnd = new DateOnly(2026, 11, 13),
            });
            Assert.EndsWith(".pdf", file.Path);
            Assert.True(File.Exists(file.Path));
            Assert.Equal(1, file.EventCount);
        }
        finally { Directory.Delete(dir, true); }
    }
}
