# Planning Converter — Cahier des charges fonctionnel et technique final

Édition de référence du 5 novembre 2026. Application de bureau Windows destinée au secrétariat d’un institut de formation accueillant des étudiants déficients visuels. Langue de l’interface et des documents produits : français.

## Sommaire

1. [Mandat et mode de lecture](#chapitre-1)
2. [Utilisateurs, périmètre et livraison initiale](#chapitre-2)
3. [Règles qui gouvernent une conversion](#chapitre-3)
4. [Entrée Excel et règles de lecture](#chapitre-4)
5. [Vérification, décision humaine et export partiel](#chapitre-5)
6. [Sessions, historique et actualisation](#chapitre-6)
7. [Contrat commun des formats de sortie](#chapitre-7)
8. [Spécification de chaque format](#chapitre-8)
9. [Parcours et écrans à développer](#chapitre-9)
10. [Conception visuelle et accessibilité](#chapitre-10)
11. [Exemple complet de fonctionnement attendu](#chapitre-11)
12. [État des sources et écarts à traiter](#chapitre-12)
13. [Architecture de réalisation proposée](#chapitre-13)
14. [Plan de développement et critères de passage](#chapitre-14)
15. [Recette fonctionnelle et technique](#chapitre-15)
16. [Conditions de remise et questions de terrain](#chapitre-16)

<a id="chapitre-1"></a>

## 1. Mandat et mode de lecture

Le secrétariat dépose aujourd’hui plusieurs fichiers Excel, chacun correspondant, pour les MK4, à une semaine de cours. La même difficulté se répète pour l’étudiant : repérer son groupe, retrouver le bon jour, suivre des horaires parfois répartis sur deux lignes et associer chaque séance au bon lieu. Le résultat attendu est concret. Le secrétariat sélectionne la promotion et les semaines ; un étudiant peut ensuite ouvrir le planning de son groupe et retrouver seul le prochain cours, son heure et sa salle.

Ce document consolide le cahier initial, les échanges de conception, le compte rendu de développement et la solution C# fournie, les deux maquettes sombres et les décisions prises ensuite avec Bruno. Les règles formulées ici priment sur les exemples anciens. Les maquettes sont des inspirations : un bouton grisé ou un exemple de fichier unique dans une image ne vaut pas règle de fonctionnement. Les 22 événements d’une maquette illustrent un écran ; ils ne constituent pas un résultat mesuré sur la semaine dessinée.

Les chapitres 2 à 11 spécifient le produit attendu. Les chapitres 12 à 16 décrivent le développement et la recette. Les annexes donnent des précisions applicables et des exemples. Une « proposition » désigne un choix de réalisation que l’équipe peut adapter si le résultat utilisateur reste équivalent. Un « point terrain » demande une observation sur le poste du secrétariat ou sur des documents représentatifs. Le présent cahier décrit une cible à développer. L’existence d’un fichier source ne prouve ni la compilation ni le bon fonctionnement du logiciel.

<a id="chapitre-2"></a>

## 2. Utilisateurs, périmètre et livraison initiale

**La personne du secrétariat** prépare seule les plannings. Elle connaît ses documents, décide en dernier ressort et doit pouvoir revenir sur une correction. Elle ne devrait avoir à connaître ni le format iCalendar, ni un numéro de cellule Excel, ni les règles du moteur pour mener une conversion habituelle. Sa tâche se termine lorsqu’elle retrouve dans un dossier les bons fichiers, prêts à être déposés dans l’espace partagé ou joints à son propre courriel.

**L’étudiant** reçoit le résultat pour son groupe. Il peut utiliser un iPhone, un appareil Android, un ordinateur, la synthèse vocale, le grossissement ou une copie imprimée. Il n’installe pas Planning Converter pour consulter son planning. L’ordre des informations et la lisibilité du résultat comptent autant que l’accessibilité de la fenêtre de conversion.

**La personne qui installe ou maintient le logiciel** doit disposer d’une version identifiable, d’un paquet Windows reproductible, d’un jeu d’essais anonymisé et d’informations de diagnostic compréhensibles. Le fonctionnement quotidien est local, hors connexion, sans compte, sans envoi automatique des plannings et sans installation obligatoire d’Excel.

**Livraison initiale visée.** Lecture XLS et XLSX, traitement par promotion et par période, séparation des groupes, vérification et corrections, historique et reprise, exports ICS, TXT, CSV et PDF en gros caractères, écriture locale et ouverture du dossier. L’accessibilité de bout en bout fait partie de la livraison initiale. L’éventuel Excel normalisé est une évolution souhaitée du cahier initial. L’OCR, la lecture de PDF en entrée, une IA distante, la synchronisation de calendriers, l’envoi d’e-mails intégré et un espace collaboratif ne sont pas nécessaires à cette livraison. Produire un PDF est bien une fonction initiale ; analyser un planning reçu en PDF serait une autre fonction.

<a id="chapitre-3"></a>

## 3. Règles qui gouvernent une conversion

**PROM-01 — Une promotion par conversion.** Choisir la promotion avant de constituer le lot. Toute vue de vérification ou de reprise rappelle cette promotion. Si le nom indique MK4 et qu’une feuille ou un en-tête indique sans ambiguïté MK3, présenter les indices contradictoires avant de rattacher quoi que ce soit. Un fichier clairement rattaché à une autre promotion est écarté et nommé à l’écran. Si la promotion reste inconnue, demander une confirmation explicite portant sur ce fichier. Cette décision est mémorisée pour sa version et sa session.

**PER-01 — Une ou plusieurs semaines, un résultat par groupe.** Chaque fichier hebdomadaire peut être sélectionné seul ou avec d’autres fichiers. Les ordonner d’après les dates du planning, indépendamment de l’ordre de sélection. Trois semaines de MK4 produisent, pour chaque format choisi, un planning A et un planning B couvrant toute la période. Aucune question « fusionner ou séparer les semaines ? » n’est nécessaire.

**PER-02 — Deux versions de la même semaine.** Montrer les deux noms, les dates couvertes et, si disponible, leur date de modification. La secrétaire choisit la source à utiliser. Une date de modification plus récente peut provenir d’une simple copie ; elle ne constitue pas un arbitrage automatique. Distinguer ce conflit de la sélection répétée du même fichier, qui peut être signalée et dédupliquée avant lecture.

**PER-03 — Semaine intermédiaire absente.** Si les semaines 46 et 48 sont présentes, signaler à la secrétaire que la 47 manque. Proposer d’ajouter un fichier ou de poursuivre. Les étudiants voient uniquement les semaines fournies, avec leurs dates. Aucun texte « planning non fourni », aucune semaine vide artificielle et aucun événement calendrier de remplissage. Tenir compte du changement d’année lors du calcul des semaines.

**GRP-01 — Cas MK4 validé.** Un cours explicitement A va dans A ; un cours explicitement B va dans B ; une séance sans mention de groupe est commune et figure une fois dans chaque planning. L’utilisateur ne choisit pas entre sortir toute la promotion et sortir les deux groupes. L’aperçu d’un seul groupe change uniquement la consultation. Une mention inhabituelle ne doit pas introduire un parcours spécial consacré au « groupe illisible » ; conserver le texte original pour la vérification ordinaire.

**GRP-02 — Autres promotions.** L’application doit pouvoir traiter plusieurs promotions dans des sessions distinctes. Les règles MK4 ne prouvent pas que les autres promotions ont deux groupes ni que l’absence de mention a le même sens partout. Lors du premier usage, présenter les groupes repérés, permettre de confirmer leur organisation et conserver le réglage pour cette promotion. Une promotion confirmée comme indivisée donne un planning unique. La généralisation du modèle sera validée sur une autre promotion réelle avant d’annoncer sa compatibilité. Si un enseignement concerne plusieurs groupes explicitement nommés, rattacher un seul événement source à plusieurs destinataires.

**FID-01 — Fidélité du contenu.** Pour chaque séance diffusée, conserver les informations utiles qui figurent dans le classeur : libellé exact, UE, intervenants, lieu, groupe, remarques, consignes et précisions propres à la séance. Conserver également les champs complémentaires que le logiciel ne sait pas classer sous une rubrique prédéfinie, avec leur libellé source lorsqu’il est disponible. Ne transformer ni une cellule vide en valeur inventée, ni un code incertain en nom de salle affirmé. Le texte d’origine reste consultable, même après correction.

<a id="chapitre-4"></a>

## 4. Entrée Excel et règles de lecture

### 4.1 Ouvrir le lot sans modifier les originaux

Le sélecteur accepte plusieurs fichiers et la zone de dépôt offre la même possibilité. Après sélection, exposer un inventaire lisible avec nom, promotion pressentie, semaine détectée ou à préciser, état et action « Retirer ». Un fichier illisible ne doit pas faire perdre les autres sélections. Autoriser une nouvelle tentative si le fichier a été fermé ou rendu disponible depuis un dossier partagé. Si Excel contient des changements en cours non enregistrés, seule la version enregistrée sur disque peut être analysée.

Vérifier la signature du document et sa cohérence avec les formats pris en charge. Traiter séparément fichier absent, accès refusé, protection par mot de passe, format incompatible et classeur endommagé. Ouvrir en lecture seule, libérer les ressources dès la fin de la lecture, ne lancer aucune macro et ne suivre aucun lien externe. Si une formule dispose d’une valeur mise en cache utilisable, lire cette valeur ; préserver la provenance et soumettre les cas non interprétables à examen.

### 4.2 Choisir les feuilles et comprendre la mise en page

Identifier les feuilles utiles et celles qui sont vides. Avec une seule feuille pertinente, poursuivre directement. Avec plusieurs candidates, afficher leur nom et le contexte de promotion ; ne réunir que des feuilles compatibles avec la conversion choisie. Plusieurs feuilles utiles d’un même classeur peuvent décrire des parties complémentaires d’une semaine. Cette sélection est distincte du choix entre deux versions concurrentes de la même semaine.

Le format observé dans trois fichiers MK4 est organisé en sections de jour : une ligne telle que « Lundi 01 Juin 2026 » ouvre un bloc, les séances suivent, puis un nouveau jour ouvre le bloc suivant. Aucun besoin de cellules fusionnées n’a été observé dans ces trois exemples, mais ces cellules font partie des entrées demandées. Une cellule fusionnée s’applique à son rectangle réel. Ne pas prolonger une valeur vers un autre jour, une autre feuille ou un autre fichier. Réinitialiser le contexte aux frontières de bloc.

Reconnaître les en-têtes par leur texte et leurs variantes (« Cours », « Intitulé », « Intervenant », « Salle », « Horaires »). Pour une colonne sans titre, examiner plusieurs valeurs représentatives et conserver la raison du choix. Le seuil de reconnaissance présent dans le prototype est un réglage technique à tester. Si l’interprétation reste ambiguë, présenter un extrait réel de la feuille et permettre de désigner la colonne. Un modèle mémorisé peut accélérer cette étape à condition de vérifier que la structure correspond toujours.

Une présentation en grille avec les jours en colonnes n’a pas été démontrée par le corpus MK4. Identifier une disposition non prise en charge et afficher ce qui empêche sa lecture. Élargir les lecteurs à partir d’exemples réels. Une feuille manifestement remplie qui produit zéro séance doit susciter une explication, même si aucune exception technique n’a été levée.

### 4.3 Dates, horaires, lieu et contenu

Accepter les dates et heures Excel natives ainsi que les formes textuelles françaises. Respecter le système de dates du classeur. Accepter une plage sur une ligne (« 8 h 30 à 10 h ») ou deux heures sur deux lignes dans une même cellule. Les variantes de casse, blancs, tirets et retours à la ligne servent à comparer les valeurs. Conserver pour l’affichage les noms et libellés signifiants, notamment accents et ponctuation.

Établir la date à partir du jour, du bloc de semaine et des indices explicites. Comparer titre de feuille, en-tête et nom du fichier. Si l’année manque et qu’elle ne peut être établie avec fiabilité, demander une seule précision pour le bloc concerné, l’appliquer, puis réévaluer les jours. Une date, un intitulé et des horaires exploitables permettent d’exporter un cours ordinaire. Un horaire inversé, une heure impossible ou une date contradictoire appelle une correction. Ne pas déduire la fin d’un cours du début du suivant.

Un intitulé peut commencer par un code UE. Extraire le code pour la recherche sans tronquer le titre exporté ni le doubler. La colonne « Salle » peut contenir un lieu, une mention de groupe ou les deux. Les règles spécifiques aux documents observés doivent pouvoir évoluer sans réécrire l’export. Si plusieurs enseignants sont indiqués, garder leur ordre et leur orthographe. Un intervenant manquant, notamment pour du travail autonome, et une salle absente n’empêchent pas l’export.

### 4.4 Nature des lignes et portée des informations

Distinguer date de section, séance, consigne à la semaine, jour férié nommé, ligne de pause, ligne vide et note à examiner. Une ligne sans heure ne constitue pas à elle seule une preuve qu’il s’agit d’un titre. Le corpus contient précisément « VICTOIRE 1945 » sans horaire : conserver cet intitulé. Dans un calendrier, un jour férié explicitement nommé peut être représenté comme une journée entière. Éviter l’ajout de fêtes déduites du seul calendrier civil.

Un jour explicitement présent et véritablement vide reçoit dans les sorties lisibles la mention « Aucun cours indiqué ». Distinguer ce cas d’une semaine absente et d’une journée dont le seul cours est temporairement exclu faute d’heure de fin. Dans ce dernier cas, conserver l’information dans l’écran de contrôle, sans annoncer aux étudiants que la journée est libre. Pour un jour qui ne contient que des cours d’un autre groupe, proposition de rendu : « Aucun cours indiqué pour ce groupe », si la journée est conservée dans le document de ce groupe.

Omettre les pauses déjeuner identifiées des plannings diffusés. Une plage horaire vide entre deux cours ne crée pas une pause. Une séance d’enseignement dont le titre évoque l’alimentation reste un cours. Une consigne explicitement valable pour toute une semaine se lit une seule fois avant le premier jour dans les sorties de lecture. Pour l’ICS, le choix délégué est un événement en journée entière le lundi, intitulé « Consigne de la semaine », avec son texte dans les notes et une disponibilité libre. Une consigne propre à une séance reste attachée à la séance.

L’usage d’un fond de couleur, de caractères barrés ou de commentaires pour transmettre une annulation ou un groupe demeure un point terrain. Avant confirmation d’une convention réelle, n’en déduire aucune règle automatique. Permettre la consultation de la source en cas de doute.

<a id="chapitre-5"></a>

## 5. Vérification, décision humaine et export partiel

**VER-01 — Synthèse honnête.** Après analyse, indiquer les fichiers et semaines lus, les séances détectées, les éléments à examiner et les exclusions. Préciser l’unité des compteurs. Un cours commun est une seule séance source mais une entrée dans chacun des deux plannings. « Détecté » ne signifie pas « confirmé par la secrétaire ». Les véritables décomptes par groupe se calculent après application des corrections.

**VER-02 — Source, correction et résultat.** Pour chaque séance, présenter date, horaire et intitulé d’abord, puis les champs disponibles. Une commande donne accès aux valeurs originales, au classeur, à la feuille et aux cellules. La modification change le résultat de la session. Si la secrétaire efface volontairement une valeur, distinguer cet effacement d’un champ qui n’a jamais été corrigé. Une correction du lieu ne doit pas figer le nom de l’enseignant.

**VER-03 — Cours incomplet.** Si un cours commence à 14 h et que sa fin manque, lui attribuer « À compléter ». Le garder dans la session avec un accès « Corriger ». Exclure cette séance des exports aux étudiants jusqu’à correction. Ne pas y insérer de ligne « fin à confirmer ». Afficher au secrétariat, avant l’enregistrement, les exclusions et leurs conséquences par groupe. Le bouton d’export reste disponible pour les autres cours. Une correction valide réintègre la séance lors d’un nouvel export daté. Si aucun cours ni événement publiable ne subsiste pour un groupe, expliquer cet état et distinguer un document de semaine réellement vide d’un export vide dû aux exclusions.

**VER-04 — Chevauchement.** Comparer les intervalles qui concernent au moins un même groupe à la même date. Un cours commun et un TD du groupe A peuvent se chevaucher pour A uniquement. Deux TD simultanés pour A et B sont compatibles. Une séance finissant à 10 h et une séance commençant à 10 h sont consécutives. Présenter les deux séances et les horaires, permettre de corriger ou de confirmer le maintien des deux. Mémoriser la confirmation tant que les données pertinentes restent identiques.

**VER-05 — Doublons.** Deux lignes du même groupe dont le contenu utile est identique doivent pouvoir être comparées, avec leur origine. Proposer « Garder un seul cours » et « Conserver les deux ». Le logiciel ne décide pas seul. Comparer aussi lieu, enseignant et précisions pour éviter d’assimiler à des doublons exacts deux activités différentes qui partagent un horaire. Une détection approximative, si proposée, appelle une vérification humaine. Deux occurrences conservées intentionnellement gardent des identités distinctes.

**VER-06 — Actions.** Autoriser ajouter, modifier, dupliquer, ignorer, réintégrer et supprimer. « Ignorer » garde la séance consultable avec un statut. « Supprimer » la retire de la session active avec possibilité d’annulation. « Dupliquer » crée une nouvelle séance éditable. Après l’opération, recalculer les contrôles et l’affectation par groupe sans revenir au début de la liste. Une édition manuelle suit les mêmes règles de validation qu’un cours importé.

**VER-07 — Filtrer sans modifier.** Recherche par intitulé, UE, enseignant, salle ou groupe. Filtres par semaine, feuille si utile, éléments à vérifier et éléments ignorés. Les filtres de consultation ne modifient jamais les destinataires d’un export. Dans l’aperçu d’un groupe, afficher les cours communs et les cours propres à ce groupe, dans l’ordre exact de diffusion. Une secrétaire qui consulte le groupe B retrouve bien les deux groupes à l’étape d’export.

<a id="chapitre-6"></a>

## 6. Sessions, historique et actualisation

Une session enregistre la promotion, les fichiers associés à chaque semaine, les versions lues, les valeurs importées, les corrections par champ, les décisions de validation, les fichiers produits et la position de travail. Elle peut durer plusieurs jours. L’historique appartient à l’utilisateur Windows du poste. Le scénario validé est celui d’une personne qui prépare les plannings ; aucune coordination simultanée entre plusieurs postes n’est nécessaire.

**REP-01 — Reprendre là où l’on s’est arrêté.** Sauvegarder après les opérations validées. Mémoriser étape, semaine, filtres utiles et identifiant du cours consulté. Au démarrage, proposer la reprise avec promotion, période et date de dernière modification. Si le cours repère a disparu, revenir à son jour ou à la liste en annonçant ce changement. Une saisie encore ouverte demande « Enregistrer », « Abandonner la saisie » ou « Continuer la saisie » en cas de fermeture normale. Une sauvegarde de brouillon récupérée après interruption doit être présentée comme telle.

**REP-02 — Réutiliser une période.** Depuis l’historique, ouvrir l’ensemble d’une période sans resélectionner les classeurs. Utiliser les données enregistrées pour consulter et réexporter même si un original a été déplacé. Pour actualiser depuis Excel, proposer de retrouver le fichier manquant. « Ajouter une semaine » élargit la même période, applique les contrôles de promotion, de semaine dupliquée et de trou chronologique, puis conserve les corrections préexistantes. Les exports antérieurs restent repérables avec la période qu’ils couvraient au moment de leur création.

**REP-03 — Reconnaître une nouvelle version.** Comparer l’identité de la semaine et l’empreinte du contenu. Une copie identique renommée ne devient pas une nouvelle semaine. Une version différente au même emplacement est signalée à la réouverture. L’action « Actualiser cette semaine » est volontaire. Dans le menu de la semaine, « Remplacer le fichier de cette semaine » traite le cas d’un fichier corrigé sous un autre nom. Contrôler promotion et dates avant substitution.

**REP-04 — Priorité des corrections.** Conserver séparément la valeur source et la valeur corrigée. Lors d’une actualisation, pour un cours retrouvé, appliquer les nouvelles valeurs Excel aux champs non corrigés. Utiliser la dernière correction de Planning Converter pour chaque champ corrigé, sans question supplémentaire. Exemple : la secrétaire a saisi 16 h comme heure de fin ; le nouveau classeur donne 16 h 30 et déplace le cours en salle 204. La sortie affiche 16 h et salle 204 si la salle n’avait pas été corrigée.

**REP-05 — Rapprochement des séances.** Proposition technique : utiliser une identité interne persistante, une empreinte de la source et plusieurs indices de rapprochement. Une coordonnée de cellule seule casse lorsque le secrétariat insère une ligne. Un intitulé seul confond deux TD récurrents. Rechercher d’abord les correspondances certaines, puis comparer date, créneau, groupe, libellé et voisinage des lignes. Une ambiguïté ne doit jamais transférer une correction d’un cours à un autre. Montrer les deux sources à la secrétaire si une association exige un choix.

**REP-06 — Cours corrigé disparu.** Afficher l’ancien cours, sa date, son groupe et sa correction. Proposer « Conserver le cours » ou « Retirer des prochains exports ». Le conserver crée ou maintient une séance locale traçable. Le retirer ne modifie aucun fichier déjà diffusé. Les nouveaux cours du classeur sont intégrés et contrôlés normalement. Un cours source supprimé sans correction locale disparaît de la période actualisée. Aucun bulletin automatique des changements ni notification aux étudiants n’est requis.

**REP-07 — Sécurité des opérations.** Préparer et valider l’actualisation avant de remplacer l’état enregistré. Une lecture échouée, une annulation ou un manque d’espace doit laisser la dernière session utilisable. Les exports constituent des instantanés datés ; restaurer ou annuler une correction affecte le travail courant et les exports futurs. Retirer une entrée de l’historique ne supprime pas les exports déjà présents sur disque.

<a id="chapitre-7"></a>

## 7. Contrat commun des formats de sortie

Un même résultat validé alimente tous les exportateurs. Pour chaque groupe, sélectionner les événements communs et ceux qui lui sont destinés, retirer les exclusions, puis ordonner par date et heure locale. En cas d’égalité, garder un ordre stable. Les jours explicitement décrits et les consignes hebdomadaires ont leur propre représentation. Les sorties d’une même opération utilisent la même version de session, même si l’utilisateur reprend ensuite l’édition.

ICS et TXT sont cochés par défaut. CSV est disponible à la demande. Le PDF en gros caractères est décoché par défaut. Pour les MK4, deux formats cochés donnent quatre fichiers, trois formats en donnent six et quatre formats en donnent huit, pour toute la période. Indiquer ce nombre et les destinataires avant écriture. Le nom du format doit être accompagné de son usage.

Choisir le dossier avant de générer. Afficher son chemin lisible et proposer « Changer de dossier ». La mémorisation du dernier dossier est facultative. Une clé USB retirée, un chemin réseau inaccessible et un dossier non inscriptible doivent conduire à une action concrète : choisir une autre destination ou réessayer. Proposer « Ouvrir le dossier » à la fin. Le secrétariat peut ensuite joindre les fichiers à un courriel depuis sa messagerie habituelle ; aucune liste de diffusion n’est nécessaire à la conversion.

**Nommage proposé.** Inclure promotion, groupe, bornes de la période et date de génération. Exemple fictif : `MK4_Groupe-A_2026-11-09_2026-11-20_export-2026-11-05_1430.ics`. La même racine sert aux autres extensions. Prévoir suffixe ou précision temporelle en cas de deuxième export au même instant. Vérifier la disponibilité lors de la création effective, y compris en cas de relance. Préserver chaque ancienne version. Les noms techniques et chemins internes des sources n’apparaissent pas dans les notes des calendriers étudiants.

Écrire chaque fichier dans un emplacement temporaire sur le même volume, le fermer, vérifier sa lisibilité, puis le publier sous son nom définitif. L’écran final distingue succès complet, partiel et échec. En cas d’échec pour le PDF B après cinq réussites, nommer les cinq fichiers présents et le fichier en échec. « Réessayer les fichiers en échec » doit réutiliser le même contenu figé. Un nouvel export volontaire recrée une nouvelle série datée. Une annulation ne doit jamais laisser un document tronqué sous un nom prêt à diffuser.

<a id="chapitre-8"></a>

## 8. Spécification de chaque format

### 8.1 Calendrier ICS — Apple Calendrier, Google Agenda et Outlook

Un fichier par groupe, un événement par séance. Le titre réunit l’intitulé exact du cours et les intervenants renseignés. Les heures figurent dans les champs de début et de fin, la salle dans le lieu et le reste dans les notes : UE si utile, groupe, nature du cours, remarques, matériel ou consigne. Si le nom d’un enseignant manque, aucun séparateur suspendu ni ligne vide. Conserver les valeurs utiles tout en évitant de répéter un code UE déjà présent dans le titre.

Utiliser un identifiant d’événement durable qui ne dépende ni du nom de l’export ni de la date à laquelle l’utilisateur clique. Un changement d’horaire du même cours doit pouvoir conserver son identité. Distinguer les événements réellement distincts, même après une duplication intentionnelle ; documenter le choix de portée des identifiants par calendrier de groupe. Actualiser les métadonnées de révision quand le contenu change. La production de fichiers successifs n’est pas un abonnement en ligne. Le résultat d’un second import dépend du logiciel destinataire et doit être observé, notamment pour un cours déplacé ou supprimé.

Décrire le fuseau de l’établissement, par défaut Europe/Paris. Pour une séance de 8 h 30, le fichier importé doit restituer l’heure locale attendue en hiver et en été. Vérifier le bloc de fuseau déjà écrit dans le code avant de s’y fier. Un jour férié nommé devient un événement daté en journée entière avec fin exclusive au jour suivant. Une journée vide n’occupe pas un créneau. Pour les caractères spéciaux, tester virgule, point-virgule, barre oblique inverse, accents et retours à la ligne ; vérifier aussi le pliage des lignes longues. Ouvrir le fichier dans un lecteur indépendant du générateur.

Le critère étudiant sur iPhone est concret : depuis le fichier du bon groupe, importer le calendrier et retrouver avec VoiceOver le prochain cours, son heure et sa salle. Pour Android, reproduire le chemin réel depuis l’espace partagé avec une application disponible sur le téléphone. Documenter le chemin testé, y compris une éventuelle étape via le service d’agenda utilisé. Éviter de promettre qu’un toucher suffit sur tous les appareils. Le TXT reste lisible indépendamment de la réussite de l’import calendrier.

### 8.2 Texte accessible

Produire du texte Unicode en UTF-8. Inscrire l’identité du groupe et de la période puis un repère pour chaque semaine avec numéro et dates. À l’intérieur d’une semaine, annoncer le jour puis directement le premier cours sans sommaire oral ni décompte préalable. Horaire, intitulé, enseignant, salle puis compléments suivent l’ordre de lecture. Garder des lignes courtes et des paragraphes distincts. Éviter tableaux en caractères, colonnes alignées avec des espaces, répétitions de signes et icônes décoratives. Un mode compact éventuel peut réduire les retours à la ligne mais doit préserver l’information.

Exemple fictif du contenu pour A :

> MK4, groupe A  
> Semaine 46, du 9 au 13 novembre 2026  
> Lundi 9 novembre  
> De 8 h 30 à 10 h 30. UE 18, Neurologie.  
> Enseignante : Mme Martin. Salle 204.  
> Apporter la fiche de travaux pratiques.  
> Mardi 10 novembre. Aucun cours indiqué.  
> Mercredi 11 novembre. Armistice.

Les titres, dates et enseignants de cet exemple sont illustratifs. Un fichier réel doit garder les libellés du secrétariat. Le bouton « Copier le planning en texte » copie la projection explicitement indiquée du groupe, avec la même chronologie et les mêmes exclusions que le TXT. Confirmer la copie par un message accessible.

### 8.3 Tableau CSV

Proposer un profil générique et, si retenu dans le paquet, le profil Google Agenda déjà prévu dans le code. Pour le générique, une ligne par séance et des colonnes explicites pour date, début, fin, journée entière, intitulé, enseignant, lieu, UE, groupe et notes. La projection par groupe s’applique avant sérialisation. Prévoir la restitution des consignes hebdomadaires comme entrées identifiées. Paramétrer le séparateur et l’encodage par profil, documenter dans l’aide le comportement des dates et les colonnes propres à l’importateur. Pour le profil agenda, reporter dans la description les informations sans colonne propre.

Tester les accents, les noms comprenant des virgules, les guillemets, les sauts de ligne et une éventuelle première cellule interprétable comme formule dans un tableur. Protéger le profil tableur contre l’évaluation de formules provenant de textes non fiables ; tester le compromis de restitution dans les logiciels cibles. Ne transformer ni une date française en date américaine ambiguë ni une heure de fin absente en minuit.

### 8.4 PDF en gros caractères

Un PDF par groupe et par période. Le secrétariat l’active au besoin. Objectif : une page pour une semaine ordinaire si la densité le permet. Commencer la semaine suivante sur une nouvelle page. Si une semaine dépasse l’espace disponible, continuer sur une deuxième page, et davantage si nécessaire plutôt que tronquer des informations. Le nombre de pages n’est pas un critère supérieur à la lisibilité. Il n’est pas demandé de choisir entre plusieurs tailles de gros caractères.

Proposition à tester avec Bruno : papier A4 portrait, une colonne, fond blanc et texte sombre pour l’impression, police sans empattement incorporée, corps initial de 18 points et titres distincts. Ajuster cette valeur sur tirage ou écran réel au lieu de réduire automatiquement le texte selon la densité. Utiliser de vraies lignes de texte sélectionnables avec accents, ordre de lecture logique et langue française. Viser un PDF balisé avec titres de semaine et de jour ; valider les capacités de la bibliothèque choisie. Ne pas annoncer une conformité certifiée sans vérification.

Chaque semaine rappelle le groupe et la plage de dates. Maintenir ensemble le titre du jour et son premier cours ; maintenir de préférence un bloc cours sur la même page. Si une très longue consigne dépasse à elle seule une page, permettre sa continuation sans perte et rappeler le contexte. Sur une page de suite, répéter un repère court de semaine et de groupe. Tester une impression à taille réelle, la sélection du texte, le défilement vertical sur iPad et la lecture vocale. Le thème sombre de l’application ne détermine pas les couleurs du PDF destiné au papier.

<a id="chapitre-9"></a>

## 9. Parcours et écrans à développer

### 9.1 Cadre commun et accueil

Une seule fenêtre principale, trois repères « Importer », « Vérifier » et « Exporter ». À gauche, navigation « Nouvelle conversion », « Historique », puis « Paramètres » et « Aide ». La barre de titre offre les commandes Windows habituelles. Une fois un lot sélectionné, le contexte utile est la promotion, la période et le nombre de semaines ; le détail des fichiers est consultable. La sélection d’une nouvelle conversion pendant une édition offre la reprise du travail existant ou sa sauvegarde avant de commencer.

L’accueil vide montre le titre « Convertir un planning », le choix de promotion et le bouton « Choisir les fichiers Excel ». Le dépôt est un raccourci équivalent. Expliquer XLS/XLSX et le traitement local en une phrase. Lorsqu’une session interrompue existe, un encart discret identifie la dernière période et offre « Reprendre la vérification ». Au retour de la boîte d’ouverture, le focus revient au résumé de la sélection ou à l’action qui suit dans un ordre prévisible.

L’accueil rempli présente les fichiers retenus et les actions « Ajouter des fichiers », « Retirer » et « Analyser le planning ». Sur la maquette initiale on voit un seul fichier et un bouton « Changer de fichier » : étendre ce composant à un lot. Afficher le nombre retenu, les exclusions avec cause et les questions encore ouvertes. Une sélection vide ramène à l’état initial. Retirer un fichier en cours d’édition doit expliquer l’effet sur ses corrections.

### 9.2 Préparation du lot et analyse

Le choix des feuilles, le rattachement d’une promotion inconnue, le choix entre deux versions et la demande d’année sont des sous-étapes conditionnelles. Pour chacune, montrer le nom du fichier concerné, l’indice ayant motivé la question, une action pour poursuivre et une façon de revenir. Éviter de redemander l’année vingt fois pour la même feuille. Le cas simple doit passer directement de la sélection à la liste des cours.

Pendant l’analyse, afficher l’opération en cours et, pour un lot, le fichier concerné et sa position. N’annoncer un pourcentage que s’il reflète l’avancement réel. Autoriser « Annuler l’analyse », libérer les fichiers et conserver la sélection. Si un seul classeur échoue, préciser lequel et permettre de le remplacer ou poursuivre explicitement avec les autres semaines. Distinguer échec de lecture et semaine découverte comme manquante à la suite de ce retrait.

### 9.3 Vérification, source et correction

Le titre « Vérifier le planning » précède le résumé et la liste par jour. Des boutons « Voir les éléments à vérifier », « Ajouter un événement », « Annuler » et « Rétablir » restent disponibles selon le contexte. Les actions d’un cours sont utilisables sans survol. Pour la lecture vocale, chaque « Modifier » doit identifier la séance par son libellé accessible. Les informations du fichier d’origine apparaissent sur demande avec possibilité de copie.

Le formulaire d’édition ouvre sur le champ concerné lorsqu’il s’agit d’une anomalie précise. Champs : intitulé, jour, heures, journée entière, enseignant, lieu, UE, appartenance aux groupes, notes et compléments. Les libellés restent visibles après saisie. L’option « Journée entière » doit correspondre à une décision explicite, sans basculement automatique pour contourner une heure manquante. « Enregistrer » applique et revalide ; « Annuler » abandonne la saisie courante. La fermeture renvoie le focus à la séance, ou à sa voisine si elle vient d’être retirée du filtre.

Le traitement guidé affiche un problème à la fois, par exemple « Le cours commence à 14 h. Heure de fin manquante. » Proposer de compléter, revenir au planning ou continuer avec les séances exportables. Une alerte confirmable telle qu’un chevauchement peut être acceptée. Confirmer la lecture d’un message ne fabrique aucune valeur horaire. Le compteur « élément 2 sur 4 » doit être recalculé sans perdre la position de l’utilisateur après correction.

La maquette sombre de vérification représente un bouton d’export bloqué. Dans la version finale, le même emplacement porte une action disponible s’il reste des événements exportables. Exemple de retour : « 21 cours seront exportés. 1 cours à compléter sera exclu. » L’accès direct au cours en question reste disponible. Les erreurs de promotion et de destination incompatible sont traitées à leur étape propre.

### 9.4 Choix des formats et fin du parcours

Afficher le groupe et la période dans l’aperçu, puis tous les groupes destinataires sur l’écran « Enregistrer le planning ». Cases de formats accompagnées de leur usage. Placer les réglages propres au CSV ou aux rendus texte sous l’option correspondante. Donner les noms et le dossier prévu. « Retour au planning » conserve les corrections et les options. « Enregistrer les fichiers » crée une seule opération. Pendant l’écriture, empêcher un double déclenchement.

La vue de progression devient utile si l’écriture dure. Elle indique le fichier en cours et sait différencier les fichiers déjà publiés des fichiers en attente. Le résultat final propose « Ouvrir le dossier », « Revenir au planning » et « Convertir un autre planning ». « Ouvrir » sur un fichier appelle l’application système associée. Si elle manque, permettre d’accéder au fichier dans son dossier. Une ouverture impossible ne transforme pas une création réussie en échec d’export.

### 9.5 Historique, préférences, modèles et aide

L’historique affiche promotion, période, dernière modification, origine et résultat des exports. Le détail distingue les semaines de la session et les séries de fichiers produites. Actions : reprendre, ajouter une semaine, actualiser, remplacer la source d’une semaine, ouvrir le dossier et retirer de l’historique. Si un export a été déplacé, indiquer que l’emplacement n’est plus accessible. Conserver les autres informations disponibles.

Paramètres regroupés par usage : affichage, accessibilité, préférences de sortie et conservation locale. Une préférence ne doit pas retraiter silencieusement la période ouverte. Les règles validées telles que la production de tous les groupes et la préservation des corrections ne doivent pas devenir des choix imposés à chaque conversion. Les modèles réutilisent une disposition de feuille et des règles de lecture vérifiées. Ils ne mémorisent ni les noms des enseignants ni les dates d’une semaine comme valeurs permanentes. Ne présenter un gestionnaire utilisable qu’une fois les modèles véritablement branchés au moteur.

L’aide hors ligne explique une conversion simple, les extensions des fichiers, la différence entre « Retirer de la session » et modifier Excel, la reprise et les raccourcis. Inclure des instructions d’utilisation des exports sur les appareils effectivement testés. « À propos » indique version et accès au diagnostic local. Toute copie d’informations de dépannage affiche ce qui sera copié.

<a id="chapitre-10"></a>

## 10. Conception visuelle et accessibilité

Utiliser comme références l’accueil sombre avec fichier choisi et l’aperçu sombre avec anomalie. Fond anthracite, surfaces légèrement plus claires, titres clairs, accent bleu pour l’action principale, boutons bien arrondis, pictogrammes simples avec texte. Hiérarchiser sans dégradés décoratifs ni transparence derrière les libellés. Une alerte associe une explication à son repère visuel. Une réussite ou une exclusion reste compréhensible en monochrome. Le thème clair, le contraste système et les préférences d’agrandissement restent pris en compte.

Proposition de palette de départ : fond #171C26, panneau #222A38, texte #F4F6FA, secondaire #C3CDDC. Vérifier le contraste réel des composants dans chaque état, y compris survol, sélection et focus. Les valeurs sont une base de maquette. Privilégier une police familière de Windows, des titres repérables et un alignement à gauche. Sur une fenêtre étroite, replier le menu derrière un bouton nommé et empiler les actions. Ne pas contraindre l’utilisateur à maximiser la fenêtre pour accéder à « Enregistrer ».

Le parcours complet est utilisable au clavier. Tabulation entre régions et actions, flèches dans les listes, Entrée pour activer, Échap pour revenir d’un dialogue sans perdre une saisie par inadvertance. Prévoir Ctrl+O pour ouvrir, Ctrl+Z et Ctrl+Y pour annuler et rétablir dans le bon contexte. Conserver les conventions d’édition d’une zone de texte lorsqu’elle possède le focus. Le passage à une nouvelle étape amène le focus au titre ou au premier élément utile. L’actualisation d’une liste ne doit jamais le déplacer vers un bouton sans rapport.

Exposer aux lecteurs d’écran nom, rôle, valeur, état et appartenance à un groupe. Le contrôle d’une case de format doit annoncer le format et son état coché. L’ordre d’accessibilité d’un cours doit suivre date, heure, intitulé et compléments. Une région d’annonce signale fin d’analyse, validation, exclusion ou échec sans lire chaque variation de progression. Les messages importants persistent à l’écran et restent consultables. Les raccourcis n’excusent jamais l’absence de commande accessible.

Recette visuelle proposée à 100 %, 150 % et 200 % de mise à l’échelle Windows, fenêtre restaurée et maximisée, thèmes clair, sombre et contraste système. Contrôler particulièrement les noms de fichiers longs, les intitulés sur plusieurs lignes et les boutons du bas de formulaire. Recette vocale sous Windows avec Narrateur et NVDA ; validation distincte du document et du calendrier sur les appareils étudiants avec VoiceOver et, si disponible, TalkBack. L’accès clavier de l’application et l’accessibilité des fichiers diffusés ont des preuves de recette distinctes.

<a id="chapitre-11"></a>

## 11. Exemple complet de fonctionnement attendu

Scénario fictif de recette. La secrétaire choisit MK4 et ajoute les semaines 46 et 48. Une copie de la semaine 46 portant un autre nom contient une version différente. Elle choisit la bonne version. Elle poursuit malgré la semaine 47 absente. Un fichier MK3 ajouté par mégarde est identifié et écarté. Les deux semaines retenues apparaissent dans l’ordre des dates.

Lundi de la semaine 46, « Neurologie » à 8 h 30 est commun. À 10 h 30, deux TD distincts commencent pour A et B. L’après-midi, une séance A manque d’heure de fin. La secrétaire décide de la compléter plus tard. Le mardi ne contient aucune séance. Le mercredi porte un intitulé de jour férié. Vendredi de la semaine 48, il y a un cours commun dont la salle est inconnue. Une pause déjeuner est explicitement inscrite entre deux cours.

Pour ce jeu d’essai et en l’absence de toute autre entrée, A reçoit Neurologie, son TD, le jour férié et le cours commun du vendredi. B reçoit Neurologie, son autre TD, le jour férié et le cours du vendredi. Chaque ICS contient donc quatre événements. TXT et PDF mentionnent aussi le mardi vide. Aucun document ne crée de page pour la semaine 47 ; le cours sans salle est présent et la pause est omise. Les deux TD simultanés de groupes différents ne déclenchent pas de conflit. La secrétaire voit quel cours manque à l’export A.

Elle conserve ICS et TXT et coche le PDF. Six fichiers sont créés. Depuis le résultat, elle ouvre le dossier et joint les documents utiles à son courriel. Le lendemain, elle reprend sa période dans l’historique et précise l’heure de fin de la séance A. Un nouvel export produit six fichiers avec de nouveaux noms. Le calendrier A contient désormais cinq événements, le calendrier B toujours quatre. Si une nouvelle source déplace ce cours mais change aussi la fin qu’elle avait corrigée, la fin corrigée reste prioritaire pour le cours reconnu. Le déplacement doit être vérifié par rapprochement et non par simple numéro de ligne.

<a id="chapitre-12"></a>

## 12. État des sources et écarts à traiter

L’archive fournie contient une solution C# ciblant .NET 10 et une interface WPF pour Windows x64. Le compte rendu indique qu’un prototype Python a été exécuté sur trois classeurs réels. Les résultats documentés sont six événements en semaine 23, onze en semaine 19, dont le jour férié du 8 mai, et cinq en semaine 26. Ces nombres concernent le corpus et les règles de ce prototype. Ils servent de point de comparaison du lecteur, avant répartition par groupe et application des exclusions finales. Aucune compilation C# réussie, exécution des tests C# ou livraison d’exécutable n’a été établie lors de l’examen.

**Domain et Spreadsheet.** Les types ScheduleEvent, AnalysisResult et CellGrid existent. NpoiSpreadsheetReader et SpreadsheetFormatDetector traitent l’ouverture des feuilles. Vérifier le comportement réel en XLS comme en XLSX, la conversion des cellules de dates et la libération des fichiers. AnalysisResult décrit une feuille ; la période multi-source nécessite une agrégation supplémentaire.

**Parsing et Validation.** Le code comprend DaySectionParser, ColumnMapper, des lexiques français, des analyseurs de date et d’heure, UeExtractor et LocationSplitter. Valider les conventions provisoires de salle/groupe. DuplicateDetector, OverlapDetector et ValidationEngine fournissent une base à adapter à la portée réelle des groupes, aux cours communs et aux décisions humaines persistantes. Différencier confirmation d’une alerte, exclusion d’un seul cours et impossibilité de poursuivre une conversion entière.

**Application.** ConversionSession possède un seul chemin source et une seule empreinte. ReplaceEvent marque l’événement comme édité dans son ensemble. L’actualisation demandée impose une liste de sources et un suivi des modifications par champ. FromAutosaveJson reconstruit une session via un constructeur qui attribue un nouvel identifiant : garantir la continuité de l’identité lors de la reprise et préserver la position dans le parcours. Les instantanés d’annulation prévus pour quelques dizaines de séances devront être évalués sur plusieurs semaines.

**Export.** ExportUseCase refuse aujourd’hui toute l’opération si une anomalie bloquante subsiste. AnalysisResult.ReadyToExport exprime aussi une validation globale. Corriger les deux niveaux et les consommateurs de cette propriété. Construire la liste publiable par groupe avec motifs d’exclusion avant chaque rendu. Supprimer les replis sur la date du jour lorsque la période n’est pas identifiée : demander une date fiable. FileNaming comprend promotion et période, mais la cible exige groupe et date de génération. Le générateur d’UID actuel dépend entre autres du début et du titre ; un déplacement ou un renommage change donc la clé calculée. Prévoir une identité persistante pour les révisions.

**Storage.** SQLite enregistre des conversions et retrouve un fichier par empreinte. Le schéma historique existant est centré sur un fichier et les formats demandés. En cas de succès partiel, mémoriser les fichiers effectivement produits et les erreurs restantes. Les composants de sauvegarde, paramètres et modèles existent ; leur présence ne prouve ni une restauration complète ni l’application des modèles au lecteur.

**Interface et emballage.** L’accueil WPF et le rail de navigation sont présents. L’action recevant le fichier annonce sa réception mais ne déclenche pas l’analyse. Les vues restantes et la navigation doivent être reliées aux cas d’usage réels. Les paramètres du projet d’interface prévoient une publication autonome Windows x64 en fichier unique ; tester le type de sortie exécutable, les ressources WPF, les bibliothèques natives et l’exécution sur poste vierge. Le PDF est un nouvel exportateur à développer. Les exemples Excel d’origine contiennent des noms réels et ne doivent pas entrer dans un paquet diffusé.

<a id="chapitre-13"></a>

## 13. Architecture de réalisation proposée

Conserver la séparation Domain, Spreadsheet, Parsing, Validation, Export, Storage, Application et Ui. Le domaine porte les valeurs et règles déterministes. La couche de lecture traduit le classeur en cellules typées avec provenance. Le moteur traduit les cellules en événements et en observations. L’application coordonne les décisions et la persistance. L’interface présente l’état et transmet les actions explicites. Les exportateurs consomment un résultat final préparé : aucun exportateur ne doit interpréter lui-même « Groupe A » ou choisir si une anomalie est acceptable.

### 13.1 Objets persistants et identités

- **Promotion.** Identifiant, libellé visible, règles de groupe confirmées, modèles applicables et date de confirmation. Un nom servant à l’affichage peut changer sans casser l’historique.
- **Session de période.** Identifiant durable, promotion unique, semaines retenues, étape en cours, sélection et filtre, date de dernière sauvegarde et version du schéma. Les bornes sont calculées sur les semaines sélectionnées, même si le premier lundi est vide.
- **Source hebdomadaire.** Identifiant de semaine et année, période réelle, chemin, nom, empreinte, version, feuilles retenues, indices de promotion et rattachement confirmé. Conserver assez de données extraites pour relire la session hors de l’emplacement original.
- **Jour et entrée de semaine.** Stocker la date et son existence explicite dans le document même si elle ne contient aucune séance. Représenter séparément les consignes hebdomadaires et les journées nommées. Ce modèle évite de perdre un jour vide lors d’une simple liste d’événements.
- **Événement.** Identité interne, source, valeurs importées, type, date, heures, titre original, lieu, enseignants, UE, groupes, notes et autres champs. Distinguer valeur absente, déduite et confirmée. Un ensemble de groupes peut être vide uniquement avec une interprétation explicite du modèle, telle que « commun ».
- **Correction.** Identité du cours, nom de champ, valeur corrigée ou effacement explicite, date et révision. Calculer la vue effective en superposant seulement les champs corrigés à la source en cours. Pour une séance ajoutée manuellement, garder son origine manuelle.
- **Observation.** Code, portée (session, fichier, semaine ou événement), précision compréhensible, données ayant déclenché la règle, action attendue, conséquences et décision de la secrétaire. Une décision ne doit pas masquer une nouvelle alerte provoquée par d’autres valeurs.
- **Série d’export.** Identité de révision, date et heure, sélection de formats, groupe, période, emplacement, résultat de chaque fichier et nombre d’événements effectivement écrits. Permettre d’ouvrir le dossier d’une ancienne série indépendamment d’une nouvelle génération.

### 13.2 Ordre de traitement

1. Créer ou restaurer la session de promotion. Vérifier les droits de lecture, constituer la liste des fichiers et relever leurs empreintes.
2. Examiner classeur et feuilles, extraire indices de promotion et de période. Résoudre uniquement les ambiguïtés qui influencent la composition du lot.
3. Former le calendrier des semaines sélectionnées. Faire arbitrer les versions concurrentes et traiter les trous chronologiques.
4. Construire les cellules typées et les blocs de jours. Extraire les séances, jours vides, entrées de journée entière et notes ; conserver les références sources.
5. Rapprocher avec les séances mémorisées, superposer les corrections et traiter les disparitions. Appliquer les exclusions de pauses et les décisions humaines.
6. Recalculer validité, chevauchements et doublons. Présenter les observations dans l’interface, enregistrer chaque correction confirmée et actualiser les aperçus.
7. Pour chaque groupe, figer les jours, notes et séances publiables de toutes les semaines. Construire une demande par groupe et format avec une même révision de données.
8. Sauvegarder, écrire, contrôler et publier les fichiers. Enregistrer l’issue individuelle, afficher le résultat, permettre la reprise.

### 13.3 Robustesse, données locales et performance

Exécuter lecture, analyse et écriture hors du fil de l’interface. Propager le signal d’annulation jusqu’aux tâches qui prennent du temps. Interdire deux exports concurrents de la même demande par double clic. Si une opération est en cours, empêcher l’édition de son instantané, ou travailler sur une copie immuable. Centraliser le calcul des destinataires et le statut d’un cours ; tout aperçu et tout export doivent utiliser les mêmes règles.

Écrire paramètres et sauvegardes de manière atomique. Versionner les données persistées ; prévoir une migration testée pour les anciennes sessions à un seul fichier ou une conservation en lecture seule avec explication. Protéger l’état précédent en cas de migration interrompue. Une base corrompue doit pouvoir être isolée sans modifier les plannings sources ou les exports. Ne pas imposer une durée de conservation non décidée ; rendre la suppression locale explicite.

Limiter la taille, le nombre de cellules et les volumes décompressés acceptés suivant des seuils testés sur le poste cible. Informer l’utilisateur lorsqu’un seuil empêche la lecture. Mesurer séparément chargement, analyse, rendu et écriture pour une semaine, puis pour plusieurs semaines. La fenêtre doit continuer de répondre à la navigation ou à l’annulation. Proposer les seuils de temps de recette après mesure sur le poste de référence ; ne pas présenter une estimation comme performance acquise.

Aucune télémétrie obligatoire. Le journal local conserve l’étape, le type d’erreur et une référence de diagnostic, sans copier le contenu des cours ou les noms des enseignants par défaut. Les sessions conservées localement peuvent contenir les données nécessaires au travail. Les traiter dans le profil Windows de l’utilisateur et distinguer ces données du diagnostic partageable. La génération dans un dossier synchronisé relève du choix de destination par la secrétaire.

<a id="chapitre-14"></a>

## 14. Plan de développement et critères de passage

**Lot 0 — Reproduire l’existant.** Installer le SDK adapté sur l’environnement de construction, restaurer les dépendances déclarées, compiler, puis exécuter les tests du moteur et des exportateurs existants. Consigner les résultats et les bibliothèques effectivement utilisées. Vérifier particulièrement NPOI, Ical.Net et le moteur SQLite. Figer les versions après validation. Livrable : compilation répétable et rapport distinguant succès, échecs et essais non exécutés.

**Lot 1 — Obtenir une conversion verticale.** Brancher sélection, lecture, analyse, vue de vérification et un premier export sur un vrai classeur anonymisé. Montrer au clavier l’événement du lundi et le résultat sur disque. Ne pas construire d’abord toutes les fenêtres autour de résultats simulés. Ce lot démontre les interfaces entre composants, pas la conformité complète.

**Lot 2 — Promotion, période et groupes.** Introduire session multi-fichiers, arbitrage des semaines, organisation des autres promotions, jours explicites et projection par groupe. Tester les cours communs, groupes simultanés et semaines manquantes. Livrable : aperçus et décomptes par groupe comparés manuellement à deux semaines de référence.

**Lot 3 — Vérification et mémoire des corrections.** Implémenter la correction par champ, les exclusions temporaires, le choix sur doublon et chevauchement, l’historique de période et la reprise sur l’événement consulté. Tester remplacement de fichier, actualisation, association ambiguë et suppression d’une séance corrigée. Livrable : démonstration avant et après redémarrage.

**Lot 4 — Quatre sorties.** Faire produire ICS, TXT, CSV et PDF par les mêmes projections, contrôler noms, date d’export, collision et résultat partiel. Livrable : jeu de fichiers exportés anonymisés permettant la comparaison des formats. Faire relire les informations visuellement et vocalement avant de figer le gabarit PDF.

**Lot 5 — Poste Windows et destinataires.** Finaliser interface sombre et commandes arrondies, navigation, thèmes, agrandissement et aide. Tester sur Windows 10 et 11 dans les configurations réellement visées. L’accessibilité est testée dans chaque lot ; ce dernier passage couvre le parcours entier. Vérifier sur iPhone, Android et ordinateur les actions réellement décrites dans l’aide.

**Lot 6 — Livraison.** Produire un exécutable autonome ou un paquet installable validé sur le poste du secrétariat. L’installation simple par utilisateur est privilégiée ; confirmer les droits réels auprès de l’institut. Vérifier un démarrage hors ligne, sans SDK et sans Excel, avec un profil utilisateur neuf, puis un scénario de mise à niveau préservant paramètres et sessions. Tenir séparés sources, outils de développement, jeu de test et paquet utilisateur. Fournir version, instructions courtes de démarrage, notes de limites connues et procédure de sauvegarde/reprise.

<a id="chapitre-15"></a>

## 15. Recette fonctionnelle et technique

Pour chaque essai, consigner version, plateforme, source anonymisée, résultat attendu, résultat observé et preuve. Un essai non exécuté reste à réaliser. Les cas suivants sont des obligations de vérification et non un rapport de succès.

1. **Import unique.** Choisir MK4, charger un XLSX valide, puis réaliser le même parcours sur un fichier XLS représentatif. Les dates et caractères accentués restent corrects. Les empreintes des originaux restent inchangées après export.
2. **Lot inversé.** Sélectionner trois semaines dans le désordre. Les sorties de chaque groupe forment une période unique chronologique.
3. **Promotions.** Fournir MK4, MK3 et un fichier non identifié. MK3 est écarté, le fichier inconnu a besoin d’une confirmation et les sessions des promotions restent distinctes.
4. **Versions concurrentes.** Choisir deux versions d’une même semaine. Après arbitrage, vérifier l’absence des événements propres à la version écartée.
5. **Semaine manquante.** Retenir 46 et 48, poursuivre après alerte. Le texte et le PDF passent directement de 46 à 48 avec les bonnes dates.
6. **Groupes.** Utiliser un cours A, un B et un commun. Chaque groupe reçoit deux cours. Aperçu B puis export : les deux groupes sont effectivement produits.
7. **Import atypique.** Cellules fusionnées, absence d’en-tête horaire, saut de ligne entre heures, intitulé avec UE, cellule de salle mixte et journée vide. Comparer à une transcription manuelle indépendante.
8. **Donnée non essentielle absente.** Vérifier une séance sans enseignant, puis sans salle. Les deux sont diffusées avec leurs informations disponibles.
9. **Heure de fin absente.** La séance ordinaire reste visible dans la session. Les autres cours sortent dans les quatre formats. La corriger puis réexporter : elle apparaît dans le groupe concerné.
10. **Jour vide, férié, pause et consigne.** Quatre cas distincts ; le texte annonce la journée vide, conserve le nom du férié, omet la pause et place la consigne de semaine une seule fois. Contrôler les événements de journée entière dans l’ICS.
11. **Doublon exact.** Exécuter deux variantes, conserver un seul puis les deux. Vérifier le nombre et les identifiants obtenus dans chaque variante.
12. **Chevauchement.** Deux cours simultanés du même groupe déclenchent une vérification. Deux TD pour des groupes disjoints ne déclenchent pas cette alerte. Confirmer un chevauchement intentionnel puis reprendre la session.
13. **Correction persistante.** Corriger la fin dans l’application, modifier l’heure et la salle dans une nouvelle source, actualiser. La fin locale et la salle mise à jour sont présentes dans l’aperçu et dans les fichiers.
14. **Cours disparu.** Actualiser une semaine dont un cours corrigé a disparu. Tester séparément conservation et retrait ; les versions d’export précédentes restent intactes.
15. **Identité ambiguë.** Déplacer ou renommer deux lignes proches ; vérifier qu’une correction de salle ne se retrouve pas sur le mauvais cours. Tester insertion de ligne et modification d’horaire avec réimport calendrier.
16. **Reprise.** Fermer après correction du troisième cours d’une liste filtrée. Reprendre sous le même profil Windows. La période, le filtre et la séance sont retrouvés.
17. **Prolonger et remplacer.** Ajouter la semaine suivante depuis l’historique puis remplacer une ancienne semaine avec un classeur renommé. Vérifier la période finale et les corrections des semaines intactes.
18. **Export daté.** Générer deux fois le même jour et dans la même minute. Les séries ont des noms distincts et aucune version antérieure n’est perdue.
19. **Défaillance partielle.** Simuler l’impossibilité d’écrire un fichier. Vérifier la liste précise des réussites, l’absence de faux fichier définitif, puis la relance limitée aux échecs.
20. **TXT et CSV.** Relire accents, guillemets et retours à la ligne. Comparer chaque information de cours à la source corrigée. Importer le profil CSV dans l’outil pour lequel il est présenté.
21. **PDF.** Tester une semaine courte, une dense et un intitulé très long. Vérifier rendu à l’écran et à l’impression, absence de rognage, taille stable, texte sélectionnable et suite d’une semaine sur plusieurs pages.
22. **Décalage horaire.** Importer des séances en hiver et en été et une journée entière. Vérifier la date et l’heure dans Apple Calendrier, Google Agenda et Outlook.
23. **Réimport et suppression.** Importer une première série, déplacer un cours, en retirer un autre puis importer la nouvelle version. Relever le comportement de chaque agenda. Documenter la procédure utilisable par les étudiants si l’import simple ne remplace pas ou ne supprime pas les événements déjà présents.
24. **Parcours étudiant.** Sur iPhone avec VoiceOver et sur un téléphone Android depuis le moyen de partage réellement utilisé, retrouver groupe, intitulé, horaire et salle du prochain cours sans aide humaine. Relever les gestes nécessaires.
25. **Parcours secrétariat.** Au clavier, ouvrir deux semaines, examiner une alerte, modifier la séance, vérifier un groupe, exporter et ouvrir le dossier. Refaire avec lecteur d’écran et fort agrandissement. Contrôler l’ordre des annonces et la récupération du focus.
26. **Poste vierge et défaillances locales.** Démarrer sans Internet et sans Excel, importer un classeur protégé, puis un fichier corrompu, annuler un traitement et essayer un dossier non inscriptible. Vérifier la reprise et l’intégrité des sources.
27. **Autre promotion.** Faire valider par le secrétariat au moins un planning hors MK4 ; documenter ses groupes, ses conventions et son résultat attendu. Ne généraliser la couverture que sur les cas testés.
28. **Confidentialité de livraison.** Inspecter le paquet livré et les diagnostics partageables. Vérifier l’absence des vrais classeurs et des données de travail de l’auteur.

<a id="chapitre-16"></a>

## 16. Conditions de remise et questions de terrain

La première livraison utilisable réunit trois preuves : une secrétaire réalise seule l’intégralité du parcours depuis ses documents habituels, les fichiers correspondent aux séances effectivement destinées à chaque groupe et un étudiant retrouve avec son appareil les informations dont il a besoin. Une simple compilation ou une suite de captures d’écran ne suffit pas à démontrer ces trois résultats.

À observer en priorité : configuration et droits du poste Windows ; autre promotion et structure réelle de ses groupes ; éventuelle signification de couleurs ou barrés ; solution de partage et parcours Android ; comportement des imports répétés ; taille de texte PDF jugée confortable sur une vraie semaine dense. Les autres décisions décrites dans ce document peuvent être engagées sans attendre ces observations. Tout écart découvert doit être consigné avec un exemple anonymisé, la règle affectée et le test qui permettra de vérifier la correction.

## Annexe A. Repères sur les sources du projet

Le cahier de départ comprend 148 sections couvrant notamment lecture Excel, ergonomie, architecture, déploiement et stratégie de test. La présente édition intègre ses exigences compatibles et remplace ses anciennes hypothèses sur le fichier unique, la validation globale, l’interface, les sorties obligatoires et les versions. Le compte rendu « Conversation_Planning_Converter.md », le README, les notes de développement et les fichiers de la solution sont fournis dans l’archive « Explication_du_projet_et_source.zip ». Les indications anciennes sur des couches « non commencées » sont à lire à la lumière de leur ajout documenté plus loin dans les notes et des sources réellement présentes.

Les maquettes « Convertisseur de planning en mode sombre » et « Vérification du planning avant export » documentent les préférences visuelles. La première maquette claire est une étape exploratoire antérieure au choix du fond sombre. Le cahier constitue le document de référence pour le comportement ; une maquette ultérieure doit illustrer la sélection multi-fichiers et la poursuite possible de l’export en présence d’un cours exclu.

## Annexe B. Vocabulaire à conserver dans l’application

**Promotion** : ensemble d’étudiants traité dans une conversion, par exemple MK4. **Groupe** : sous-ensemble destinataire d’un planning. **Période** : ensemble de semaines sélectionnées pour une promotion. **Semaine présente** : semaine fournie et acceptée. **Jour vide** : journée explicitement présente dont la source ne donne aucun cours. **Cours commun** : séance qui appartient à tous les groupes concernés selon une règle confirmée. **Correction** : choix enregistré par champ et prioritaire pour une séance retrouvée. **À compléter** : séance conservée pour travail ultérieur, temporairement exclue si les données indispensables manquent. **Aperçu** : lecture du résultat pour un groupe. **Série d’export** : fichiers datés issus d’une même version de la période.

## Annexe C. Contrôle de cohérence avant validation d’une modification du logiciel

Pour toute évolution du lecteur, vérifier un exemple qui fonctionne déjà, un exemple qui motive l’évolution et un exemple qui doit rester ambigu. Pour toute évolution des groupes, comparer le nombre de séances source au contenu individuel de chaque planning. Pour toute évolution de la présentation, effectuer le même geste au clavier et vérifier ce qu’annonce la synthèse vocale. Pour toute évolution du stockage, rouvrir une session créée par la version précédente. Pour toute évolution des exportateurs, comparer le résultat aux valeurs effectives après correction, puis ouvrir réellement les fichiers sur les appareils cibles. Conserver ces contrôles dans la recette de livraison.
