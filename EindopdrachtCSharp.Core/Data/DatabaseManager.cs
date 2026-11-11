using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using EindopdrachtCSharp.Models;

namespace EindopdrachtCSharp.Data;

/// <summary>
/// Beheert de SQLite-database en voert alle databasebewerkingen uit.
/// De database wordt opgeslagen in de gebruikersmap zodat ook een
/// zelfstandig gepubliceerd Windows-programma schrijfrechten heeft.
/// </summary>
public class DatabaseManager
{
    private readonly string connectionString;

    /// <summary>
    /// Maakt een databasebeheerder. De SQLite-database staat in de persoonlijke
    /// applicatiemap, zodat de applicatie ook zonder beheerdersrechten kan schrijven.
    /// </summary>
    public DatabaseManager()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EindopdrachtCSharp");

        Directory.CreateDirectory(folder);
        string databasePath = Path.Combine(folder, "eindopdracht.db");
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath
        }.ToString();
    }

    /// <summary>Maakt een nieuwe, nog niet geopende SQLite-verbinding.</summary>
    /// <returns>Een verbinding waarvan de aanroeper zelf de levensduur beheert.</returns>
    public SqliteConnection GetConnection() => new(connectionString);

    /// <summary>
    /// Maakt bij de allereerste start de database en voorbeeldgegevens aan.
    /// Bij volgende starts blijven ook verwijderde en gewijzigde personen
    /// daadwerkelijk verwijderd of gewijzigd. Alleen ResetDatabase() vult
    /// de voorbeeldgegevens opnieuw.
    /// </summary>
    public void InitializeDatabase()
    {
        using var connection = GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        // Controle VOOR het uitvoeren van schema.sql: welke tabellen bestaan al?
        // Dit is essentieel: opnieuw opstarten mag geen verwijderde personen herstellen.
        using var check = connection.CreateCommand();
        check.Transaction = transaction;
        check.CommandText = """
            SELECT COUNT(*) FROM sqlite_master
            WHERE type = 'table'
              AND name IN ('Personen', 'Opleidingen', 'Beroepen');
            """;
        int existingTables = Convert.ToInt32(check.ExecuteScalar());

        ExecuteEmbeddedSql(connection, transaction, "schema.sql");

        // Alleen een volledig nieuwe database krijgt de initiële 10 records
        // per tabel. Wanneer er al tabellen zijn, behouden we de gebruikersdata.
        if (existingTables == 0)
        {
            ExecuteEmbeddedSql(connection, transaction, "seed.sql");
        }

        transaction.Commit();
    }

    /// <summary>
    /// Leest personen als objecten met een betrouwbaar database-ID.
    /// Zo hoeven we de ID niet uit een weergavetekst te halen.
    /// </summary>
    /// <returns>Nieuwe lijst van persoonsobjecten, gesorteerd op ID.</returns>
    public List<Persoon> GetPersonObjects()
    {
        var personen = new List<Persoon>();
        using var connection = GetConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Naam, Leeftijd FROM Personen ORDER BY Id";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            personen.Add(new Persoon
            {
                Id = reader.GetInt32(0),
                Naam = reader.GetString(1),
                Leeftijd = reader.GetInt32(2)
            });
        }
        return personen;
    }

    /// <summary>Leest alle personen als opgemaakte tekstregels.</summary>
    /// <returns>Lijst met ID, naam en leeftijd voor het personenoverzicht.</returns>
    public List<string> GetPersonen()
    {
        var personen = new List<string>();
        using var connection = GetConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Naam, Leeftijd FROM Personen ORDER BY Id";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            personen.Add($"{reader.GetInt32(0)} - {reader.GetString(1)} ({reader.GetInt32(2)} jaar)");
        }
        return personen;
    }

    /// <summary>Leest de namen van alle opleidingen uit SQLite.</summary>
    /// <returns>Opleidingen gerangschikt op database-ID.</returns>
    public List<string> GetOpleidingen() => GetNames("Opleidingen");

    /// <summary>Leest de namen van alle beroepen uit SQLite.</summary>
    /// <returns>Beroepen gerangschikt op database-ID.</returns>
    public List<string> GetBeroepen() => GetNames("Beroepen");

    private List<string> GetNames(string tableName)
    {
        var names = new List<string>();
        using var connection = GetConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        // Wordt alleen aangeroepen met de twee vaste tabelnamen hierboven.
        command.CommandText = $"SELECT Naam FROM {tableName} ORDER BY Id";
        using var reader = command.ExecuteReader();
        while (reader.Read()) names.Add(reader.GetString(0));
        return names;
    }

    /// <summary>Voegt een persoon toe met een geparametriseerde SQL-query.</summary>
    /// <param name="naam">Naam van de nieuwe persoon.</param>
    /// <param name="leeftijd">Leeftijd (0–120).</param>
    public void AddPersoon(string naam, int leeftijd)
    {
        using var connection = GetConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Personen (Naam, Leeftijd) VALUES (@naam, @leeftijd)";
        command.Parameters.AddWithValue("@naam", naam);
        command.Parameters.AddWithValue("@leeftijd", leeftijd);
        command.ExecuteNonQuery();
    }

    /// <summary>Verwijdert precies één persoon op basis van het database-ID.</summary>
    /// <param name="id">Database-ID van de te verwijderen persoon.</param>
    public void DeletePersoon(int id)
    {
        using var connection = GetConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Personen WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }

    /// <summary>Past de naam en leeftijd van een bestaande persoon aan.</summary>
    /// <param name="id">Database-ID van de te wijzigen persoon.</param>
    /// <param name="naam">Nieuwe naam.</param>
    /// <param name="leeftijd">Nieuwe leeftijd (0–120).</param>
    public void UpdatePersoon(int id, string naam, int leeftijd)
    {
        using var connection = GetConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Personen SET Naam = @naam, Leeftijd = @leeftijd WHERE Id = @id";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@naam", naam);
        command.Parameters.AddWithValue("@leeftijd", leeftijd);
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Bouwt de database opnieuw op en vult alle tabellen opnieuw.
    /// Deze actie verwijdert bewust alle door de gebruiker toegevoegde personen.
    /// </summary>
    public void ResetDatabase()
    {
        using var connection = GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = "DROP TABLE IF EXISTS Personen; DROP TABLE IF EXISTS Opleidingen; DROP TABLE IF EXISTS Beroepen;";
            command.ExecuteNonQuery();
        }
        ExecuteEmbeddedSql(connection, transaction, "schema.sql");
        ExecuteEmbeddedSql(connection, transaction, "seed.sql");
        transaction.Commit();
    }

    private static void ExecuteEmbeddedSql(
        SqliteConnection connection, SqliteTransaction transaction, string fileName)
    {
        var assembly = typeof(DatabaseManager).Assembly;
        string resourceName = $"EindopdrachtCSharp.Sql.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Ingebouwd SQL-script ontbreekt: {fileName}");
        using var reader = new StreamReader(stream);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = reader.ReadToEnd();
        command.ExecuteNonQuery();
    }
}
