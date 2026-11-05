namespace PlanningConverter.Spreadsheet;

/// <summary>
/// Base des erreurs de lecture. Porte volontairement un message DÉJÀ rédigé pour
/// l'utilisateur final (§83) : la couche UI n'a pas à connaître NPOI pour afficher
/// quelque chose de compréhensible.
/// </summary>
public abstract class SpreadsheetReadException : Exception
{
    public string UserMessage { get; }

    protected SpreadsheetReadException(string userMessage, string technicalMessage, Exception? inner = null)
        : base(technicalMessage, inner)
    {
        UserMessage = userMessage;
    }
}

/// <summary>§85 : classeur protégé ou chiffré.</summary>
public sealed class ProtectedWorkbookException(string technicalMessage, Exception? inner = null)
    : SpreadsheetReadException(
        "Ce fichier semble être protégé ou chiffré.\n\nOuvrez-le dans Excel et enregistrez une copie accessible avant de réessayer.",
        technicalMessage, inner);

/// <summary>§86 : classeur corrompu ou incomplet.</summary>
public sealed class CorruptWorkbookException(string technicalMessage, Exception? inner = null)
    : SpreadsheetReadException(
        "Le fichier Excel semble endommagé ou incomplet.",
        technicalMessage, inner);

/// <summary>Extension .xls/.xlsx mais contenu qui n'est pas réellement un classeur Excel (§131).</summary>
public sealed class UnsupportedWorkbookFormatException(string technicalMessage, Exception? inner = null)
    : SpreadsheetReadException(
        "Ce fichier n'est pas un classeur Excel valide.\n\nFormats acceptés : XLSX et XLS.",
        technicalMessage, inner);

/// <summary>§84 : le fichier est actuellement verrouillé (probablement ouvert dans Excel).</summary>
public sealed class WorkbookLockedException(string technicalMessage, Exception? inner = null)
    : SpreadsheetReadException(
        "Le fichier est actuellement ouvert dans une autre application.\n\nFermez-le puis réessayez.",
        technicalMessage, inner);

/// <summary>§131 : classeur dépassant les limites raisonnables (taille, nombre de cellules).</summary>
public sealed class WorkbookTooLargeException(string technicalMessage, Exception? inner = null)
    : SpreadsheetReadException(
        "Ce fichier est trop volumineux pour être analysé.",
        technicalMessage, inner);
