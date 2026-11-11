# C# Eindopdracht – Baanberekeningen

Grafische desktopapplicatie gemaakt met **C#**, **Avalonia UI** en **SQLite**.

## Functies

- Personen toevoegen, bekijken, wijzigen en verwijderen (CRUD)
- SQLite-database met tabellen **Personen**, **Opleidingen** en **Beroepen**
- Kansberekeningen met willekeurige kansen voor opleiding-beroepcombinaties
- Gelijktijdige asynchrone berekeningen met voortgang per persoon en totaal
- Logboek met automatisch scrollen en gebruiksvriendelijke foutmeldingen
- Resetknop om de voorbeeldgegevens opnieuw op te bouwen
- Aparte herbruikbare Core-library met XML-documentatie

## Starten vanuit de broncode

**Vereiste:** .NET 10 SDK.

Open een terminal in de hoofdmap en voer uit:

```bash
dotnet run
```

De applicatie maakt de lokale SQLite-database bij de eerste start automatisch aan.

## Projectstructuur

- `MainWindow.axaml` / `MainWindow.axaml.cs`: grafische interface en gebruikersacties
- `Dialogs/`: foutmeldingen en bevestigingen
- `EindopdrachtCSharp.Core/`: database, modellen, kansberekeningen en SQL-scripts

## Windows-versie voor inlevering

Voor de schoolinlevering is ook een self-contained Windows-publicatie beschikbaar. Die is bewust **niet** opgenomen in deze broncoderepository, zodat GitHub overzichtelijk blijft.
