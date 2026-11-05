namespace PlanningConverter.Spreadsheet;

public enum DetectedFormat
{
    OpenXml,   // .xlsx — signature ZIP "PK\x03\x04"
    Biff,      // .xls  — signature OLE2 "D0 CF 11 E0 A1 B1 1A E1"
    Unknown    // ni l'un ni l'autre : extension mensongère, HTML exporté, etc. — §131
}

/// <summary>
/// Vérifie le contenu réel du fichier plutôt que de faire confiance à l'extension (§131).
/// Cas fréquent rencontré en établissement : un export d'un logiciel de scolarité nommé
/// ".xls" qui est en réalité du HTML ou du XML — provoque un crash NPOI non explicite
/// si on ne le détecte pas en amont.
/// </summary>
public static class SpreadsheetFormatDetector
{
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] Ole2Signature = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

    public static DetectedFormat Detect(Stream stream)
    {
        Span<byte> header = stackalloc byte[8];
        var originalPosition = stream.Position;
        try
        {
            var read = stream.Read(header);
            if (read >= 4 && header[..4].SequenceEqual(ZipSignature))
            {
                return DetectedFormat.OpenXml;
            }
            if (read >= 8 && header.SequenceEqual(Ole2Signature))
            {
                return DetectedFormat.Biff;
            }
            return DetectedFormat.Unknown;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }
}
