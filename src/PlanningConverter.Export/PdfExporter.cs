using System.Globalization;
using System.Text;
using PlanningConverter.Domain;

namespace PlanningConverter.Export;

/// <summary>
/// PDF « gros caractères » (corps 18 pt, titres de jour 22 pt), texte sélectionnable, polices standard
/// Helvetica (aucune dépendance externe). Écrit à la main : PDF 1.4, A4 portrait, une séance ne
/// se coupe jamais entre deux pages. Limite connue : le PDF n'est pas balisé (pas d'arborescence
/// d'accessibilité) ; le fichier TXT reste la sortie de référence pour un lecteur d'écran.
/// </summary>
public sealed class PdfExporter : IExporter
{
    public string Id => "pdf";
    public string DisplayName => "PDF gros caractères";

    public async Task<ExportedFile> ExportAsync(ExportRequest request, CancellationToken ct = default)
    {
        var events = ExportSelection.Publishable(request.Events);
        var bytes = PdfPlanningWriter.Build(events, request.Promotion, request.PeriodStart, request.PeriodEnd);

        Directory.CreateDirectory(request.OutputFolder);
        var baseName = request.BaseName ?? FileNaming.BuildBaseName(request.Promotion, request.PeriodStart,
            request.PeriodEnd, request.GroupLabel, request.GeneratedAt);
        var path = FileNaming.ResolveNonColliding(request.OutputFolder, baseName, "pdf");

        var temp = path + ".part";
        try
        {
            await File.WriteAllBytesAsync(temp, bytes, ct);
            File.Move(temp, path, overwrite: false);
        }
        finally
        {
            if (File.Exists(temp)) File.Delete(temp);
        }

        var written = await File.ReadAllBytesAsync(path, ct);
        if (written.Length < 100 || !PdfPlanningWriter.LooksLikePdf(written))
        {
            throw new IOException($"Le fichier PDF généré est invalide : {path}");
        }
        return new ExportedFile(path, written.Length, events.Count);
    }
}

/// <summary>Mise en page et écriture du PDF. Séparé de l'exporteur pour pouvoir être testé sans disque.</summary>
public static class PdfPlanningWriter
{
    private const double PageW = 595, PageH = 842, Margin = 56;
    private const double BodySize = 18, DaySize = 22, TitleSize = 24, FooterSize = 14;
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    private sealed record Line(string Text, double Size, bool Bold, double SpaceBefore);

    // Largeurs Helvetica (millièmes d'em) pour ASCII 32..126.
    private static readonly int[] W =
    [
        278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278,
        556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 278, 278, 584, 584, 584, 556,
        1015, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778,
        667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 469, 556,
        333, 556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556,
        556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500, 334, 260, 334, 584,
    ];

    private static double CharWidth(char c, bool bold)
    {
        var w = c is >= ' ' and <= '~' ? W[c - ' '] : c is '—' ? 1000 : c is '–' ? 556 : 556;
        if (c is >= 'À' and <= 'ÿ') w = char.IsUpper(c) ? 700 : 556;   // lettres accentuées
        return w / 1000.0 * (bold ? 1.1 : 1.0);                       // marge de sécurité pour le gras
    }

    private static double TextWidth(string s, double size, bool bold) => s.Sum(c => CharWidth(c, bold)) * size;

    private static IEnumerable<string> Wrap(string text, double size, bool bold, double maxWidth)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = "";
        foreach (var word in words)
        {
            var candidate = current.Length == 0 ? word : current + " " + word;
            if (current.Length > 0 && TextWidth(candidate, size, bold) > maxWidth)
            {
                yield return current;
                current = word;
            }
            else current = candidate;
        }
        if (current.Length > 0) yield return current;
    }

    public static byte[] Build(IReadOnlyList<ScheduleEvent> events, string promotion, DateOnly start, DateOnly end)
    {
        var maxWidth = PageW - 2 * Margin;
        var blocks = new List<List<Line>>();

        var head = new List<Line>
        {
            new($"Planning {promotion}", TitleSize, true, 0),
            new($"Du {start.ToString("d MMMM", Fr)} au {end.ToString("d MMMM yyyy", Fr)}", BodySize, false, 6),
        };
        blocks.Add(head);

        foreach (var day in events.Where(e => e.Date is not null).GroupBy(e => e.Date!.Value))
        {
            var dayTitle = Capitalize(day.Key.ToString("dddd d MMMM yyyy", Fr));
            var firstOfDay = true;
            foreach (var ev in day)
            {
                var block = new List<Line>();
                if (firstOfDay) block.Add(new Line(dayTitle, DaySize, true, 26));
                block.Add(new Line(TimeLabel(ev), BodySize, true, firstOfDay ? 10 : 14));
                foreach (var l in Wrap(TitleBuilder.ExactTitle(ev) + (string.IsNullOrWhiteSpace(ev.Group) ? "" : " — " + ev.Group), BodySize, false, maxWidth))
                    block.Add(new Line(l, BodySize, false, 0));
                if (!string.IsNullOrWhiteSpace(ev.Teacher))
                    foreach (var l in Wrap("Enseignant : " + ev.Teacher, BodySize, false, maxWidth)) block.Add(new Line(l, BodySize, false, 0));
                if (!string.IsNullOrWhiteSpace(ev.Room))
                    foreach (var l in Wrap("Salle : " + ev.Room, BodySize, false, maxWidth)) block.Add(new Line(l, BodySize, false, 0));
                blocks.Add(block);
                firstOfDay = false;
            }
        }

        if (blocks.Count == 1)
        {
            blocks.Add([new Line("Aucun cours à afficher.", BodySize, false, 20)]);
        }

        // Pagination : un bloc (jour + première séance, ou séance) ne se coupe pas.
        var pages = new List<List<(double Y, Line Line)>>();
        var page = new List<(double, Line)>();
        var y = PageH - Margin;
        foreach (var block in blocks)
        {
            var height = block.Sum(l => l.SpaceBefore + l.Size * 1.3);
            if (page.Count > 0 && y - height < Margin + 24)
            {
                pages.Add(page);
                page = [];
                y = PageH - Margin;
            }
            foreach (var l in block)
            {
                y -= l.Size * 1.3 + (page.Count == 0 ? 0 : l.SpaceBefore);
                page.Add((y, l));
            }
        }
        if (page.Count > 0) pages.Add(page);

        return Serialize(pages, $"Planning {promotion} — {start:yyyy-MM-dd} au {end:yyyy-MM-dd}");
    }

    private static string TimeLabel(ScheduleEvent ev) =>
        ev.IsAllDay ? "Journée entière" : $"{ev.StartTime:HH\\hmm} - {ev.EndTime:HH\\hmm}";

    private static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];

    static PdfPlanningWriter() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private static byte[] Serialize(List<List<(double Y, Line Line)>> pages, string title)
    {
        var win = Encoding.GetEncoding(1252, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
        var ms = new MemoryStream();
        var offsets = new List<long>();

        void Raw(string s) { var b = Encoding.Latin1.GetBytes(s); ms.Write(b, 0, b.Length); }
        void Obj(int n, string body) { offsets.Add(ms.Position); Raw($"{n} 0 obj\n{body}\nendobj\n"); }

        static string Esc(byte[] b)
        {
            var sb = new StringBuilder("(");
            foreach (var x in b)
            {
                if (x is (byte)'(' or (byte)')' or (byte)'\\') sb.Append('\\').Append((char)x);
                else sb.Append((char)x);           // Latin1 : un octet = un caractère
            }
            return sb.Append(')').ToString();
        }

        Raw("%PDF-1.4\n");
        // Objets : 1 catalogue, 2 pages, 3 police normale, 4 gras, 5 infos, puis (page, contenu) par page.
        var kids = string.Join(" ", Enumerable.Range(0, pages.Count).Select(i => $"{6 + 2 * i} 0 R"));
        Obj(1, "<< /Type /Catalog /Pages 2 0 R /Lang (fr-FR) /ViewerPreferences << /DisplayDocTitle true >> >>");
        Obj(2, $"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>");
        Obj(3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        Obj(4, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");
        Obj(5, $"<< /Title {Esc(win.GetBytes(title))} /Producer (Planning Converter) >>");

        for (var i = 0; i < pages.Count; i++)
        {
            var content = new MemoryStream();
            void C(string s) { var b = Encoding.Latin1.GetBytes(s); content.Write(b, 0, b.Length); }
            foreach (var (y, line) in pages[i])
            {
                C($"BT /{(line.Bold ? "F2" : "F1")} {line.Size.ToString("0.##", CultureInfo.InvariantCulture)} Tf " +
                  $"{Margin.ToString(CultureInfo.InvariantCulture)} {y.ToString("0.##", CultureInfo.InvariantCulture)} Td ");
                C(Esc(win.GetBytes(line.Text)) + " Tj ET\n");
            }
            C($"BT /F1 {FooterSize} Tf {Margin} 30 Td {Esc(win.GetBytes($"Page {i + 1} sur {pages.Count}"))} Tj ET\n");

            var data = content.ToArray();
            Obj(6 + 2 * i, $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageW} {PageH}] " +
                           $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {7 + 2 * i} 0 R >>");
            offsets.Add(ms.Position);
            Raw($"{7 + 2 * i} 0 obj\n<< /Length {data.Length} >>\nstream\n");
            ms.Write(data, 0, data.Length);
            Raw("\nendstream\nendobj\n");
        }

        var xref = ms.Position;
        Raw($"xref\n0 {offsets.Count + 1}\n0000000000 65535 f \n");
        foreach (var o in offsets) Raw($"{o:D10} 00000 n \n");
        Raw($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R /Info 5 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return ms.ToArray();
    }

    public static bool LooksLikePdf(byte[] bytes)
    {
        var s = Encoding.Latin1.GetString(bytes);
        return s.StartsWith("%PDF-", StringComparison.Ordinal) && s.TrimEnd().EndsWith("%%EOF", StringComparison.Ordinal);
    }
}
