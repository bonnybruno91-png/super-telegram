namespace PlanningConverter.Domain;

/// <summary>§38 : format d'affichage du titre d'un événement exporté.</summary>
public enum TitleFormat { Simple, CourseDashTeacher, UeDashCourse }

/// <summary>§40 : profil de colonnes/séparateur/encodage pour l'export CSV.</summary>
public enum CsvProfile { Generic, GoogleCalendar }

/// <summary>§41 : mise en forme du texte accessible.</summary>
public enum TextFormat { Detailed, Compact }
