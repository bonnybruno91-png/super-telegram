using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PlanningConverter.Domain;

/// <summary>
/// Produit un UID iCalendar déterministe (§102 : anticiper la mise à jour des calendriers
/// sans duplication). Reconvertir le même planning corrigé doit produire les mêmes UID.
/// </summary>
public static class UidGenerator
{
    private const string Suffix = "@planning-converter.local";

    public static string Generate(string? promotion, DateOnly? date, TimeOnly? startTime, string? title, string? group)
    {
        var normalizedTitle = NormalizeForHash(title);
        var key = string.Join(
            '|',
            promotion?.Trim().ToUpperInvariant() ?? "",
            date?.ToString("yyyy-MM-dd") ?? "",
            startTime?.ToString("HH:mm") ?? "",
            normalizedTitle,
            group?.Trim().ToUpperInvariant() ?? "");

        var bytes = Encoding.UTF8.GetBytes(key);
        var hash = SHA256.HashData(bytes);
        var hex = Convert.ToHexStringLower(hash)[..32];
        return hex + Suffix;
    }

    /// <summary>Minuscules, accents retirés, espaces compressés — pour que deux libellés
    /// équivalents à la casse/accentuation près produisent le même UID.</summary>
    private static string NormalizeForHash(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "";

        var normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        var collapsed = string.Join(' ', sb.ToString().Split(
            (char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return collapsed.Normalize(NormalizationForm.FormC);
    }
}
