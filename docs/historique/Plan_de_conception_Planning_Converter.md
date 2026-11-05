# Plan de conception — Planning Converter

**Document de planification. Aucun code n'est écrit à ce stade.**
Version 1.0 — établie à partir du cahier des charges en 148 sections.

---

## 0. Comment lire ce plan

Ce document répond à trois questions :

1. **Quoi construire exactement** (sections 1 à 12) : décisions techniques, architecture, modèle de données, algorithmes, écrans.
2. **Dans quel ordre** (section 14) : un lotissement incrémental de 16 lots, chacun avec une définition de « terminé » vérifiable.
3. **Quels sont les pièges** (sections 13, 15, 16) : tests, risques, décisions restant à trancher.

Chaque décision structurante est justifiée par un renvoi au cahier des charges (`§n`).

---

## 1. Les cinq invariants du produit

Tout arbitrage de conception doit être tranché par ces cinq règles, dans cet ordre. Elles sont extraites de §4, §141, §142 et §148.

| # | Invariant | Conséquence concrète sur le code |
|---|-----------|----------------------------------|
| I1 | **Ne jamais inventer** (§4.4, §81) | Toute valeur non lue directement d'une cellule porte un marqueur `Inferred` + une baisse de confiance + une anomalie visible. Aucune durée par défaut sans règle explicite de modèle (§20). |
| I2 | **Accessible ou incomplet** (§142) | Une fonctionnalité non pilotable au clavier et non annoncée par un lecteur d'écran n'est pas livrable. Critère de sortie de chaque lot, pas d'un lot final. |
| I3 | **Local et sans effet de bord** (§4.3, §4.5, §78) | Zéro appel réseau dans le MVP (vérifié par test automatisé). Le classeur source est ouvert en lecture seule, copié en mémoire, jamais réécrit. |
| I4 | **Ne pas reproduire Excel** (§141) | Le modèle interne est une **liste d'événements**, jamais une grille. Aucune vue tableau dans le parcours principal. |
| I5 | **Difficile à mal utiliser** (§148) | Les questions posées à l'utilisateur sont globales et rares, pas unitaires et répétées (voir §6.4 de ce plan : l'ancrage de semaine). |

---

## 2. Décisions techniques structurantes

### 2.1 Tableau de décisions

| Sujet | Décision | Justification | Alternatives écartées |
|-------|----------|---------------|-----------------------|
| Runtime | **.NET 10 (LTS)** | LTS courante, WPF y reçoit encore des investissements actifs (perf, styles Fluent, corrections de compatibilité lecteurs d'écran). Déploiement autonome possible. | .NET 8 : LTS plus ancienne, à retenir seulement si des postes Windows 10 non-LTSC doivent être supportés (→ §16, décision D3). |
| Framework UI | **WPF** | §89 classe l'accessibilité en priorité 1. WPF a l'écosystème UI Automation le plus mature et le plus éprouvé avec NVDA/JAWS/Narrateur, le style est entièrement contrôlable (indispensable pour le contraste et le focus visible §65/§125), et il se déploie en `.exe` autonome sans runtime tiers (§117, §119). | **WinUI 3** : dépendance au Windows App SDK, exigences OS plus élevées, support lecteurs d'écran moins éprouvé sur contrôles personnalisés. **WinForms** : styling insuffisant pour §68. **Electron/web** : contraire à §4.3 et au poids attendu. |
| Lecture Excel | **NPOI** (Apache 2.0) | Seule bibliothèque courante couvrant **à la fois** `.xls` (BIFF/HSSF) et `.xlsx` (OOXML/XSSF), exigés par §7.1, sans installation d'Excel (§90). Expose les régions fusionnées et les valeurs mises en cache des formules (§13, §14). | **ClosedXML** : `.xlsx` uniquement. **ExcelDataReader** : lecture séquentielle, accès aux fusions plus pauvre. **Interop Excel** : interdit par §90. |
| Génération ICS | **Ical.Net** (MIT) | Gère le pliage de lignes à 75 octets, l'échappement RFC 5545, les `VTIMEZONE`. Écrire un générateur maison est le premier endroit où l'on casse Apple Calendrier. | Génération manuelle : à ne faire qu'en dernier recours, avec une suite de tests de conformité complète. |
| Génération CSV | **CsvHelper** + profils | Trois profils imposés par §40 avec des règles incompatibles entre elles (séparateur, format de date, BOM). | Concaténation maison : source classique de bugs sur les guillemets et les accents. |
| Génération XLSX accessible | **ClosedXML** | Écriture seulement, API simple, produit un fichier propre et non fusionné (§8.4). | NPOI en écriture : possible, API plus verbeuse. |
| MVVM | **CommunityToolkit.Mvvm** | Générateurs de source, pas de framework lourd, testabilité des ViewModels. | Prism, ReactiveUI : surdimensionnés. |
| Stockage | `settings.json`, `templates.json` (§91) ; **historique en SQLite** via `Microsoft.Data.Sqlite` | §92 décrit des enregistrements interrogeables (détection de doublons d'import §100, régénération §52). SQLite reste un fichier unique, compatible version portable (§119). | Historique JSON : suffisant au démarrage, mais migration pénible ; on l'évite en posant l'interface `IHistoryStore` dès le lot 1. |
| Journalisation | **Serilog**, sink fichier roulant, dossier `%LOCALAPPDATA%` | §82, §128. Un filtre de rédaction supprime noms d'enseignants et contenus de cellules des logs. | — |
| Tests | **xUnit** + **Verify** (tests d'instantané pour les fichiers générés) | §109, §113 : les exports se testent par comparaison à des fichiers de référence. | — |
| Installateur | **Inno Setup**, installation par utilisateur (sans droits admin) + **zip portable** | §117, §119, §120. | MSIX : signature et déploiement plus contraignants en milieu universitaire. |

### 2.2 Contraintes non négociables du build

- Publication **self-contained, single-file, win-x64** : aucun prérequis à installer (§117).
- `InvariantGlobalization = false` (les noms de mois français sont nécessaires).
- Aucun package effectuant des appels réseau ; test d'architecture interdisant `System.Net.Http` dans toutes les couches sauf un futur module IA (§79, §80).
- Nom du produit centralisé dans une seule constante et les ressources de localisation (§5 : le nom doit pouvoir changer sans refonte).

---

## 3. Architecture logicielle

### 3.1 Découpage en projets

Traduction directe de §87 et §88 en solution .NET :

```
PlanningConverter.sln
├── src/
│   ├── PlanningConverter.Domain            (0 dépendance externe)
│   ├── PlanningConverter.Spreadsheet       (lecture Excel → CellGrid)
│   ├── PlanningConverter.Parsing           (moteur d'analyse)
│   ├── PlanningConverter.Validation        (règles, anomalies, statuts)
│   ├── PlanningConverter.Export            (ICS, CSV, TXT, XLSX)
│   ├── PlanningConverter.Storage           (paramètres, modèles, historique, autosave)
│   ├── PlanningConverter.Application       (orchestration, cas d'usage)
│   └── PlanningConverter.Ui                (WPF, MVVM)
└── tests/
    ├── PlanningConverter.Domain.Tests
    ├── PlanningConverter.Parsing.Tests
    ├── PlanningConverter.Export.Tests
    ├── PlanningConverter.Integration.Tests (corpus de fichiers réels anonymisés)
    └── PlanningConverter.Architecture.Tests (règles de dépendance, interdiction réseau)
```

### 3.2 Règles de dépendance (vérifiées par test)

- `Domain` ne référence **rien**. Pas d'I/O, pas de NPOI, pas de WPF.
- `Parsing` dépend de `Domain` et `Spreadsheet`, jamais de `Ui`.
- `Ui` ne référence **jamais** `Spreadsheet`, `Parsing` ni `Export` directement : uniquement `Application`.
- Aucune classe ne référence NPOI hors de `Spreadsheet` (permet de brancher un jour un lecteur PDF/OCR sans toucher au reste, §7.2, §80).

### 3.3 Flux nominal

```
Fichier .xlsx/.xls
  → ISpreadsheetReader          (Spreadsheet)  → Workbook / CellGrid
  → SheetTriage                 (Parsing)      → feuilles candidates
  → AnalysisPipeline            (Parsing)      → IReadOnlyList<ScheduleEvent> + AnalysisTrace
  → ValidationEngine            (Validation)   → statuts + anomalies + confiance
  → ConversionSession           (Application)  → état éditable, undo, autosave
  → IExporter[]                 (Export)       → fichiers vérifiés sur disque
  → IHistoryStore               (Storage)      → entrée d'historique
```

**Point clé :** `ConversionSession` est l'unique objet mutable de la chaîne. Le pipeline d'analyse est composé de fonctions pures ; c'est ce qui le rend testable ligne par ligne et remplaçable (§88).

---

## 4. Modèle de domaine

### 4.1 Représentation de la source

Nécessaire pour §94 (conservation de la source) et §95 (aperçu des cellules d'origine).

```csharp
public sealed record SourceCell(
    string SheetName,
    int Row, int Column,          // 0-based interne
    string A1Reference,           // "C17", affiché à l'utilisateur
    CellValueKind Kind,           // Text, Number, Date, Time, Formula, Empty
    string RawText,               // texte tel qu'affiché dans Excel
    object? TypedValue,
    bool IsMergedAnchor,
    MergedRegion? Merge);

public enum CellValueKind { Empty, Text, Number, Date, Time, Formula, Error }
```

`CellGrid` expose `Cell(row, col)` en résolvant les fusions : toute cellule d'une région fusionnée renvoie la valeur de son ancre, avec `IsMergedAnchor = false` (§14).

### 4.2 L'événement

Reprend intégralement §25, avec trois ajouts nécessaires aux exigences §27, §94 et §102.

```csharp
public sealed record ScheduleEvent
{
    public required Guid Id { get; init; }
    public required string Uid { get; init; }          // UID iCalendar stable — §102

    // Provenance — §94
    public required string SourceFile { get; init; }
    public required string SourceSheet { get; init; }
    public required int SourceRow { get; init; }
    public required IReadOnlyList<SourceCell> SourceCells { get; init; }
    public required string OriginalContent { get; init; }

    // Données du planning — §25
    public DateOnly? Date { get; init; }
    public DayOfWeek? Day { get; init; }
    public TimeOnly? StartTime { get; init; }
    public TimeOnly? EndTime { get; init; }
    public bool IsAllDay { get; init; }                 // §57
    public string? Title { get; init; }
    public string? Teacher { get; init; }
    public string? Room { get; init; }
    public string? Location { get; init; }
    public string? Ue { get; init; }
    public string? Group { get; init; }                 // §97
    public string? Promotion { get; init; }             // §96
    public string? Notes { get; init; }

    // Qualité — §26, §27, §81
    public EventKind Kind { get; init; }                // Course, Exam, Break, Closure, Other — §23, §24
    public EventStatus Status { get; init; }            // Valid, Warning, Error, Ignored
    public double Confidence { get; init; }
    public required IReadOnlyList<EventIssue> Issues { get; init; }
    public required IReadOnlySet<string> InferredFields { get; init; } // I1 : traçabilité des déductions
    public bool EditedByUser { get; init; }
}
```

**`InferredFields`** est la matérialisation de l'invariant I1 : tout champ qui n'a pas été lu tel quel y figure, et l'interface peut alors afficher « déduit » à côté de la valeur.

### 4.3 UID stables (§102) — à décider maintenant, pas plus tard

Le cahier des charges demande d'anticiper la mise à jour des calendriers sans duplication. La seule façon de ne pas payer cette dette plus tard est de rendre l'UID **déterministe dès la version 1** :

```
uid = SHA256( promotion | date ISO | heure début | titre normalisé | groupe )
      → 32 caractères hexadécimaux + "@planning-converter.local"
```

Titre normalisé = minuscules, accents retirés, espaces compressés. Conséquence : reconvertir le même planning corrigé produit les mêmes UID, donc les calendriers se mettent à jour au lieu de se dupliquer, dès qu'on incrémentera `SEQUENCE` en v2.

### 4.4 Anomalies

```csharp
public enum IssueCode
{
    MissingDate, MissingStartTime, MissingEndTime, MissingTitle,   // bloquantes — §56
    EndBeforeStart, InvalidDate, InvalidTime,                      // bloquantes — §55
    YearInferred, WeekAnchorInferred, DateFromDayName,             // avertissements — §17
    Overlap, Duplicate,                                            // avertissements — §53, §54
    LowConfidence, UnmappedColumn, BreakDetected, RowIgnored       // informations — §22, §23
}
```

Chaque `EventIssue` porte : code, sévérité, message utilisateur en français clair (§83, §136), champ concerné (pour placer le message près du champ, §126), et éventuellement une valeur suggérée.

---

## 5. Le moteur d'analyse

C'est le cœur du projet et la principale source de risque. Il est découpé en **onze étages** correspondant au pseudo-processus de §93. Chaque étage est une classe autonome, sans état, avec ses propres tests, et enregistre une entrée dans un `AnalysisTrace` (base du diagnostic §128 et de l'écran « voir les données d'origine » §95).

### Étage 0 — Ouverture du classeur
Détection réelle du format par signature binaire, pas seulement par extension (§131) : `PK\x03\x04` → OOXML, `D0 CF 11 E0` → BIFF. Cas fréquent à gérer : fichier nommé `.xls` qui est en réalité du HTML ou du XML exporté par un logiciel de scolarité → message dédié plutôt que crash. Aucune macro n'est chargée ni évaluée (§130). Limites : taille de fichier et nombre de cellules plafonnés (§115, §131).

### Étage 1 — Tri des feuilles (§12)
Chaque feuille reçoit un **score de « planningité » 0–100**, combinant :
- densité de cellules non vides,
- nombre de noms de jours reconnus,
- nombre de motifs horaires reconnus,
- présence d'en-têtes du lexique (§21),
- pénalité si la feuille ressemble à une légende ou une notice.

Résultat : `score ≥ 40` → feuille candidate. Une seule candidate → sélection automatique. Plusieurs → écran de sélection multiple (§12). Zéro → message explicite de §83.

### Étage 2 — Normalisation de la grille
Résolution des fusions, découpe de la plage réellement utilisée, typage des cellules, lecture des **valeurs mises en cache des formules** (§13), nettoyage des espaces insécables et des retours à la ligne internes.

### Étage 3 — Détection des régions de données
Un classeur contient souvent titre, logo, tableau, légende, mentions d'impression. On segmente la feuille en blocs denses séparés par des lignes/colonnes vides. Les blocs trop petits ou constitués d'une seule cellule longue sont classés « non pertinents » (§22) — **mais conservés dans la trace, jamais supprimés silencieusement**.

### Étage 4 — Classification de la disposition
Deux dispositions réelles existent dans les plannings universitaires :

- **A — orientée lignes** : un événement par ligne, les colonnes sont les champs. C'est la disposition illustrée par le cahier des charges (§15, §21, §50).
- **B — orientée grille** : les jours en colonnes, les créneaux horaires en lignes, les cours dans les cellules. Très fréquente dans les emplois du temps imprimables.

Discriminant : proportion de noms de jours en ligne 1 (→ B) contre en colonne A (→ A) ; proportion de motifs horaires en colonne A (→ B) contre répartis (→ A). Résultat `RowOriented`, `ColumnOriented` ou `Unknown`.

> **Décision de périmètre proposée :** le MVP traite **A** de bout en bout, **détecte B** et le traite avec un extracteur simplifié (cellule = titre, ligne = créneau, colonne = jour), et bascule sur l'éditeur de modèle (§50) si la confiance reste faible. `Unknown` n'échoue jamais : il propose l'éditeur de modèle. Cette décision doit être confirmée après examen de fichiers réels (voir §16, D1).

### Étage 5 — Cartographie des colonnes (§21)
Deux mécanismes complémentaires :
1. **Par en-tête** : lexique de synonymes de §21, comparaison insensible à la casse et aux accents, tolérance aux fautes légères (distance de Levenshtein ≤ 1). Confiance élevée.
2. **Par contenu**, si aucun en-tête n'est trouvé : une colonne dont > 60 % des cellules correspondent à des motifs horaires est la colonne horaire ; une colonne contenant `Dr`, `M.`, `Mme`, `Pr` est la colonne enseignant ; une colonne contenant `salle`, `amphi`, `A204`, `B12` est la colonne salle. Confiance réduite, anomalie `UnmappedColumn` si aucune règle ne tranche.

Le lexique vit dans un **fichier de ressources externe**, pas en dur : le secrétariat pourra l'enrichir sans recompilation.

### Étage 6 — Propagation du contexte (§14, §15)
Règle de propagation **gardée**, jamais générale :
- Propageables : jour, date, promotion, groupe, UE.
- **Jamais propageables** : horaire, intitulé, enseignant, salle. Un intitulé absent reste absent (I1).
- La propagation s'arrête à une ligne entièrement vide, à un changement de région, ou à l'apparition d'une nouvelle valeur.
- Toute valeur propagée est marquée dans `InferredFields` et visible dans l'aperçu de la source (§95).

### Étage 7 — Construction des événements candidats
Une ligne (disposition A) ou une cellule non vide de la grille (disposition B) devient un candidat. Les lignes classées non pertinentes deviennent des candidats `Ignored` avec le motif — visibles dans un repli « 4 lignes ignorées — afficher » plutôt que supprimées.

### Étage 8 — Normalisation des champs
Détaillée en section 6 ci-dessous.

### Étage 9 — Enrichissement
Détection de la promotion (§96) via expression régulière `\b(MK|L|M|BUT|DUT)[1-5]\b` recherchée dans les cellules, le nom de feuille et le nom de fichier. Détection des groupes (§97). Classification `EventKind` : pauses (§23), examens, soutenances, vacances, fermetures (§24) par lexique. Les pauses deviennent `Ignored` par défaut, paramètre pour les inclure (§23).

### Étage 10 — Validation et statut
Voir section 7.

### Étage 11 — Agrégation
Calcul de la période couverte (« semaine du 14 au 18 septembre 2026 »), des compteurs de §28, du nom de fichier proposé (§43).

---

## 6. Normalisation des dates et des horaires

C'est le module qui concentrera le plus de tests unitaires. Il est **purement fonctionnel** : `string → résultat typé + confiance + anomalie`.

### 6.1 Jours (§16)
Table de correspondance couvrant : forme complète, forme abrégée avec ou sans point, majuscules, minuscules, accents supprimés. `LUNDI`, `Lundi`, `lundi`, `Lun.`, `LUN`, `lun` → `DayOfWeek.Monday`.

### 6.2 Heures (§18)
Grammaire ordonnée, la première correspondance gagne :

| Motif | Exemple | Note |
|-------|---------|------|
| `H{1,2}[:hH.]M{2}` | `08:30`, `8h30`, `08H30`, `8.30` | cas dominant |
| `H{1,2}\s*[hH]\s*M{2}` | `8 H 30` | espaces tolérés |
| `HHMM` (4 chiffres) | `0830` | seulement si valeur ≤ 2359 et cohérente |
| `H{1,2}\s*[hH]` | `14h` | minutes = 00 |
| Heure Excel native | valeur numérique typée | confiance maximale |

Refus explicite (`InvalidTime`) au-delà de 23:59 et pour les valeurs à minutes ≥ 60.

### 6.3 Plages horaires (§19)
Séparateurs acceptés : `-`, `–` (tiret demi-cadratin), `—`, `/`, `à`, `au`, `>`, `jusqu'à`, avec ou sans espaces. Découpage puis normalisation indépendante de chaque borne.
Cas particulier : `8h30–10h` — la deuxième borne n'a pas de minutes, elle est valide.
Cas particulier : fin < début (ex. `22h00-01h00`) → **pas** de report automatique au lendemain, anomalie `EndBeforeStart` et validation humaine (I1, §55).

### 6.4 Dates (§17) et **ancrage de la semaine**

Formats reconnus : `14/09/2026`, `14-09-2026`, `14.09.2026`, `14 septembre 2026`, `Lundi 14 septembre`, `14/09`, `14 sept.`, plus les dates Excel natives.

Le cas le plus important est celui du **critère d'acceptation final §147** : la cellule contient seulement `Lundi`, et il faut produire une date. Chaîne de résolution :

1. Une date explicite existe dans la même ligne ou la même région → utilisée. Confiance pleine.
2. Sinon, recherche d'un **contexte de semaine** dans : les cellules d'en-tête (`Semaine du 14 au 18 septembre 2026`, `Semaine 38`), le nom de la feuille, le nom du fichier. Le lundi de cette semaine sert d'origine, chaque nom de jour donne un décalage. Anomalie `WeekAnchorInferred`, confiance réduite.
3. Sinon, **une seule question globale** est posée à l'utilisateur : « De quelle semaine s'agit-il ? » avec un sélecteur de date. Elle résout tous les événements d'un coup.

> C'est un choix d'expérience utilisateur délibéré, dicté par I5 : sans lui, un planning de 28 événements sans dates explicites produirait 28 erreurs individuelles. Il en produit une seule.

**Inférence de l'année manquante** (§17) : si le jour de la semaine est écrit et que la date ne porte pas d'année, on retient l'année où le jour de semaine correspond effectivement, dans une fenêtre de ±1 an autour de l'année universitaire courante. Ambiguïté persistante → question globale unique, `YearInferred`.

### 6.5 Heure de fin absente (§20)
Aucune durée n'est inventée. `MissingEndTime`, statut `Error`, l'événement remonte dans l'écran de vérification. **Sauf** si un modèle enregistré (§48) définit une règle de durée par défaut : dans ce cas la valeur est appliquée, marquée `Inferred`, et le statut est `Warning`, jamais `Valid`.

---

## 7. Validation, statuts et confiance

### 7.1 Calcul de la confiance (§27)

Départ à 1.00, application de pénalités cumulatives :

| Situation | Pénalité |
|-----------|----------|
| Colonne identifiée par contenu et non par en-tête | −0.15 |
| Date déduite d'un nom de jour + ancrage de semaine | −0.15 |
| Année inférée | −0.10 |
| Valeur héritée par propagation | −0.05 par champ |
| Heure de fin issue d'une règle de modèle | −0.25 |
| Disposition classée `Unknown` | −0.20 |
| Cellule contenant plusieurs informations mal séparées | −0.20 |

Seuils : `≥ 0.85` → `Valid` ; `0.50–0.85` → `Warning` ; `< 0.50` → `Error`.
Toute anomalie bloquante force `Error` quelle que soit la confiance.
Le score n'est **pas affiché** (§27) ; il pilote seulement le tri et le statut.

### 7.2 Règles de validation

| Règle | Code | Effet | Réf. |
|-------|------|-------|------|
| Date, début, fin et titre présents | `Missing*` | Bloquant | §56 |
| Fin postérieure au début | `EndBeforeStart` | Bloquant | §55 |
| Date et heure syntaxiquement valides | `InvalidDate/Time` | Bloquant | §55 |
| Doublon (date + début + fin + titre) | `Duplicate` | Avertissement + choix « Conserver les deux / un seul / Examiner » | §53 |
| Chevauchement de créneaux | `Overlap` | Avertissement seul, **aucune suppression automatique** | §54 |
| Journée entière | — | Autorisée seulement si mention explicite | §57 |

### 7.3 Contrat de sortie de l'analyse

L'écran de résultat (§28) est alimenté par un objet unique :
`AnalysisResult { PlanningName, PeriodStart, PeriodEnd, Events, TotalCount, ValidCount, WarningCount, ErrorCount, IgnoredCount, Trace }`.

---

## 8. Exportateurs

Interface commune : `IExporter { string Id; string DisplayName; Task<ExportedFile> ExportAsync(ExportRequest, CancellationToken) }`.
**Règle §146 :** aucun export n'est déclaré réussi avant relecture du fichier écrit sur disque et vérification qu'il est non vide et parsable.

### 8.1 ICS (§36, §37, §113)

Pièges à traiter explicitement, chacun couvert par un test :

- Fins de ligne **CRLF** obligatoires ; pliage à 75 octets (et non 75 caractères — les accents comptent double en UTF-8).
- Échappement de `\`, `;`, `,` et des retours à la ligne (`\n`) dans `SUMMARY`, `DESCRIPTION`, `LOCATION`.
- **`VTIMEZONE` Europe/Paris embarqué dans le fichier**, pas seulement un `TZID` référencé : Apple Calendrier et certains clients Outlook refusent ou décalent les événements sinon. Les transitions heure d'été/hiver en découlent (§37).
- `UID` stable (§4.3 de ce plan), `DTSTAMP`, `SEQUENCE:0`, `PRODID` identifiant le logiciel, `VERSION:2.0`, `CALSCALE:GREGORIAN`, `METHOD:PUBLISH`, `X-WR-CALNAME` avec le nom du planning.
- Encodage **UTF-8 sans BOM** (le BOM casse certains analyseurs iCalendar).
- Titre selon le format configuré (§38) : `Simple`, `Cours — Enseignant` (défaut), `UE — Cours`, personnalisé.
- Description construite ligne à ligne, **en omettant les champs absents** (§39).

### 8.2 CSV (§40) — trois profils

| Profil | Séparateur | Format de date | Encodage | Usage |
|--------|-----------|----------------|----------|-------|
| Générique | `;` | `AAAA-MM-JJ` | UTF-8 **avec BOM** | ouverture directe dans Excel français |
| Google Agenda | `,` | `MM/JJ/AAAA` | UTF-8 sans BOM | import Google Agenda, colonnes `Subject, Start Date, Start Time, End Date, End Time, All Day Event, Description, Location, Private` |
| Personnalisé | configurable | configurable | configurable | §40 |

Le format de date américain du profil Google est une contrainte de l'importateur, pas un choix : c'est le genre de détail qui fait échouer silencieusement un import et qu'il faut figer par un test.

### 8.3 Texte accessible (§41)
Deux formats, détaillé et compact, exactement conformes aux exemples du cahier des charges. Regroupement par jour, jour en clair (`Lundi 14 septembre 2026`), tri chronologique, UTF-8, lignes courtes.
Ce format alimente aussi la copie presse-papiers (§42) et le corps de l'e-mail (§46).

### 8.4 Excel accessible (§8.4)
Une ligne = un événement, aucune fusion, en-têtes explicites sur la première ligne figée, largeurs de colonnes ajustées, aucune information portée uniquement par une couleur, statut écrit en toutes lettres. Colonnes exactement dans l'ordre de §8.4.

### 8.5 Nommage et destination (§43, §44)
`Planning_{Promotion}_{DateDebut}_{DateFin}.{ext}`, caractères Windows interdits remplacés, accents translittérés, longueur de chemin surveillée. Dossier par défaut `Documents\Plannings`, dernier dossier mémorisé. Collision de nom → suffixe ` (2)`, jamais d'écrasement silencieux.

---

## 9. Persistance

| Élément | Emplacement | Format |
|---------|------------|--------|
| Paramètres | `%APPDATA%\PlanningConverter\settings.json` | JSON versionné (`schemaVersion`) |
| Modèles | `%APPDATA%\PlanningConverter\templates.json` + export unitaire `*.template.json` (§132) | JSON |
| Historique | `%LOCALAPPDATA%\PlanningConverter\history.db` | SQLite, schéma de §92 |
| Reprise après incident | `%LOCALAPPDATA%\PlanningConverter\autosave\{sessionId}.json` | JSON, écrit à chaque modification, purgé à la fin (§105) |
| Journaux | `%LOCALAPPDATA%\PlanningConverter\logs\` | fichiers roulants 7 jours (§82) |

En **mode portable**, tous ces chemins basculent dans un sous-dossier `data\` à côté de l'exécutable (§119).

L'historique **ne conserve pas les fichiers Excel sources** (§52) : seulement les métadonnées et une empreinte du fichier, qui sert à détecter la reconversion d'un même planning (§100).

---

## 10. Interface utilisateur

### 10.1 Structure générale (§121, §124)

Une seule fenêtre, navigation interne, très peu de fenêtres modales.

```
┌──────────────────────────────────────────────────────┐
│  Planning Converter                          [_][□][X]│
├───────────┬──────────────────────────────────────────┤
│ Accueil   │                                          │
│ Historique│        zone de contenu (une vue)         │
│ Modèles   │                                          │
│ Paramètres│                                          │
│ Aide      │                                          │
├───────────┴──────────────────────────────────────────┤
│  barre d'état / messages (région live)               │
└──────────────────────────────────────────────────────┘
```

Le rail de navigation garde **toujours des libellés textuels** à côté des icônes (§63) et se replie en dessous de 900 px de large (§123).
Modales réservées à : confirmation de suppression destructrice (§104), modifications non enregistrées (§127), doublons (§53).

### 10.2 Les vues

| # | Vue | Rôle | Réf. |
|---|-----|------|------|
| V1 | Accueil | dépôt / sélection de fichier | §10 |
| V2 | Sélection de feuille | seulement si plusieurs candidates | §12 |
| V3 | Analyse en cours | progression non factice | §11, §116 |
| V4 | Aperçu | liste verticale d'événements groupés par jour | §29, §30 |
| V5 | Vérification | uniquement les éléments à corriger, navigation « précédent / suivant », compteur « 2 sur 4 » | §32, §33 |
| V6 | Édition d'un événement | formulaire de §31, validation près du champ | §31, §126 |
| V7 | Export | cases à cocher des formats + dossier de destination | §35, §44 |
| V8 | Terminé | compteurs + 4 actions | §45, §46 |
| V9 | Historique | §51, §52 |
| V10 | Modèles + éditeur | §47–§50 |
| V11 | Paramètres (6 onglets) | §71–§74 |
| V12 | Aide + première ouverture | §107, §108 |

L'indicateur d'étapes **Importer / Analyser / Vérifier / Exporter / Terminé** (§9) est masqué sur V1 et affiché de V3 à V8, avec l'étape courante annoncée aux lecteurs d'écran.

### 10.3 Écran d'accueil — direction artistique

Aucune image de référence n'ayant été fournie, voici la proposition de départ. Elle vise l'immersion **sans sacrifier l'accessibilité**, ce qui exclut les fonds photographiques, les textes sur image et les animations décoratives.

**Principe : la fenêtre entière est la zone de dépôt.** Pas de petit rectangle en pointillés au centre — une cible de 60 px de haut est un échec d'utilisabilité pour quiconque a un tremblement ou une faible vision. Toute la zone de contenu accepte le fichier.

- **Composition** : titre `Convertir un planning` en très grande taille (32–40 px, Segoe UI Variable Display), sous-titre `Déposez votre fichier Excel ici`, bouton principal `Sélectionner un fichier` (min. 44 × 44 px, §66), puis en gris secondaire `Formats acceptés : XLSX et XLS`. Beaucoup d'air : marge minimale de 48 px, ligne de contenu limitée à ~640 px de large.
- **Profondeur** : un dégradé très doux en arrière-plan et **une seule** carte surélevée (ombre légère, coins arrondis 12 px) portant l'action principale. C'est la seule surface élevée de l'écran, elle attire donc l'œil sans compétition.
- **Réaction au survol de fichier** : bordure d'accent de 4 px sur toute la fenêtre + élévation de la carte + libellé qui change en `Déposer pour analyser`. Le changement est **annoncé** en plus d'être visible.
- **Après dépôt** : nom du fichier, taille, et bouton `Analyser` qui reçoit le focus automatiquement (§10.4).
- **Mouvement** : transitions de 150–200 ms maximum, uniquement sur l'opacité et la position. Désactivées si Windows signale la réduction des animations ou si le paramètre correspondant est actif (§74).
- **Jetons de couleur** (thème clair, contrastes vérifiés) : fond `#F7F8FA`, surface `#FFFFFF`, texte principal `#1A1D21` (≈ 15:1), texte secondaire `#4A5058` (≈ 8:1), accent `#1B5FBF` (≈ 6:1 sur blanc), succès `#146C43`, avertissement `#8A5300`, erreur `#B32020`. Chaque état est **toujours doublé d'un mot** (§65).
- **Focus clavier** : anneau de 2 px, décalé de 1 px, contraste ≥ 3:1, jamais supprimé, visible aussi sur la grande zone de dépôt (§125).
- **Mode sombre** : jeu de jetons symétrique, aligné sur le thème Windows, prévu structurellement dès le départ mais activable en v1.1 (§69).

### 10.4 Vue d'aperçu — le choix de contrôle qui compte

**Ne pas utiliser de `DataGrid`.** Une grille est laborieuse à parcourir au lecteur d'écran (annonce cellule par cellule) et contredit §141 : le planning n'est pas un tableau, c'est une succession d'événements.

Structure retenue : un `ItemsControl` groupé par jour, chaque jour introduit par un en-tête de niveau 2, chaque événement étant **un seul élément focalisable** dont le nom accessible est composé :

> « Lundi 14 septembre, 8 h 30 à 10 h 30, Neurologie, Dr Martin, salle 204. Statut : à vérifier. »

Navigation : flèches haut/bas entre les événements, `Entrée` pour modifier, `Suppr` pour supprimer (avec annulation, §103), touche Applications pour le menu contextuel `Modifier / Supprimer / Ignorer / Dupliquer` (§30). Tabulation : le groupe de la liste est un seul arrêt de tabulation, on entre dedans avec les flèches — c'est le comportement attendu par les utilisateurs de lecteurs d'écran, et il évite 100 arrêts de tabulation sur un planning chargé.

---

## 11. Plan d'accessibilité (§58–§67, §142)

L'accessibilité n'est pas un lot final ; elle est un **critère de sortie de chaque lot d'interface**.

### 11.1 Contrat par écran
Chaque vue doit documenter, dans son fichier de code, quatre points :
1. l'ordre de tabulation attendu (§60),
2. l'élément recevant le focus à l'entrée dans la vue,
3. les annonces de région live déclenchées,
4. le comportement d'`Échap`.

### 11.2 Règles techniques
- `AutomationProperties.Name` explicite sur **tout** contrôle interactif (§62), vérifié par un test automatisé parcourant l'arbre visuel.
- Titres de sections via `AutomationProperties.HeadingLevel` : la navigation par titres est le principal moyen de repérage d'un utilisateur de NVDA.
- Régions live : `AutomationProperties.LiveSetting = Polite` sur la barre d'état, avec émission explicite de `LiveRegionChanged` pour annoncer par exemple « Analyse terminée. 28 événements détectés. 2 éléments à vérifier. » (§64).
- Gestion du focus à la navigation : le focus va sur le titre de la nouvelle vue, jamais perdu à la racine de la fenêtre.
- Jamais d'information portée uniquement par la couleur : chaque statut a un mot et une forme (§65).
- Mise à l'échelle : aucune hauteur fixe sur les conteneurs de texte, tout en `Auto` / `*`, vérifié à 100 %, 125 %, 150 %, 200 % (§67, §111).
- Raccourcis de §106 déclarés comme `InputBindings` et listés dans l'aide (§107).

### 11.3 Critère d'acceptation clavier (§112)
Un scénario de test manuel, rejoué à chaque livraison de lot : lancer, sélectionner un fichier, analyser, parcourir, corriger, exporter, ouvrir le dossier — **sans souris**, puis **écran éteint avec NVDA**.

---

## 12. Robustesse, erreurs, sécurité

### 12.1 Table de correspondance exception → message (§83, §84, §85, §86)

| Cause technique | Message utilisateur | Action proposée |
|-----------------|--------------------|-----------------|
| Fichier verrouillé par Excel | Le fichier est actuellement ouvert dans Excel. | Réessayer / Continuer en lecture seule |
| Classeur protégé ou chiffré | Ce fichier semble être protégé ou chiffré. | Instruction de §85 |
| Archive OOXML invalide | Le fichier Excel semble endommagé ou incomplet. | Choisir un autre fichier |
| Extension mensongère | Ce fichier n'est pas un vrai classeur Excel. | Formats acceptés |
| Aucune donnée exploitable | La feuille sélectionnée ne contient pas suffisamment d'informations pour identifier un planning. | Choisir une autre feuille / éditeur de modèle |
| Écriture impossible | Les fichiers n'ont pas pu être enregistrés dans ce dossier. | Changer de dossier |

Chaque message dispose d'un bouton `Afficher les détails techniques` replié par défaut (§83). Aucune exception non gérée ne doit fermer l'application (§145) : gestionnaire global sur `DispatcherUnhandledException`, `UnobservedTaskException` et `AppDomain.UnhandledException`, qui journalise et affiche une vue de récupération.

### 12.2 Sécurité (§130, §131)
Aucune macro chargée ni exécutée, aucun contenu actif évalué. Validation de l'extension **et** de la signature binaire. Plafonds de taille de fichier, de taux de décompression (protection contre les archives piégées) et de nombre de cellules. Aucun accès réseau. Journaux rédigés sans contenu de cellules ni noms de personnes (§82).

---

## 13. Stratégie de tests

### 13.1 Tests unitaires (§109)
Priorité absolue sur les modules purs : normalisation des jours, heures, plages, dates, inférence d'année, ancrage de semaine, propagation des fusions, cartographie des en-têtes, détection des doublons, détection des chevauchements, calcul de confiance. Objectif : **couverture ≥ 90 % sur `Parsing` et `Domain`**, sans objectif chiffré sur l'UI.

### 13.2 Corpus de fichiers réels (§110)
À constituer **avant** le lot 4. Neuf cas minimum, anonymisés, chacun accompagné d'un fichier attendu :

| Cas | Ce qu'il valide |
|-----|-----------------|
| Tableau simple, une feuille | chemin nominal |
| Cellules fusionnées verticalement | §14 |
| Valeurs implicites non répétées | §15 |
| Plusieurs feuilles dont une non-planning | §12 |
| Horaires et cours dans une même cellule | découpage |
| Dates uniquement dans les en-têtes | ancrage de semaine |
| Champs manquants (fin absente, titre absent) | §20, §56 |
| Doublons et chevauchements | §53, §54 |
| Planning très irrégulier | dégradation propre, aucune invention |

### 13.3 Tests d'export (§113, §114)
Tests d'instantané sur chaque fichier généré. Re-lecture des ICS par un analyseur iCalendar indépendant. Chaîne de caractères de torture systématiquement injectée dans un test : `Éducation`, `Rhumatologie`, `M. N'Guessan`, `Amphithéâtre`, `Ça`, `Œ`, plus un titre contenant `;`, `,` et un retour à la ligne.
Matrice d'import manuelle avant chaque version : Outlook bureau, Outlook web, Google Agenda, Apple Calendrier (iPhone).

### 13.4 Tests d'accessibilité (§111)
Accessibility Insights for Windows (contrôles automatisés), puis passage manuel Narrateur + NVDA, mise à l'échelle 125 / 150 / 200 %, contraste élevé Windows, navigation clavier seule.

### 13.5 Performance (§115)
Budget : planning hebdomadaire (≤ 500 lignes) analysé en **moins d'une seconde** ; classeur de 50 000 cellules en moins de cinq secondes ; interface jamais bloquée (analyse sur tâche d'arrière-plan, annulable). Progression réelle par feuille, jamais de pourcentage factice (§116).

---

## 14. Lotissement

Suit l'ordre imposé par §143, avec un jalon démontrable dès le lot 9. Les estimations supposent un développeur assisté par IA ; elles sont indicatives.

| Lot | Contenu | Définition de « terminé » | Est. |
|-----|---------|---------------------------|------|
| **L0** | Socle : solution, projets, conventions, CI, tests d'architecture | La CI compile et lance les tests ; les règles de dépendance échouent si violées | 1 j |
| **L1** | Modèle de domaine, UID stables, statuts, anomalies | Tests unitaires du domaine verts, UID déterministe prouvé | 1–2 j |
| **L2** | Lecture Excel → `CellGrid` (xlsx + xls, fusions, types, formules en cache) | Les 9 fichiers du corpus se chargent sans exception ; fusions correctement résolues | 3 j |
| **L3** | Normalisation jours / heures / plages / dates / année / ancrage semaine | Tous les exemples de §16–§20 passent, plus les cas d'échec attendus | 3–4 j |
| **L4** | Détection des régions, classification de disposition, cartographie des en-têtes | Disposition A correctement identifiée sur le corpus ; B détectée | 4 j |
| **L5** | Construction des événements, propagation gardée, promotion / groupes / pauses | Le critère §147 passe de bout en bout en test d'intégration | 3 j |
| **L6** | Validation, doublons, chevauchements, confiance | Table de §7.2 entièrement couverte | 2 j |
| **L7** | Export ICS + `VTIMEZONE` + conformité | ICS relu par analyseur tiers, importé manuellement dans les 4 calendriers cibles | 3 j |
| **L8** | Export CSV (3 profils), TXT (2 formats), Excel accessible | Tests d'instantané + import Google Agenda réel réussi | 2–3 j |
| **L9** | **Coquille UI + V1 Accueil + V3 Analyse + V4 Aperçu (lecture seule)** | **Première démonstration bout en bout : fichier → aperçu → 3 fichiers exportés** | 5 j |
| **L10** | V6 Édition, V5 Vérification, navigation entre erreurs, annulation, autosave | Un planning avec 2 erreurs est corrigé et exporté sans souris | 4 j |
| **L11** | V7 Export, V8 Terminé, ouverture du dossier, presse-papiers, préparation e-mail | §45 et §46 opérationnels | 2 j |
| **L12** | **Passe d'accessibilité complète** | §112 validé sous NVDA écran éteint ; zéro violation automatisée | 3–4 j |
| **L13** | V9 Historique, V11 Paramètres, V12 Aide, première ouverture | Paramètres de §72–§74 effectifs et persistés | 3 j |
| **L14** | Modèles : enregistrement, détection automatique, éditeur (§47–§50) | Un modèle enregistré est reconnu et appliqué automatiquement | 4 j |
| **L15** | Packaging : exécutable autonome, installateur sans droits admin, version portable, export de diagnostic | Installation puis conversion réussie sur un poste vierge sans Excel | 3 j |

**MVP au sens de §137 = L0 → L13** (L14 relève de la v1.1 selon §138, mais son architecture est posée dès L4).
Total indicatif MVP : **40 à 45 jours-homme**, dont environ un tiers sur le seul moteur d'analyse (L2–L6).

**Jalons de contrôle :**
- **J1 après L6** : le moteur produit des événements corrects sur le corpus, sans aucune interface. C'est le vrai point de risque du projet ; s'il glisse, tout glisse.
- **J2 après L9** : démonstration utilisable au secrétariat, retours réels avant d'investir dans l'édition.
- **J3 après L12** : validation d'accessibilité, si possible avec un utilisateur de lecteur d'écran.
- **J4 après L15** : livraison.

---

## 15. Registre des risques

| Risque | Impact | Probabilité | Mitigation |
|--------|--------|-------------|------------|
| Les fichiers réels sont en disposition « grille » (jours en colonnes) et non en lignes | Élevé — remet en cause L4/L5 | Moyenne à élevée | **Obtenir 3 à 5 fichiers réels avant L4.** Si la grille domine, en faire la disposition principale et réordonner L4/L5 |
| Fichiers `.xls` anciens ou faussement étiquetés | Moyen | Élevée | Détection par signature dès L2, message dédié, conseil de réenregistrement en `.xlsx` |
| ICS mal interprété par Apple Calendrier (fuseau) | Élevé — fonction principale | Moyenne | `VTIMEZONE` embarqué dès L7, matrice d'import manuelle obligatoire |
| Les cartes d'événements personnalisées mal restituées par NVDA | Élevé (I2) | Moyenne | Prototype d'accessibilité de la liste **dans L9**, pas dans L12 |
| Dérive de périmètre vers l'IA / OCR / modèles | Moyen | Élevée | §143 est explicite : ces sujets sont hors MVP. Les interfaces d'extension existent, les implémentations non |
| Trop de faux positifs d'anomalies → l'utilisateur clique sans lire | Élevé (I5) | Moyenne | Mesurer sur le corpus le nombre d'éléments « à vérifier » par planning ; cible **≤ 3 pour un planning propre** |
| Windows 10 hors support / contraintes de version .NET sur les postes cibles | Moyen | Moyenne | Vérifier les versions Windows réelles du secrétariat avant L0 (décision D3) |

---

## 16. Décisions à confirmer avant de commencer

| # | Décision | Pourquoi c'est bloquant | Recommandation |
|---|----------|------------------------|----------------|
| **D1** | **Obtenir 3 à 5 plannings Excel réels, anonymisés** | Détermine la disposition principale (lignes ou grille), donc l'architecture du moteur. C'est l'action la plus rentable du projet | À faire **avant L4**, idéalement avant L0 |
| **D2** | Périmètre de la première livraison | MVP strict de §137, ou MVP + modèles enregistrés | MVP strict ; les modèles arrivent quand on aura observé les fichiers réels |
| **D3** | Versions de Windows sur les postes du secrétariat | Conditionne le choix .NET 10 vs .NET 8 et la stratégie d'installation | Vérifier ; si Windows 11 uniquement, .NET 10 sans réserve |
| **D4** | Format de titre par défaut des événements | §38 recommande `Cours — Enseignant` | Retenir la recommandation, paramétrable |
| **D5** | Pauses incluses ou non par défaut | §23 recommande « désactivé » | Retenir la recommandation |
| **D6** | Qui développe (vous avec assistance IA, une équipe interne, un prestataire) | Change la granularité attendue des spécifications de chaque lot | À préciser |

---

## 17. Ce que ce plan garantit

Si ce plan est suivi, le critère d'acceptation final de §147 est satisfait par construction : un fichier contenant `Lundi / 08h30 - 10h30 / Neurologie / Dr Martin / Salle 204` traverse les étages 0 à 11, obtient sa date par ancrage de semaine, son titre par le format `Cours — Enseignant`, son lieu par la colonne salle, et produit un `VEVENT` conforme testé à l'import.

Et l'invariant qui compte le plus, §148 : **à aucun endroit de cette chaîne le logiciel ne complète une information manquante en silence.**
