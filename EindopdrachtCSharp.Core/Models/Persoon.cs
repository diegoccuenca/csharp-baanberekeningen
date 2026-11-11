namespace EindopdrachtCSharp.Models;

/// <summary>Model van één persoon uit de SQLite-tabel Personen.</summary>
/// <remarks>Deze klasse bevat uitsluitend gegevens en is onafhankelijk van Avalonia.</remarks>
public class Persoon
{
    /// <summary>Unieke sleutel uit de SQLite-database.</summary>
    public int Id { get; set; }

    /// <summary>Naam van de persoon.</summary>
    public string Naam { get; set; } = "";

    /// <summary>Leeftijd in jaren.</summary>
    public int Leeftijd { get; set; }

    /// <summary>Maakt een voor gebruikers leesbare beschrijving van de persoon.</summary>
    /// <returns>Persoons-ID, naam en leeftijd.</returns>
    public override string ToString() => $"{Id} - {Naam} ({Leeftijd} jaar)";
}
