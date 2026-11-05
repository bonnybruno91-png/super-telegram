# Planning Converter

Application Windows (C# / .NET 10 / WPF) qui convertit les emplois du temps Excel du secrétariat
(promotion MK4) en plannings accessibles : calendrier **ICS**, texte accessible **TXT**, tableur **CSV**
et **PDF** gros caractères. Conçue pour des étudiants malvoyants : navigation clavier, libellés
explicites, messages annoncés aux lecteurs d'écran, thème sombre à fort contraste.

Spécification faisant autorité : `docs/Cahier_des_charges_final_Planning_Converter.md`
(`docs/historique/` = anciens documents, à titre d'archive). Notes techniques : `DEV_NOTES.md`.

## État réel (5 novembre 2026)

Compilé et testé en ligne par GitHub Actions (runner Windows) : **99 tests automatiques passent**
(65 analyse/validation/sauvegarde/fusion, 34 exports) et l'exécutable démarre sans erreur.
**Jamais vérifié à l'écran** : le rendu WPF, la navigation clavier et l'annonce par NVDA/Narrateur
restent à essayer sur un poste Windows.

Parcours disponible : Importer (un ou plusieurs fichiers, un par semaine) → Vérifier (cours par jour,
corriger / ignorer / réintégrer, annuler / rétablir, remarques sur les semaines manquantes ou en
double) → Exporter (ICS, TXT, CSV, PDF). Reprise automatique de la dernière conversion, écrans Aide et
Historique.

Règles métier confirmées :
- P1, P2, S1…S4 sont des **salles**. Le **groupe** (A/B) vient de la colonne « Groupe » ; vide = cours commun.
- Un seul fichier par format avec tous les cours ; le groupe termine le titre (« … — Groupe B »).
- « TEMPS PERSONNEL », réunions et jours fériés sont conservés.
- Un cours incomplet est exclu de l'export sans bloquer les autres ; les pauses sont ignorées.

Pas encore fait : thèmes clair et contraste système, écran Paramètres et Modèles, PDF balisé
(accessibilité native), correction des cours par champ avec rapprochement entre versions d'un fichier,
signature de l'exécutable.

## Obtenir l'exécutable

Onglet *Actions* du dépôt GitHub → dernière exécution réussie → artefact `PlanningConverter_win-x64`
(autonome, aucun prérequis). Il n'est pas signé : Windows SmartScreen peut afficher un avertissement.

En local (Windows, SDK .NET 10) : `.\build.ps1` → `artifacts\windows-x64\PlanningConverter.exe`.

## Structure

```
src/
  PlanningConverter.Domain        — modèle métier
  PlanningConverter.Spreadsheet   — lecture .xlsx/.xls (NPOI)
  PlanningConverter.Parsing       — analyse (sections de jour, colonnes, groupes)
  PlanningConverter.Validation    — doublons, chevauchements, confiance, statut
  PlanningConverter.Export        — ICS, CSV, TXT, PDF (écrits sans bibliothèque externe)
  PlanningConverter.Storage       — paramètres, historique, sauvegarde automatique
  PlanningConverter.Application   — session, cas d'usage, fusion de semaines
  PlanningConverter.Ui            — WPF (seul projet net10.0-windows)
tests/
  PlanningConverter.Parsing.Tests — corpus anonymisé (3 semaines + 1 semaine avec groupes)
  PlanningConverter.Export.Tests
```

Les fichiers de test sont anonymisés (noms d'enseignants fictifs). Les originaux ne doivent jamais
être déposés dans le dépôt.
