# Cahier des charges — Application Windows de conversion de plannings Excel vers calendriers

## 1. Présentation générale du projet

### 1.1. Contexte

Les emplois du temps universitaires sont fréquemment transmis sous forme de fichiers Microsoft Excel contenant des tableaux organisés par jours, horaires, cours, enseignants, salles, unités d’enseignement et informations complémentaires.

Ces tableaux sont relativement faciles à consulter visuellement mais peuvent devenir complexes pour les utilisateurs de lecteurs d’écran ou de synthèse vocale, notamment lorsque le fichier comporte :

- des cellules fusionnées ;
- plusieurs niveaux d’en-têtes ;
- des jours répartis sur plusieurs lignes ;
- plusieurs cours dans une même journée ;
- des horaires regroupés dans une seule cellule ;
- des codes couleurs ;
- plusieurs feuilles Excel ;
- des informations répétées ou laissées implicitement vides ;
- des mises en page conçues avant tout pour l’impression.

L’objectif du logiciel est de permettre à un secrétariat universitaire de convertir très simplement ces plannings Excel en formats directement exploitables par les étudiants, notamment dans Apple Calendrier, Google Agenda, Outlook ou sous forme de texte accessible.

Le logiciel doit être conçu pour pouvoir être utilisé quotidiennement par des personnes n’ayant aucune compétence informatique particulière.

---

# 2. Objectif principal

Créer une application Windows permettant de :

1. sélectionner ou déposer un fichier Excel contenant un emploi du temps ;
2. analyser automatiquement sa structure ;
3. identifier les événements du planning ;
4. détecter les jours, dates, horaires, cours, enseignants, salles et autres informations ;
5. signaler uniquement les éléments ambigus ou incomplets ;
6. permettre une correction simple ;
7. générer plusieurs formats accessibles ;
8. enregistrer les fichiers produits ;
9. faciliter leur transmission aux étudiants.

Le logiciel doit fonctionner en priorité localement, sans dépendre d’une connexion Internet.

---

# 3. Public cible

## 3.1. Utilisateur principal

Personnel administratif ou secrétariat d’un établissement universitaire.

L’utilisateur principal ne doit pas avoir besoin de connaître :

- le fonctionnement du format CSV ;
- le fonctionnement du format ICS ;
- les règles iCalendar ;
- le fonctionnement interne d’Excel ;
- la programmation ;
- les notions de parsing ou de structures de données.

Le logiciel doit masquer toute cette complexité.

## 3.2. Bénéficiaires

Les fichiers générés sont destinés notamment :

- aux étudiants ;
- aux étudiants déficients visuels ;
- aux utilisateurs d’iPhone ;
- aux utilisateurs d’Android ;
- aux utilisateurs de Google Agenda ;
- aux utilisateurs d’Outlook ;
- aux utilisateurs d’Apple Calendrier ;
- aux utilisateurs de lecteurs d’écran.

---

# 4. Principes fondamentaux du produit

## 4.1. Simplicité

Le parcours principal doit pouvoir être réalisé en quelques clics.

Scénario idéal :

**ouvrir l’application → choisir le fichier Excel → vérifier → générer les fichiers.**

## 4.2. Accessibilité

L’application doit être utilisable intégralement au clavier et compatible avec les technologies d’assistance Windows.

Aucune fonctionnalité essentielle ne doit nécessiter l’utilisation de la souris.

## 4.3. Local-first

L’analyse des fichiers doit être effectuée localement lorsque cela est possible.

Aucun fichier contenant un planning étudiant ne doit être envoyé automatiquement vers un serveur externe.

## 4.4. Fiabilité

Le logiciel ne doit jamais inventer une information absente.

En cas d’incertitude, il doit :

- conserver l’information originale ;
- signaler l’ambiguïté ;
- demander une validation humaine.

## 4.5. Réversibilité

Le fichier Excel original ne doit jamais être modifié.

Toute conversion doit produire de nouveaux fichiers.

---

# 5. Nom provisoire

Nom de travail :

**Planning Converter**

Le nom devra pouvoir être facilement remplacé ultérieurement sans modification importante du code.

---

# 6. Plateforme cible

Application de bureau Windows.

Compatibilité souhaitée :

- Windows 10 ;
- Windows 11 ;
- versions ultérieures lorsque techniquement possible.

Architecture recommandée :

- application native ou desktop ;
- fonctionnement hors ligne ;
- installation simple ;
- possibilité ultérieure de produire une version portable.

---

# 7. Formats d’entrée

## 7.1. Formats obligatoires

Le logiciel doit accepter au minimum :

- `.xlsx`
- `.xls`

## 7.2. Formats futurs optionnels

Prévoir une architecture permettant ultérieurement l’ajout de :

- `.xlsm`
- `.ods`
- `.csv`
- PDF texte ;
- PDF scanné ;
- image PNG ;
- image JPEG.

Les formats PDF et image pourront utiliser ultérieurement un module OCR distinct.

Ils ne sont pas indispensables à la version initiale.

---

# 8. Formats de sortie

Le logiciel doit pouvoir produire plusieurs représentations du même planning.

## 8.1. ICS / iCalendar

Format principal recommandé.

Extension :

`.ics`

Compatible avec de nombreux logiciels de calendrier.

Chaque cours doit devenir un événement indépendant.

## 8.2. CSV

Le logiciel doit produire un CSV structuré permettant notamment l’importation dans certains services de calendrier ou l’exploitation dans un tableur.

## 8.3. Texte accessible

Format `.txt`.

Exemple :

```text
Lundi 14 septembre 2026

08h30 - 10h30
Neurologie
Enseignant : Dr Martin
Salle : 204
UE : UE18

11h00 - 12h30
Traumatologie
Enseignant : Mme Bernard
Salle : 201
```

## 8.4. Excel accessible

Option souhaitable.

Produire un nouveau fichier Excel normalisé dans lequel :

- chaque ligne correspond à un événement ;
- aucune cellule n’est fusionnée ;
- les en-têtes sont explicites ;
- aucune information essentielle ne dépend uniquement d’une couleur.

Structure recommandée :

```text
Date
Jour
Heure de début
Heure de fin
Cours
Enseignant
Salle
UE
Groupe
Informations complémentaires
```

---

# 9. Parcours utilisateur principal

Le parcours doit être organisé autour de cinq étapes.

```text
1. Importer
2. Analyser
3. Vérifier
4. Exporter
5. Terminé
```

Une barre de progression ou un indicateur d’étapes peut afficher ces cinq étapes.

L’utilisateur doit toujours savoir où il se trouve.

---

# 10. Écran 1 — Accueil

## 10.1. Objectif

Permettre de lancer immédiatement une conversion.

## 10.2. Contenu principal

Titre :

**Convertir un planning**

Zone centrale :

**Déposez votre fichier Excel ici**

Bouton :

**Sélectionner un fichier**

Texte secondaire :

```text
Formats acceptés : XLSX et XLS
```

## 10.3. Actions secondaires

Les actions secondaires peuvent apparaître dans un menu latéral :

- Accueil
- Historique
- Modèles
- Paramètres
- Aide

Le bouton « Convertir un planning » reste visuellement prioritaire.

## 10.4. Glisser-déposer

Le logiciel doit accepter le glisser-déposer.

Lorsqu’un fichier valide est déposé :

- afficher son nom ;
- afficher éventuellement sa taille ;
- afficher un bouton « Analyser ».

Si le fichier n’est pas compatible :

```text
Ce format de fichier n’est pas pris en charge.

Formats acceptés : XLSX et XLS.
```

---

# 11. Écran 2 — Analyse

## 11.1. Déclenchement

Une fois le fichier choisi, lancer automatiquement ou manuellement l’analyse.

Afficher :

```text
Analyse du planning en cours…
```

L’interface ne doit pas paraître bloquée.

## 11.2. Informations analysées

Le moteur doit rechercher notamment :

- nom de la feuille ;
- jours ;
- dates ;
- heures ;
- plages horaires ;
- intitulés de cours ;
- enseignants ;
- salles ;
- UE ;
- groupes ;
- promotions ;
- remarques ;
- informations complémentaires.

---

# 12. Gestion des feuilles Excel

Un classeur peut contenir plusieurs feuilles.

Le logiciel doit :

1. détecter toutes les feuilles ;
2. ignorer éventuellement les feuilles complètement vides ;
3. analyser les feuilles contenant suffisamment de données ;
4. proposer une sélection si plusieurs feuilles semblent correspondre à des plannings.

Exemple :

```text
3 feuilles détectées :

MK3
MK4
Examens

Quelle feuille souhaitez-vous convertir ?
```

Prévoir la possibilité de sélectionner plusieurs feuilles.

---

# 13. Analyse des cellules

Le moteur doit distinguer :

- texte ;
- nombre ;
- date Excel ;
- heure Excel ;
- formule ;
- cellule vide ;
- cellule fusionnée.

Les valeurs résultant de formules doivent être lues si elles sont disponibles.

---

# 14. Cellules fusionnées

La gestion des cellules fusionnées est obligatoire.

Exemple :

```text
LUNDI
```

peut être fusionné verticalement sur cinq lignes.

Le moteur doit considérer que toutes les lignes concernées appartiennent au lundi.

Lorsqu’une valeur fusionnée s’applique à plusieurs événements, elle doit être propagée logiquement.

---

# 15. Propagation des valeurs implicites

Les emplois du temps utilisent souvent la convention suivante :

```text
Lundi | 08h30 | Anatomie
      | 10h30 | Biomécanique
      | 14h00 | Neurologie
```

La valeur « Lundi » ne doit pas être considérée comme absente sur les lignes suivantes.

Le moteur doit conserver le dernier contexte valide lorsque la structure indique clairement une continuité.

---

# 16. Détection des jours

Le logiciel doit reconnaître au minimum :

- lundi ;
- mardi ;
- mercredi ;
- jeudi ;
- vendredi ;
- samedi ;
- dimanche.

La reconnaissance doit être insensible à la casse.

Exemples valides :

```text
LUNDI
Lundi
lundi
Lun.
LUN
```

---

# 17. Détection des dates

Formats à reconnaître notamment :

```text
14/09/2026
14-09-2026
14.09.2026
14 septembre 2026
Lundi 14 septembre
14/09
```

Lorsqu’une année manque, le logiciel doit essayer de la déduire du contexte du planning.

En cas d’incertitude, demander confirmation.

---

# 18. Détection des horaires

Formats à reconnaître notamment :

```text
08:30
8:30
08h30
8h30
8 H 30
0830
```

---

# 19. Détection des plages horaires

Le logiciel doit reconnaître notamment :

```text
08h30 - 10h30
08:30-10:30
8h30 à 10h30
08H30 / 10H30
8h30–10h
```

Il doit séparer :

```text
heure_debut = 08:30
heure_fin = 10:30
```

---

# 20. Cas d’une heure de fin absente

Exemple :

```text
14h00
Neurologie
```

Si aucune heure de fin n’est identifiable, l’événement doit être marqué :

**À vérifier**

Le logiciel ne doit pas inventer automatiquement une durée, sauf si une règle explicite a été configurée dans un modèle.

---

# 21. Détection des en-têtes

Le logiciel doit rechercher des synonymes.

### Cours

Reconnaître par exemple :

```text
Cours
Matière
Intitulé
Enseignement
Module
Discipline
```

### Enseignant

```text
Enseignant
Professeur
Intervenant
Formateur
Prof
```

### Salle

```text
Salle
Lieu
Local
Amphi
Amphithéâtre
```

### Heure

```text
Horaire
Horaires
Heure
Créneau
```

### Jour

```text
Jour
Journée
Date
```

---

# 22. Détection des lignes non pertinentes

Le moteur doit être capable d’ignorer lorsque cela est clairement identifiable :

- lignes vides ;
- titres généraux ;
- logos ;
- légendes ;
- totaux ;
- commentaires administratifs ;
- signatures ;
- mentions d’impression.

Ne jamais supprimer silencieusement une ligne contenant des informations ambiguës.

---

# 23. Gestion des pauses

Des entrées comme :

```text
Pause déjeuner
Repas
Pause
Déjeuner
```

ne doivent pas forcément devenir des événements.

Prévoir dans les paramètres :

**Inclure les pauses dans le calendrier**

Valeur par défaut recommandée :

désactivée.

---

# 24. Événements particuliers

Le logiciel doit pouvoir gérer :

- cours ;
- TD ;
- TP ;
- examens ;
- soutenances ;
- réunions ;
- stages ;
- journées sans cours ;
- vacances ;
- fermetures ;
- événements exceptionnels.

---

# 25. Construction du modèle interne d’un événement

Chaque événement doit être représenté en interne par une structure équivalente à :

```text
id
source_file
source_sheet
source_row
date
day
start_time
end_time
title
teacher
room
location
ue
group
promotion
notes
status
confidence
original_content
```

---

# 26. Statut d’un événement

Valeurs possibles :

```text
VALID
WARNING
ERROR
IGNORED
```

---

# 27. Niveau de confiance

Prévoir éventuellement un score interne de confiance.

Exemple :

```text
0.98 = très fiable
0.75 = probablement correct
0.40 = vérification recommandée
```

Ce score ne doit pas nécessairement être montré à l’utilisateur.

---

# 28. Résultat de l’analyse

Après analyse, afficher :

```text
Planning détecté

Semaine du 14 au 18 septembre 2026

28 événements détectés
26 événements validés
2 éléments à vérifier
```

---

# 29. Écran d’aperçu

Le planning ne doit pas être présenté sous forme de grille complexe.

Préférer une liste verticale.

Exemple :

```text
Lundi 14 septembre

08h30 – 10h30
Neurologie
Dr Martin
Salle 204

11h00 – 12h30
Traumatologie
Mme Bernard
Salle 201
```

---

# 30. Actions disponibles sur un événement

Pour chaque événement :

- Modifier
- Supprimer
- Ignorer
- Dupliquer

L’action « Modifier » ouvre un formulaire simple.

---

# 31. Formulaire de modification d’un événement

Champs :

```text
Date
Heure de début
Heure de fin
Intitulé
Enseignant
Salle
UE
Groupe
Lieu
Informations complémentaires
```

Boutons :

```text
Enregistrer
Annuler
```

---

# 32. Écran de vérification des erreurs

Cet écran doit afficher uniquement les événements nécessitant une intervention.

Exemple :

```text
2 éléments nécessitent votre attention
```

Événement :

```text
Mercredi 16 septembre

Rhumatologie

Début : 14h00
Fin : non détectée
```

Champ :

```text
Heure de fin
```

Bouton :

```text
Valider
```

---

# 33. Navigation entre les erreurs

Prévoir :

```text
Erreur précédente
Erreur suivante
```

et :

```text
2 sur 4
```

---

# 34. Validation globale

Lorsque toutes les erreurs bloquantes ont été corrigées :

```text
Le planning est prêt à être exporté.
```

---

# 35. Écran d’export

Titre :

**Choisir les formats**

Options :

```text
[x] Calendrier ICS — recommandé
[x] CSV
[x] Texte accessible
[ ] Excel accessible
```

---

# 36. Export ICS

Chaque événement doit contenir au minimum :

- UID unique ;
- date et heure de début ;
- date et heure de fin ;
- titre ;
- description ;
- lieu.

Exemple conceptuel :

```text
SUMMARY:Neurologie
LOCATION:Salle 204
DESCRIPTION:Enseignant : Dr Martin\nUE : UE18
```

---

# 37. Fuseau horaire

Le logiciel doit gérer explicitement le fuseau horaire.

Pour un établissement français, valeur initiale possible :

```text
Europe/Paris
```

Le fuseau doit rester configurable.

Les changements heure d’été / heure d’hiver doivent être correctement pris en compte par l’implémentation iCalendar utilisée.

---

# 38. Structure du titre des événements

Prévoir plusieurs formats.

### Format simple

```text
Neurologie
```

### Format avec enseignant

```text
Neurologie — Dr Martin
```

### Format avec UE

```text
UE18 — Neurologie
```

Le format doit être configurable.

Valeur recommandée par défaut :

```text
Cours — Enseignant
```

---

# 39. Description de l’événement

Exemple :

```text
Enseignant : Dr Martin
Salle : 204
UE : UE18
Groupe : MK4
```

Les champs absents ne doivent pas produire des lignes inutiles.

---

# 40. Export CSV

Utiliser un encodage compatible avec les caractères français.

UTF-8 recommandé.

Structure possible :

```text
Subject
Start Date
Start Time
End Date
End Time
Description
Location
```

Prévoir éventuellement plusieurs modèles CSV :

- CSV générique ;
- CSV Google Agenda ;
- CSV personnalisé.

---

# 41. Export texte

Deux formats possibles.

## Format détaillé

```text
Lundi 14 septembre 2026

08h30 - 10h30
Neurologie
Enseignant : Dr Martin
Salle : 204
```

## Format compact

```text
Lundi 14 septembre
08h30-10h30 : Neurologie — Dr Martin — Salle 204
```

L’utilisateur peut choisir le format dans les paramètres.

---

# 42. Copie dans le presse-papiers

Ajouter :

**Copier le planning en texte**

Après l’action :

```text
Planning copié dans le presse-papiers.
```

Le texte peut ensuite être collé dans :

- un e-mail ;
- Teams ;
- un ENT ;
- un message ;
- un document.

---

# 43. Nommage automatique des fichiers

Exemple :

```text
Planning_MK4_2026-09-14_2026-09-18.ics
Planning_MK4_2026-09-14_2026-09-18.csv
Planning_MK4_2026-09-14_2026-09-18.txt
```

Éviter les caractères incompatibles avec Windows.

---

# 44. Dossier de destination

Avant l’export :

```text
Enregistrer dans :
C:\Users\...\Documents\Plannings
```

Bouton :

**Modifier**

Prévoir la mémorisation du dernier dossier utilisé.

---

# 45. Écran final

Titre :

**Conversion terminée**

Afficher :

```text
28 événements exportés.

3 fichiers ont été créés.
```

Actions :

```text
Ouvrir le dossier
Copier le planning
Préparer un e-mail
Convertir un autre planning
```

---

# 46. Fonction « Préparer un e-mail »

Version initiale simple :

ouvrir le client de messagerie par défaut avec un objet et un message préparés lorsque cela est techniquement possible.

Objet :

```text
Planning MK4 — Semaine du 14 au 18 septembre 2026
```

Message :

```text
Bonjour,

Veuillez trouver ci-joint le planning de la semaine du 14 au 18 septembre 2026.

Une version compatible avec les applications de calendrier est également disponible.

Cordialement.
```

L’ajout automatique des pièces jointes peut être proposé ultérieurement selon les possibilités du client de messagerie.

---

# 47. Modèles de planning

Le logiciel doit pouvoir mémoriser différentes structures Excel.

Exemples :

```text
Planning MK1
Planning MK2
Planning MK3
Planning MK4
Planning MK5
Planning examens
```

---

# 48. Informations enregistrées dans un modèle

Un modèle peut contenir :

```text
nom
feuille
ligne d’en-tête
colonne jour
colonne date
colonne horaire
colonne début
colonne fin
colonne cours
colonne enseignant
colonne salle
colonne UE
colonne groupe
règles particulières
```

---

# 49. Détection automatique d’un modèle

Lorsqu’un fichier ressemble fortement à un modèle enregistré :

```text
Modèle détecté : Planning MK4
```

Le logiciel applique automatiquement les règles correspondantes.

---

# 50. Éditeur de modèle

Prévoir un écran avancé permettant de configurer manuellement :

```text
Jour = colonne A
Horaires = colonne B
Cours = colonne C
Enseignant = colonne D
Salle = colonne E
UE = colonne F
```

Cet écran ne doit pas gêner le parcours standard.

---

# 51. Historique

L’application doit conserver localement un historique des conversions.

Exemple :

```text
19 août 2026
MK4 — semaine 34
27 événements
ICS + CSV + TXT

12 août 2026
MK4 — semaine 33
25 événements
ICS + CSV
```

---

# 52. Actions dans l’historique

Pour une conversion :

- consulter ;
- regénérer ;
- ouvrir le dossier ;
- supprimer de l’historique.

Éviter de conserver automatiquement les fichiers Excel sources si cela n’est pas nécessaire.

---

# 53. Gestion des doublons

Le logiciel doit détecter les événements identiques.

Critères possibles :

```text
date
heure de début
heure de fin
intitulé
```

En cas de doublon :

```text
Deux événements identiques ont été détectés.

Conserver les deux
Conserver un seul
Examiner
```

---

# 54. Chevauchements

Détecter les événements se chevauchant.

Exemple :

```text
Neurologie : 10h00-12h00
Traumatologie : 11h30-13h00
```

Afficher un avertissement mais ne pas supprimer les événements automatiquement.

---

# 55. Dates incohérentes

Détecter :

- fin avant début ;
- journée impossible ;
- date invalide ;
- horaire invalide.

Exemple :

```text
L’heure de fin 09h00 est antérieure à l’heure de début 10h00.
```

---

# 56. Données obligatoires

Pour exporter un événement calendrier, exiger au minimum :

- une date ;
- une heure de début ;
- une heure de fin ;
- un titre.

Les autres champs sont facultatifs.

---

# 57. Gestion des événements sans horaire

Prévoir éventuellement un mode :

**Événement journée entière**

Exemple :

```text
Journée pédagogique
```

Ne l’utiliser que lorsqu’il est clairement indiqué qu’il s’agit d’une journée entière.

---

# 58. Accessibilité générale

L’accessibilité doit être considérée comme une exigence fonctionnelle majeure.

---

# 59. Navigation clavier

Toutes les fonctions doivent être accessibles avec :

- Tab ;
- Maj + Tab ;
- Entrée ;
- Espace ;
- touches fléchées ;
- Échap lorsque pertinent.

---

# 60. Ordre de tabulation

L’ordre doit correspondre à l’ordre logique de lecture.

Exemple :

```text
Titre
Description
Champ 1
Champ 2
Bouton précédent
Bouton suivant
```

---

# 61. Lecteurs d’écran

L’application doit être compatible autant que possible avec :

- Narrateur Windows ;
- NVDA ;
- JAWS.

Utiliser correctement les API d’accessibilité Windows / UI Automation disponibles dans le framework choisi.

---

# 62. Noms accessibles

Tous les contrôles doivent avoir un nom accessible explicite.

Mauvais :

```text
Bouton sans nom
```

Correct :

```text
Sélectionner un fichier Excel
```

---

# 63. Icônes

Aucune action ne doit être représentée uniquement par une icône.

Mauvais :

icône corbeille seule.

Correct :

```text
Supprimer
```

avec éventuellement une icône.

---

# 64. Messages de statut

Les changements importants doivent être annoncés par les lecteurs d’écran.

Exemple :

```text
Analyse terminée. 28 événements détectés. 2 éléments à vérifier.
```

---

# 65. Contraste

Respecter un contraste élevé entre :

- texte et arrière-plan ;
- boutons et arrière-plan ;
- états actifs ;
- erreurs.

Ne jamais utiliser uniquement la couleur pour transmettre une information.

---

# 66. Taille des éléments

Les boutons doivent disposer d’une zone suffisamment grande pour les utilisateurs de souris, écrans tactiles ou personnes ayant une faible vision.

---

# 67. Zoom

L’interface doit rester utilisable avec l’agrandissement de Windows.

Les textes ne doivent pas être coupés lorsque la mise à l’échelle du système augmente.

---

# 68. Interface visuelle

Style souhaité :

- moderne ;
- sobre ;
- professionnel ;
- peu chargé ;
- lisible.

Préférer :

- fond clair ;
- cartes simples ;
- grands espacements ;
- typographie sans serif lisible ;
- boutons clairement identifiables.

---

# 69. Mode sombre

Option souhaitable mais non indispensable à la première version.

Respecter éventuellement le thème Windows.

---

# 70. Notifications

Éviter les notifications inutiles.

Utiliser des messages uniquement lorsqu’une action mérite l’attention.

---

# 71. Paramètres

Catégories :

```text
Général
Export
Calendrier
Accessibilité
Modèles
Données
```

---

# 72. Paramètres généraux

Exemples :

```text
Langue
Dossier de sortie
Ouvrir automatiquement le dossier après export
Mémoriser le dernier modèle utilisé
```

---

# 73. Paramètres calendrier

```text
Fuseau horaire
Format du titre
Inclure enseignant dans le titre
Inclure salle dans la description
Inclure UE
Inclure groupe
```

---

# 74. Paramètres d’accessibilité

```text
Taille du texte
Mode contraste renforcé
Réduire les animations
Interface simplifiée
```

Le logiciel doit également respecter les réglages Windows lorsque possible.

---

# 75. Interface simplifiée

Prévoir un mode volontairement minimal :

```text
Sélectionner le fichier
Analyser
Vérifier
Exporter
```

Les fonctions avancées restent accessibles depuis les paramètres.

---

# 76. Gestion de la confidentialité

Le planning peut contenir :

- noms d’enseignants ;
- groupes étudiants ;
- salles ;
- informations internes.

Le logiciel ne doit pas transmettre ces données à un service externe sans consentement explicite.

---

# 77. Fonctionnement sans compte

Aucun compte utilisateur ne doit être nécessaire dans la version initiale.

---

# 78. Fonctionnement hors connexion

Les fonctions suivantes doivent fonctionner hors ligne :

- lecture Excel ;
- analyse ;
- correction ;
- génération ICS ;
- génération CSV ;
- génération TXT ;
- génération Excel accessible ;
- historique local.

---

# 79. Intelligence artificielle

L’intelligence artificielle ne doit pas être obligatoire.

Le moteur principal doit utiliser :

- lecture structurée des cellules ;
- règles ;
- reconnaissance de formats ;
- modèles enregistrés ;
- heuristiques.

---

# 80. Module IA optionnel futur

Une IA pourrait être proposée ultérieurement pour :

- plannings très irréguliers ;
- reconnaissance de colonnes inconnues ;
- PDF ;
- images ;
- OCR ;
- interprétation de texte complexe.

Elle doit rester un module séparé.

---

# 81. Principe de sécurité de l’analyse

Si le logiciel hésite entre plusieurs interprétations, il ne doit pas choisir silencieusement.

Il doit produire :

```text
Information à vérifier
```

---

# 82. Journal technique

Prévoir un journal local permettant de diagnostiquer les problèmes.

Ne pas y enregistrer inutilement des données personnelles.

Informations possibles :

```text
version du logiciel
type de fichier
étape ayant échoué
message technique
date de l’erreur
```

---

# 83. Gestion des erreurs

Les messages destinés à l’utilisateur doivent être compréhensibles.

Mauvais :

```text
NullReferenceException 0x0023
```

Correct :

```text
Le fichier n’a pas pu être analysé.

La feuille sélectionnée ne contient pas suffisamment d’informations pour identifier un planning.
```

Un bouton peut permettre :

```text
Afficher les détails techniques
```

---

# 84. Fichier Excel ouvert dans Excel

Le logiciel doit gérer proprement le cas où le fichier est déjà ouvert.

Éviter de modifier ou verrouiller inutilement le document.

---

# 85. Fichier protégé

Si un classeur ne peut pas être lu :

```text
Ce fichier semble être protégé ou chiffré.

Ouvrez-le dans Excel et enregistrez une copie accessible avant de réessayer.
```

---

# 86. Fichier corrompu

Afficher :

```text
Le fichier Excel semble endommagé ou incomplet.
```

Le logiciel ne doit pas planter.

---

# 87. Architecture logicielle

Séparer au minimum :

```text
Interface utilisateur
Lecture Excel
Analyse / parsing
Validation
Modèle de données
Export ICS
Export CSV
Export TXT
Export Excel
Historique
Paramètres
Journalisation
```

---

# 88. Architecture recommandée

Utiliser une architecture modulaire permettant de modifier le moteur d’analyse sans réécrire l’interface.

Approche possible :

```text
UI
↓
Application Services
↓
Parsing Engine
↓
Domain Model
↓
Exporters
↓
Storage
```

---

# 89. Choix technologique

Une implémentation en C#/.NET est particulièrement adaptée à une application Windows.

Framework UI possible :

- WPF ;
- WinUI ;
- autre framework Windows offrant une bonne prise en charge de UI Automation.

Le choix final devra privilégier :

1. accessibilité ;
2. stabilité ;
3. facilité de déploiement ;
4. maintenance ;
5. prise en charge des fichiers Excel.

Pour la lecture Excel, utiliser une bibliothèque capable de lire les fichiers sans nécessiter l’installation de Microsoft Excel.

Les versions et licences des bibliothèques doivent être vérifiées au moment du développement.

---

# 90. Absence de dépendance à Microsoft Excel

Microsoft Excel ne doit pas être obligatoire pour utiliser le logiciel.

---

# 91. Stockage des paramètres

Utiliser un stockage local simple.

Exemple conceptuel :

```text
settings.json
templates.json
history.db
```

Ne jamais stocker de mot de passe en clair.

---

# 92. Données d’historique

Exemple :

```json
{
  "conversionId": "uuid",
  "date": "2026-08-19T10:30:00",
  "planningName": "MK4",
  "startDate": "2026-09-14",
  "endDate": "2026-09-18",
  "eventCount": 28,
  "exports": ["ics", "csv", "txt"]
}
```

---

# 93. Algorithme général d’analyse

Pseudo-processus :

```text
ouvrir le classeur

identifier les feuilles pertinentes

pour chaque feuille :
    lire les cellules
    détecter les zones contenant des données
    détecter les en-têtes
    identifier jours et dates
    identifier horaires
    identifier intitulés
    identifier enseignants
    identifier salles
    propager les valeurs fusionnées
    créer les événements candidats

pour chaque événement :
    normaliser date
    normaliser horaires
    vérifier cohérence
    calculer statut

afficher résultats
```

---

# 94. Conservation de la source

Chaque événement doit conserver une référence à :

```text
feuille
ligne
cellules source
texte original
```

Cela permettra de comprendre l’origine d’une erreur.

---

# 95. Aperçu de la source

Dans l’écran de correction, prévoir :

**Voir les données Excel d’origine**

Cela peut afficher :

```text
Feuille : MK4
Ligne : 17

B17 : Mercredi
C17 : 14h-16h
D17 : Rhumatologie
E17 : Dr Dupont
```

---

# 96. Détection d’une promotion

Le moteur peut rechercher :

```text
MK1
MK2
MK3
MK4
MK5
L1
L2
L3
M1
M2
```

Cette valeur peut servir à nommer les fichiers.

---

# 97. Gestion des groupes

Un événement peut concerner :

```text
Tous
Groupe A
Groupe B
Groupe 1
Groupe 2
```

Ne pas supprimer cette information.

---

# 98. Filtrage par groupe

Fonction avancée souhaitable :

```text
Exporter uniquement :
Tous les groupes
Groupe A
Groupe B
```

Cela permettrait au secrétariat de produire plusieurs calendriers.

---

# 99. Plusieurs promotions

Fonction future :

un seul fichier Excel pourrait contenir plusieurs promotions.

Le logiciel pourrait produire :

```text
Planning_MK3.ics
Planning_MK4.ics
Planning_MK5.ics
```

---

# 100. Prévention des doubles importations

Si le même planning est reconverti :

```text
Un planning similaire a déjà été converti le 18 août 2026.

Continuer
Voir la conversion précédente
Annuler
```

---

# 101. Mise à jour d’un planning

Prévoir l’architecture pour une future fonction permettant de comparer :

```text
planning initial
planning corrigé
```

et détecter :

- cours ajoutés ;
- cours supprimés ;
- horaires modifiés ;
- salles modifiées ;
- enseignants modifiés.

---

# 102. Versionnement futur des calendriers

Une future version pourrait générer des UID stables afin que les événements modifiés puissent être mis à jour au lieu d’être dupliqués.

Cette possibilité doit être anticipée lors de la conception du modèle de données.

---

# 103. Annulation

Les actions importantes doivent pouvoir être annulées lorsque pertinent.

Exemple :

```text
Événement supprimé.

Annuler
```

---

# 104. Confirmation de suppression

Ne pas afficher de confirmation pour chaque petite modification.

Demander confirmation uniquement pour les actions réellement destructrices :

```text
Supprimer ce modèle ?
Supprimer tout l’historique ?
```

---

# 105. Sauvegarde automatique

Les corrections réalisées pendant une conversion peuvent être sauvegardées temporairement.

Si le logiciel est fermé accidentellement :

```text
Une conversion non terminée a été trouvée.

Reprendre
Supprimer
```

---

# 106. Raccourcis clavier

Prévoir éventuellement :

```text
Ctrl + O : ouvrir un fichier
Ctrl + E : exporter
Ctrl + S : enregistrer
Ctrl + C : copier le planning texte
Ctrl + , : paramètres
F1 : aide
```

Les raccourcis doivent être documentés.

---

# 107. Aide intégrée

Prévoir une page simple :

```text
1. Sélectionnez le fichier Excel.
2. Vérifiez les événements détectés.
3. Corrigez les éventuelles erreurs.
4. Choisissez les formats.
5. Cliquez sur Exporter.
```

---

# 108. Premier lancement

Au premier lancement, ne pas imposer un long tutoriel.

Afficher simplement :

```text
Bienvenue.

Cette application transforme vos plannings Excel en calendriers compatibles avec les principales applications d’agenda.

Commencer
```

Option :

```text
Ne plus afficher
```

---

# 109. Tests unitaires

Tester notamment :

- conversion des dates ;
- conversion des heures ;
- plages horaires ;
- propagation des cellules fusionnées ;
- parsing des jours ;
- génération ICS ;
- génération CSV ;
- détection des doublons ;
- chevauchements.

---

# 110. Tests avec fichiers réels

Créer un corpus de fichiers anonymisés représentant :

- tableau simple ;
- tableau avec cellules fusionnées ;
- plusieurs jours ;
- plusieurs feuilles ;
- horaires dans une seule cellule ;
- dates réparties dans les en-têtes ;
- données manquantes ;
- doublons ;
- planning très irrégulier.

---

# 111. Tests d’accessibilité

Tester réellement avec :

- navigation clavier seule ;
- Narrateur Windows ;
- NVDA ;
- grossissement Windows ;
- mise à l’échelle 125 % ;
- 150 % ;
- 200 %.

---

# 112. Critères d’acceptation du clavier

L’utilisateur doit pouvoir :

1. lancer le programme ;
2. sélectionner un fichier ;
3. analyser ;
4. parcourir les événements ;
5. corriger un événement ;
6. exporter ;
7. ouvrir le dossier résultat ;

sans utiliser la souris.

---

# 113. Tests ICS

Tester les fichiers générés avec plusieurs calendriers lorsque possible.

Vérifier notamment :

- accents ;
- caractères spéciaux ;
- apostrophes ;
- fuseau horaire ;
- retours à la ligne ;
- durée ;
- lieux ;
- descriptions.

---

# 114. Encodage

Tous les formats texte doivent privilégier UTF-8.

Tester notamment :

```text
Éducation
Rhumatologie
M. N’Guessan
Amphithéâtre
Ça
Œ
```

---

# 115. Performance

Objectif :

un planning hebdomadaire classique doit être analysé rapidement.

Le logiciel ne doit pas charger l’intégralité d’un classeur immense en mémoire lorsque cela n’est pas nécessaire.

---

# 116. Barre de progression

Pour les opérations longues :

```text
Analyse de la feuille MK4…
```

ou :

```text
Création des fichiers…
```

Ne pas utiliser de pourcentage artificiel si la durée n’est pas réellement mesurable.

---

# 117. Installation

Prévoir un installateur simple.

Idéalement :

```text
Télécharger
Installer
Lancer
```

Éviter les dépendances techniques à installer manuellement.

---

# 118. Mise à jour

Prévoir une architecture permettant les mises à jour futures.

La première version peut utiliser une mise à jour manuelle.

---

# 119. Version portable

Option future intéressante pour une université :

```text
PlanningConverter.exe
```

pouvant éventuellement fonctionner depuis un dossier sans installation complexe.

---

# 120. Droits administrateur

Le logiciel ne doit pas nécessiter les droits administrateur pour son fonctionnement quotidien.

---

# 121. Structure visuelle recommandée

Fenêtre principale :

```text
--------------------------------------------------
Planning Converter

Accueil
Historique
Modèles
Paramètres

        Convertir un planning

        Déposez votre fichier Excel ici

        [ Sélectionner un fichier ]

--------------------------------------------------
```

---

# 122. Interface pendant conversion

```text
1 Importer    2 Vérifier    3 Exporter

Planning MK4
Semaine du 14 au 18 septembre

28 événements
2 à vérifier

Lundi 14 septembre

08h30 - 10h30
Neurologie
Dr Martin
Salle 204

[ Modifier ]

--------------------------------------------------

[ Retour ]                    [ Continuer ]
```

---

# 123. Responsive desktop

L’interface doit rester exploitable lorsque la fenêtre est redimensionnée.

Éviter les panneaux nécessitant un écran très large.

---

# 124. Architecture de l’interface

Préférer :

- une seule fenêtre principale ;
- navigation interne ;
- peu de fenêtres modales.

Les fenêtres modales sont réservées aux actions nécessitant réellement une décision.

---

# 125. États visuels des boutons

Prévoir clairement :

- normal ;
- survol ;
- focus clavier ;
- actif ;
- désactivé.

Le focus clavier doit être très visible.

---

# 126. Validation des formulaires

Afficher les erreurs près du champ concerné.

Exemple :

```text
Heure de fin

[ 09:00 ]

L’heure de fin doit être postérieure à l’heure de début.
```

---

# 127. Pas de perte de saisie

Si l’utilisateur quitte un formulaire modifié :

```text
Vous avez des modifications non enregistrées.

Enregistrer
Ignorer
Annuler
```

---

# 128. Logs accessibles à l’administrateur

Prévoir :

```text
Paramètres > Diagnostic > Exporter le rapport
```

Le rapport peut être envoyé au développeur en cas de problème.

---

# 129. Télémétrie

Aucune télémétrie obligatoire.

Si une télémétrie est ajoutée ultérieurement, elle doit être :

- transparente ;
- limitée ;
- désactivable ;
- respectueuse du RGPD.

---

# 130. Sécurité

Ne jamais exécuter de macros Excel.

Le fichier doit être traité comme une source de données et non comme un programme.

---

# 131. Protection contre les fichiers malveillants

Prévoir :

- validation des extensions ;
- gestion des erreurs de lecture ;
- limites raisonnables de taille ;
- absence d’exécution de contenu actif.

---

# 132. Sauvegarde des modèles

Les modèles peuvent être exportés/importés.

Exemple :

```text
Planning_IFMKDV_MK4.template.json
```

Cela facilite le déploiement sur plusieurs ordinateurs du secrétariat.

---

# 133. Mode administrateur facultatif

Certaines fonctions avancées peuvent être regroupées dans :

```text
Paramètres avancés
```

afin de ne pas surcharger l’utilisateur quotidien.

---

# 134. Philosophie UX

À chaque écran, le logiciel doit répondre à trois questions :

```text
Où suis-je ?
Que vient-il de se passer ?
Que dois-je faire maintenant ?
```

---

# 135. Priorité de l’information

Toujours afficher d’abord :

1. le résultat principal ;
2. les éventuelles erreurs ;
3. l’action recommandée.

Éviter d’afficher des informations techniques inutiles.

---

# 136. Vocabulaire

Utiliser un vocabulaire compréhensible.

Préférer :

```text
Événement
Cours
Planning
Exporter
Fichier calendrier
```

Éviter dans l’interface principale :

```text
Parser
Schema
Serialization
VEVENT
DTO
```

---

# 137. Version minimale viable — MVP

La première version fonctionnelle doit contenir :

- application Windows ;
- import XLSX/XLS ;
- sélection de feuille ;
- reconnaissance de jours ;
- reconnaissance de dates ;
- reconnaissance d’horaires ;
- reconnaissance de plages horaires ;
- gestion de cellules fusionnées ;
- reconnaissance cours/enseignant/salle ;
- aperçu accessible ;
- modification manuelle ;
- détection des erreurs essentielles ;
- export ICS ;
- export CSV ;
- export TXT ;
- historique local simple ;
- paramètres essentiels ;
- navigation entièrement clavier ;
- compatibilité lecteur d’écran.

---

# 138. Version 1.1

Fonctions envisageables :

- modèles enregistrés ;
- Excel accessible ;
- copie presse-papiers ;
- meilleure détection automatique ;
- préparation d’e-mails ;
- gestion avancée des groupes.

---

# 139. Version 2

Fonctions envisageables :

- PDF ;
- OCR ;
- images ;
- IA locale ou distante ;
- comparaison de plannings ;
- mises à jour de calendriers existants ;
- envoi automatisé par e-mail ;
- intégration Microsoft 365 ;
- intégration Google Workspace ;
- calendriers par promotion ;
- synchronisation directe.

---

# 140. Critères de réussite du projet

Le logiciel sera considéré comme réussi si une personne du secrétariat peut recevoir un fichier Excel classique et, sans formation technique particulière :

```text
ouvrir le logiciel
sélectionner le fichier
vérifier le planning
corriger les rares erreurs
générer un ICS
générer un CSV
générer une version texte
envoyer les fichiers aux étudiants
```

en quelques minutes.

---

# 141. Exigence essentielle d’expérience utilisateur

Le logiciel ne doit pas chercher à reproduire Excel.

Il doit transformer une structure pensée comme un tableau en une structure pensée comme une succession d’événements.

L’objectif n’est donc pas :

```text
Excel → nouveau tableau
```

mais :

```text
Excel
↓
Compréhension du planning
↓
Événements structurés
↓
Calendriers et formats accessibles
```

---

# 142. Exigence essentielle d’accessibilité

L’accessibilité ne doit pas être ajoutée après le développement.

Elle doit influencer :

- la structure des écrans ;
- le choix des contrôles ;
- la navigation ;
- les messages ;
- le modèle de données ;
- les tests ;
- les technologies choisies.

Une fonctionnalité qui n’est pas accessible au clavier ou à un lecteur d’écran doit être considérée comme incomplète.

---

# 143. Directive générale pour l’intelligence artificielle chargée du développement

Développer le logiciel de manière incrémentale.

Ordre recommandé :

```text
1. modèle de données
2. lecture Excel
3. moteur de normalisation
4. détection des événements
5. validation
6. export ICS
7. export CSV
8. export TXT
9. interface minimale
10. édition des événements
11. accessibilité
12. modèles
13. historique
14. tests
15. packaging Windows
```

Ne pas développer immédiatement les fonctionnalités futures telles que l’OCR ou une IA distante.

---

# 144. Directive de qualité du code

Le code doit être :

- modulaire ;
- documenté ;
- testable ;
- maintenable ;
- séparé par responsabilité.

Éviter les fichiers de plusieurs milliers de lignes regroupant toute l’application.

---

# 145. Directive de robustesse

Aucune erreur de fichier Excel ne doit provoquer la fermeture brutale de l’application.

Toutes les erreurs doivent être interceptées et transformées en messages compréhensibles.

---

# 146. Directive de génération des fichiers

Avant de confirmer une exportation réussie, le logiciel doit vérifier que les fichiers ont effectivement été créés et sont lisibles.

---

# 147. Critères d’acceptation finaux

Un fichier Excel contenant par exemple :

```text
Lundi
08h30 - 10h30
Neurologie
Dr Martin
Salle 204
```

doit pouvoir devenir automatiquement un événement :

```text
Date : lundi correspondant à la semaine du planning
Début : 08:30
Fin : 10:30
Titre : Neurologie — Dr Martin
Lieu : Salle 204
```

et apparaître correctement après importation du fichier ICS dans un calendrier compatible.

---

# 148. Vision finale du produit

Le produit final doit donner l’impression d’un utilitaire extrêmement simple :

```text
Je donne mon fichier Excel.
Le logiciel comprend le planning.
Je vérifie les éventuelles anomalies.
Je clique sur Exporter.
Je récupère immédiatement les fichiers destinés aux étudiants.
```

Toute décision de conception doit être évaluée à partir de cette philosophie.

Le logiciel doit prioritairement être :

**simple, fiable, accessible, rapide, local, compréhensible et difficile à mal utiliser.**
