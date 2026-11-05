using PlanningConverter.Storage;

namespace PlanningConverter.Ui.ViewModels;

/// <summary>Aide : parcours, raccourcis clavier, règles de lecture du planning.</summary>
public sealed class HelpViewModel
{
    public IReadOnlyList<(string Title, string Body)> Sections { get; } =
    [
        ("Convertir un planning",
         "1. Importer : déposez un ou plusieurs fichiers Excel (un par semaine) dans la fenêtre, ou utilisez « Sélectionner des fichiers ».\n" +
         "2. Vérifier : contrôlez la liste des cours, corrigez ou ignorez ce qui doit l'être.\n" +
         "3. Exporter : choisissez les formats et le dossier, puis « Enregistrer »."),
        ("Groupes A et B",
         "Le groupe est lu dans la colonne « Groupe » du fichier. Une cellule vide veut dire que le cours concerne toute la promotion. " +
         "Les codes P1, P2, S1, S3, S4 sont des salles. Dans les fichiers exportés, le groupe termine le titre du cours : « … — Groupe B »."),
        ("Cours à compléter",
         "Un cours sans heure ou sans intitulé est exclu de l'export tant qu'il n'est pas corrigé. Les autres cours sont exportés normalement. " +
         "« Ignorer » retire un cours de l'export sans le supprimer ; « Réintégrer » le remet."),
        ("Plusieurs semaines",
         "Les semaines sont réunies dans l'ordre. Une semaine manquante ou présente deux fois est signalée en haut de l'écran « Vérifier »."),
        ("Clavier",
         "Tab / Maj+Tab : passer d'un élément à l'autre.\nEntrée ou Espace : activer un bouton.\n" +
         "Échap : fermer le panneau de correction.\nEntrée dans un champ du panneau : enregistrer la correction."),
        ("Reprise",
         "Vos corrections sont enregistrées automatiquement. À l'ouverture, « Reprendre la dernière conversion » apparaît sur l'écran d'accueil."),
        ("Formats",
         "ICS : calendrier (Google Agenda, Outlook, iPhone, Android).\nTXT : texte accessible, recommandé avec un lecteur d'écran.\n" +
         "CSV : tableur.\nPDF : gros caractères (18 pt), pour imprimer ; non balisé."),
    ];
}

public sealed class HistoryItem
{
    public required string Title { get; init; }
    public required string Detail { get; init; }
}

/// <summary>Historique des conversions déjà exportées (récentes d'abord).</summary>
public sealed class HistoryViewModel
{
    public IReadOnlyList<HistoryItem> Items { get; }
    public bool IsEmpty => Items.Count == 0;

    public HistoryViewModel(IHistoryStore store)
    {
        var fr = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
        Items = store.ListRecent(50).Select(h => new HistoryItem
        {
            Title = h.PeriodStart is { } s && h.PeriodEnd is { } e
                ? $"{h.Promotion ?? "Planning"} — du {s.ToString("d MMMM", fr)} au {e.ToString("d MMMM yyyy", fr)}"
                : $"{h.Promotion ?? "Planning"} — {h.SourceFileName}",
            Detail = $"Converti le {h.ConvertedAt.LocalDateTime.ToString("d MMMM yyyy 'à' HH:mm", fr)} — " +
                     $"{h.EventCount} cours — formats : {string.Join(", ", h.ExportedFormats).ToUpperInvariant()}",
        }).ToList();
    }
}
