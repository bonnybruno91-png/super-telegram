namespace PlanningConverter.Export;

/// <summary>
/// Bloc VTIMEZONE Europe/Paris, conforme RFC 5545, saisi et vérifié à la main.
///
/// Pourquoi ne pas laisser Ical.Net le générer ? <c>Calendar.AddTimeZone(new VTimeZone(tzid))</c>
/// ne produit qu'un VTIMEZONE réduit à TZID + X-LIC-LOCATION, SANS les sous-composants
/// STANDARD/DAYLIGHT — pourtant obligatoires selon la RFC 5545 §3.6.5 ("MUST include ... at
/// least one definition of a STANDARD or DAYLIGHT sub-component"). C'est un manque connu et
/// documenté du projet (issues #58 et #241 sur ical-org/ical.net, la seconde encore ouverte en
/// 2025). Un tel VTIMEZONE tronqué est précisément la cause typique des décalages d'une heure
/// observés dans Outlook et Google Agenda à l'import (§37, §113).
///
/// Règle de changement d'heure européenne : dernier dimanche de mars (01h00→02h00 UTC,
/// 02h00→03h00 heure locale) et dernier dimanche d'octobre (01h00→02h00 UTC, 03h00→02h00 heure
/// locale). Règle fixe depuis 1996, appliquée sans date de fin connue à ce jour ; si l'Union
/// européenne mettait fin aux changements d'heure, cette constante serait le seul endroit à
/// mettre à jour.
///
/// Inséré tel quel par <see cref="IcsExporter"/> après les propriétés du calendrier.
/// </summary>
internal static class ParisTimeZoneBlock
{
    public const string Text =
        "BEGIN:VTIMEZONE\r\n" +
        "TZID:Europe/Paris\r\n" +
        "X-LIC-LOCATION:Europe/Paris\r\n" +
        "BEGIN:DAYLIGHT\r\n" +
        "TZOFFSETFROM:+0100\r\n" +
        "TZOFFSETTO:+0200\r\n" +
        "TZNAME:CEST\r\n" +
        "DTSTART:19700329T020000\r\n" +
        "RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=-1SU\r\n" +
        "END:DAYLIGHT\r\n" +
        "BEGIN:STANDARD\r\n" +
        "TZOFFSETFROM:+0200\r\n" +
        "TZOFFSETTO:+0100\r\n" +
        "TZNAME:CET\r\n" +
        "DTSTART:19701025T030000\r\n" +
        "RRULE:FREQ=YEARLY;BYMONTH=10;BYDAY=-1SU\r\n" +
        "END:STANDARD\r\n" +
        "END:VTIMEZONE\r\n";
}
