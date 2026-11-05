# Notes de développement — Planning Converter

Ce document complète `Plan_de_conception_Planning_Converter.md`. Il capture ce qui a changé
entre la conception (basée sur le cahier des charges seul) et le développement (basé sur
3 fichiers réels du secrétariat), et l'état exact du code livré.

---

## 1. Constat terrain — écarts avec le cahier des charges idéalisé

Trois fichiers réels ont été fournis : semaines 19, 23 et 26 du planning MK4. Analyse cellule
par cellule (styles compris) avant tout code.

| Prévu par le cahier des charges | Observé dans les fichiers réels |
|---|---|
| Cellules fusionnées pour le jour (§14) | **Aucune fusion dans aucun des 3 fichiers.** Le jour est un **en-tête de bloc** : une ligne où seule la colonne "Intitulé" contient `"Lundi 01 Juin 2026"` (gras, violet `#330033`), suivie de ses lignes d'événements. |
| Plage horaire avec séparateur (`08h30-10h30`) | **Deux lignes dans une seule cellule**, séparées par un retour à la ligne : `"10:00\n11:00"`. Aucun tiret. |
| Colonne "Jour" distincte | Absente : le nom du jour est *dans* la date de l'en-tête de bloc. |
| En-tête "Heure" toujours présent | **Absent dans le fichier "Semaine 26"** (cellule A2 vide) — colonne retrouvée par contenu. |
| UE dans une colonne dédiée | **Préfixée dans l'intitulé** : `"UE18 ORAL PATHOLOGIES SANTE MENTALE"`. |
| Colonne "Salle" = un lieu | Contient **soit un vrai lieu** (`AMPHI 3 LAENNEC`), **soit un code de groupe** (`P1 IFMKDV`, `S3 IFMKDV`), parfois les deux sur des lignes séparées de la même cellule. |
| — | Certaines séances n'ont **aucun enseignant** renseigné (travail en autonomie, "TK ... SANS ENSEIGNANT") : légitime, pas une anomalie. |
| Jour sans cours | Existe réellement (mercredi semaine 23 ; mardi et vendredi semaine 26) : bloc jour présent, zéro ligne d'événement — **pas une erreur**. |
| — | Une ligne sans aucune heure, `"VICTOIRE 1945"` (8 mai, jour férié) : cas réel du §57, traité comme candidat "journée entière" plutôt que comme une simple absence d'heure. |

**Conséquence architecturale** : le moteur livré (`DaySectionParser`) implémente la disposition
"section de jour" confirmée par ces 3 fichiers, et non le modèle générique par fusion de
cellules envisagé dans le plan initial. La genericité pour d'autres établissements (disposition
en grille, fusions réelles) reste à construire — voir section 4.

## 2. Validation croisée Python / C#

La logique a été prototypée et exécutée en Python **avant** d'être portée en C#, pour deux
raisons : (1) ce sandbox de développement n'a pas de SDK .NET ni d'accès à NuGet — impossible
de compiler le C# ici ; (2) avoir deux implémentations indépendantes qui s'accordent est une
garantie de plus que le comportement attendu est correct.

Un bug a été trouvé et corrigé **grâce à ce filet** : une cellule horaire contenant uniquement
un retour à la ligne (`"\n"`, cas réel de la ligne "VICTOIRE 1945") est considérée "vide" par
`SourceCell.IsEmpty`, ce qui faisait initialement confondre cette ligne d'événement avec un
en-tête de section de jour et la faisait disparaître silencieusement. Corrigé dans
`DaySectionParser` : la reconnaissance d'un en-tête de section se fait maintenant sur le texte
de la cellule "Intitulé" lui-même, jamais sur la simple absence de contenu des colonnes voisines.

**Décompte final validé sur le corpus** (à corriger si vous avez lu la conversation avant cette
note : le premier message annonçait 21 événements, la correction du bug ci-dessus porte le total
réel à **22** — la ligne "VICTOIRE 1945" compte comme un événement, marqué "à vérifier") :

| Fichier | Événements | Jours sans cours |
|---|---|---|
| Semaine 23 (1–5 juin) | 6 | 1 (mercredi) |
| Semaine 19 (4–8 mai) | 11 (dont "VICTOIRE 1945", journée entière) | 0 |
| Semaine 26 (22–26 juin) | 5 | 2 (mardi, vendredi) |

Ces valeurs sont figées dans `DaySectionParserRealFilesTests.cs` : elles doivent rester vraies
après toute modification du moteur.

## 3. Décisions prises pendant le développement

| Sujet | Décision | Statut |
|---|---|---|
| Disposition principale du moteur | Sections de jour (bloc titre isolé), pas fusion de cellules | **Confirmé** par 3 fichiers réels |
| Colonne horaire sans en-tête | Détection par contenu (≥ 50 % de cellules reconnues comme horaire) | Implémenté, seuil arbitraire à ajuster si d'autres fichiers arrivent |
| Séparation lieu / groupe dans "Salle" | Heuristique par ligne (mots-clés de salle vs motif `[PSGL]\d+ MOT`) | **Provisoire** — à valider avec le secrétariat ; aucune donnée n'est perdue (les deux champs sont peuplés, jamais l'un au prix de l'autre) |
| Ligne sans aucune heure | "Journée entière" seulement si le titre correspond à un lexique de mots-clés (jours fériés) ; sinon anomalie bloquante | Conforme à I1 : le doute reste visible, jamais tranché en silence |
| Format de titre par défaut | `Cours — Enseignant` (§38, D4 du plan) | Confirmé |
| VTIMEZONE Europe/Paris | Écrit à la main, pas généré par Ical.Net | Voir section 4 — limite de bibliothèque découverte en cours de route |

## 4. Limites connues et travail restant

- **Ical.Net et VTIMEZONE** : `Calendar.AddTimeZone(new VTimeZone(tzid))` ne produit qu'un
  VTIMEZONE tronqué (TZID + X-LIC-LOCATION, sans STANDARD/DAYLIGHT) — limite documentée du
  projet (issues #58 et #241 sur `ical-org/ical.net`, la seconde encore ouverte en 2025).
  Contournée en écrivant le bloc Europe/Paris à la main (`ParisTimeZoneBlock.cs`) et en
  l'insérant par substitution de texte après sérialisation. **À vérifier en priorité au premier
  test réel d'import** (§113) : Outlook, Google Agenda, Apple Calendrier.
- **Versions des paquets NuGet non vérifiables ici** (réseau restreint, pas d'accès à
  nuget.org) : NPOI 2.7.1, Ical.Net 5.0.0, CsvHelper 33.0.1, xUnit 2.9.2 sont les dernières
  versions stables connues au moment de la rédaction — à confirmer avec `dotnet add package`
  au premier `dotnet restore` (§89 l'exige explicitement).
- **Disposition "grille"** (jours en colonnes, horaires en lignes) mentionnée dans le plan
  initial comme risque principal : non implémentée, car non observée dans le corpus disponible.
  Si un autre établissement ou une autre promotion produit ce format, `LayoutClassifier` (prévu
  au plan, non encore écrit) devra être ajouté avant `DaySectionParser`.
- **Couches non commencées** : `PlanningConverter.Storage` (paramètres, modèles, historique),
  `PlanningConverter.Application` (orchestration, session de conversion, undo), et surtout
  `PlanningConverter.Ui` (WPF — tout l'écran d'accueil, l'aperçu accessible, l'édition). C'est
  la suite naturelle, correspondant aux lots L9 à L15 du plan de conception.
- **Aucune exécution réelle effectuée** : ce sandbox n'a ni SDK .NET ni accès à nuget.org.
  Le code n'a jamais été compilé. La logique métier a été validée par un prototype Python
  indépendant (section 2), mais une première compilation sur poste Windows peut révéler des
  erreurs de syntaxe ou de nom d'API (en particulier autour d'Ical.Net, dont l'API exacte n'a
  pu être vérifiée que par recherche web, pas par IntelliSense).

## 5. Corpus de test — mise en garde

Les 3 fichiers `.xlsx` copiés dans `tests/PlanningConverter.Parsing.Tests/Fixtures/` sont les
fichiers réels fournis, **non anonymisés** : ils contiennent de vrais noms d'enseignants.
Le cahier des charges (§110) demande un corpus anonymisé pour les tests. Ces fichiers sont
utiles tels quels pour le développement local, mais **ne doivent pas être commités dans un
dépôt partagé ou distribué hors de l'équipe** sans remplacer les noms par des valeurs fictives
au préalable.

## 7. Couches Storage, Application et Ui (suite du développement)

Ajoutées après la première livraison :

- **`PlanningConverter.Storage`** : paramètres (JSON, écriture atomique), historique (SQLite,
  schéma minimal §92, détection de reconversion §100 par empreinte SHA-256), sauvegarde
  automatique (§105, agnostique du contenu — reçoit du JSON déjà sérialisé), modèles enregistrés
  (§47-§50) — **modèle de données posé, mais pas encore branché sur `DaySectionParser`**, qui
  ignore pour l'instant tout modèle enregistré. C'est le travail du lot L14.
- **`PlanningConverter.Application`** : `ConversionSession` (l'unique objet mutable, undo/redo
  par instantané complet — simple mais suffisant à l'échelle d'un planning hebdomadaire), trois
  cas d'usage (`OpenWorkbookUseCase`, `AnalyzeUseCase`, `ExportUseCase`), et `CompositionRoot`
  qui assemble toutes les implémentations concrètes en un seul endroit. C'est ce dernier point
  qui permet à `Ui` de ne jamais référencer `Spreadsheet`/`Parsing`/`Export` directement, comme
  l'exige la règle d'architecture du plan, tout en ayant bien tout d'instancié quelque part.
- **`PlanningConverter.Ui`** : coquille WPF minimale — `App.xaml(.cs)` (composition, journalisation
  Serilog, filet de sécurité global §145), `MainWindow` (rail de navigation, région live de statut
  §64), et **un seul écran fonctionnel : l'Accueil** (§10.3 — toute la fenêtre en zone de dépôt,
  carte unique surélevée, focus automatique après sélection).

### Ce qui est délibérément incomplet dans l'Ui à ce stade

- Le bouton "Analyser" **n'appelle pas encore `AnalyzeUseCase`** — il se contente d'annoncer le
  nom du fichier reçu dans la barre de statut. Le brancher est la prochaine étape naturelle
  (écran V3 Analyse en cours, puis V4 Aperçu).
- Les entrées de navigation Historique / Modèles / Paramètres / Aide sont visibles mais
  désactivées (`IsEnabled="False"`) — délibéré, pour ne jamais présenter comme fonctionnel ce
  qui ne l'est pas.
- Les icônes du rail de navigation sont des emojis Unicode bruts (🏠, 🕑...), un choix de
  facilité, pas une direction graphique définitive — à remplacer par de vraies icônes vectorielles.
- Le repli du rail en dessous de 900 px (§123) n'est pas implémenté (commentaire `TODO §123`
  laissé dans `MainWindow.xaml`).
- Rien dans cette couche n'a été rendu à l'écran ni testé au clavier/lecteur d'écran, pour la
  même raison que le reste : ce sandbox ne peut pas exécuter de WPF. La conformité à I2
  (accessible ou incomplet) a été appliquée par écriture disciplinée (AutomationProperties,
  focus explicite, région live), pas vérifiée par un passage NVDA réel — à faire en priorité
  dès la première ouverture sur Windows (§112).


## 8. Comment continuer

1. Ouvrir `PlanningConverter.sln` dans Visual Studio 2022+ avec le SDK .NET 10 installé
   (nécessaire pour `PlanningConverter.Ui`, seul projet ciblant `net10.0-windows`).
2. `dotnet restore` — vérifiera/téléchargera les versions exactes des paquets.
3. `dotnet build` — première compilation réelle. Erreurs les plus probables : noms d'API exacts
   d'Ical.Net (section 4) et d'`AutomationProperties.HeadingLevel` (ajouté récemment à WPF,
   à vérifier selon la version exacte du SDK).
4. `dotnet test` — les tests de `PlanningConverter.Parsing.Tests` et `.Export.Tests` doivent
   tous passer sans modification.
5. Lancer `PlanningConverter.Ui` (F5) : l'écran d'accueil doit s'afficher, accepter un glisser-
   déposer ou un clic sur "Sélectionner un fichier", puis afficher le nom/la taille du fichier
   et un bouton "Analyser" qui reçoit le focus automatiquement.
6. Prochaine étape naturelle : brancher `AnalyzeUseCase` sur le bouton "Analyser" (actuellement
   simulé, voir section 7) et construire l'écran d'aperçu (V4, §29-§30).

## 9. Mise à jour du 5 novembre 2026 (reprise, lots 0 à 3 et suite)

Cette section prévaut sur les paragraphes plus anciens quand ils se contredisent (en particulier
§4 « jamais compilé » et §7 « Ui non commencée »).

- **Groupes** : la colonne « Groupe » du fichier est la seule source (`GroupLabel.Normalize`).
  P*/S* sont des salles (`DaySectionParser` ne les sépare plus de la colonne Salle).
  Fixture : `EDT_MK4_SEMAINE_46_AVEC_GROUPES_NOV_2026.xlsx` (noms fictifs).
- **Titre exporté** : `{UE} {Intitulé}` + ` — Enseignant` (selon le format) + ` — Groupe B` en dernier.
- **Plusieurs fichiers** : `SessionMerger` ordonne les semaines, signale un trou (lundi–vendredi), garde le
  fichier le plus récent si une semaine est en double (l'autre est ignorée, réintégrable), signale
  plusieurs promotions.
- **Reprise** : `ConversionSession.ToAutosaveJson/FromAutosaveJson`, avec `ReadOnlySetJsonConverterFactory`
  (System.Text.Json ne relit pas `IReadOnlySet<T>` — découvert par `AutosaveRoundTripTests`). Une seule
  sauvegarde reprenable ; une nouvelle analyse remplace les précédentes.
- **PDF** : `PdfPlanningWriter` (PDF 1.4 écrit à la main, Helvetica 18 pt, texte sélectionnable, WinAnsi).
  Non balisé : limite à documenter auprès des utilisateurs ; le TXT reste la sortie de référence pour
  un lecteur d'écran. Largeurs de caractères approchées (marge de sécurité pour le gras) : à relire sur un
  vrai PDF imprimé.
- **Compilation** : aucune compilation locale possible (nuget.org bloqué dans l'environnement de
  développement) ; tout passe par `.github/workflows/build.yml`.
- **Non vérifié** : rendu WPF, focus clavier, annonces de la région live, comportement du glisser-déposer.
- **Reste à faire** : thèmes clair/contraste système, Paramètres et Modèles, PDF balisé, corrections par
  champ avec rapprochement entre versions d'un même fichier, signature de l'exécutable.

