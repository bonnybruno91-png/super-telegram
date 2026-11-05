# Planning Converter

Application Windows convertissant des emplois du temps Excel en calendriers accessibles
(ICS, CSV, texte). Voir `Cahier_des_charges_Planning_Converter.md` pour la spécification
complète et `Plan_de_conception_Planning_Converter.md` pour l'architecture et le lotissement.

**Avant toute chose, lisez `DEV_NOTES.md`** : il documente les écarts constatés entre le
cahier des charges et les fichiers réels du secrétariat, les décisions prises en conséquence,
et les limites connues du code actuel.

## État réel (5 novembre 2026, reprise)

**Rien n'a encore été compilé ni exécuté.** Lot 0 appliqué sur le papier : exécutable WPF déclaré
(`WinExe`), conflit de nom `Application` corrigé, ICS et CSV écrits sans bibliothèque externe,
validation recalculée à chaque correction, cours incomplet exclu sans bloquer l'export, pauses ignorées,
lexiques par mots entiers, thème sombre, classeurs de test anonymisés, workflow Windows + `build.ps1`.
Voir `docs/Cahier_des_charges_final_Planning_Converter.md` (autorité) ; `docs/historique/` = anciens documents.

**Compiler et publier (Windows, SDK .NET 10) :** `.\build.ps1` → `artifacts\windows-x64\PlanningConverter.exe`.

## État d'avancement (ancien, version initiale)

Correspond approximativement aux lots **L0 à L9 (partiel)** du plan de conception (sur 16 au
total) : le moteur d'analyse, les trois exports obligatoires du MVP (§137), la persistance,
l'orchestration, et une coquille WPF avec un seul écran fonctionnel (Accueil).

| Couche | Statut |
|---|---|
| `PlanningConverter.Domain` | ✅ Complet |
| `PlanningConverter.Spreadsheet` (lecture Excel, NPOI) | ✅ Complet |
| `PlanningConverter.Parsing` (moteur d'analyse) | ✅ Complet pour la disposition "section de jour" confirmée par le corpus réel |
| `PlanningConverter.Validation` (doublons, chevauchements, confiance) | ✅ Complet |
| `PlanningConverter.Export` (ICS, CSV, TXT) | ✅ Complet, **jamais compilé** — voir DEV_NOTES §4 |
| `PlanningConverter.Storage` (paramètres, historique, autosave, modèles) | ✅ Complet ; modèles enregistrés pas encore consommés par le moteur |
| `PlanningConverter.Application` (session, cas d'usage, composition) | ✅ Complet |
| `PlanningConverter.Ui` (WPF) | 🟡 Coquille + écran Accueil fonctionnel ; le reste (V2-V12) reste à construire — voir DEV_NOTES §7 |

## Build

Nécessite Visual Studio 2022+ (ou le SDK .NET 10 en ligne de commande) et un accès réseau à
nuget.org — ce dépôt a été développé dans un environnement sans les deux, donc **jamais compilé
ni testé pour de vrai**. Première étape sur une machine adéquate :

```
dotnet restore
dotnet build
dotnet test
```

Les tests de `PlanningConverter.Parsing.Tests` s'exécutent contre 3 fichiers Excel réels
(`tests/PlanningConverter.Parsing.Tests/Fixtures/`) — **non anonymisés, voir DEV_NOTES §5**
avant de partager ce dépôt.

## Structure

```
src/
  PlanningConverter.Domain        — modèle métier, aucune dépendance externe
  PlanningConverter.Spreadsheet   — lecture .xlsx/.xls (NPOI)
  PlanningConverter.Parsing       — moteur d'analyse
  PlanningConverter.Validation    — doublons, chevauchements, confiance, statut
  PlanningConverter.Export        — ICS, CSV, TXT
  PlanningConverter.Storage       — paramètres, historique, autosave, modèles
  PlanningConverter.Application   — session de conversion, cas d'usage, composition
  PlanningConverter.Ui            — WPF (seul projet ciblant net10.0-windows)
tests/
  PlanningConverter.Parsing.Tests — inclut le corpus réel (§110 du cahier des charges)
  PlanningConverter.Export.Tests
```
