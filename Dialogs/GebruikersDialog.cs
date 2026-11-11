using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Data.Sqlite;

namespace EindopdrachtCSharp.Dialogs;

/// <summary>
/// Toont gebruiksvriendelijke modale foutmeldingen en bevestigingsvragen.
/// Technische foutdetails zijn op verzoek zichtbaar voor de corrector.
/// Er is geen externe messagebox-package nodig.
/// </summary>
public static class GebruikersDialog
{
    /// <summary>Maakt een leesbare fouttekst zonder technische bestandspaden aan gebruikers te tonen.</summary>
    public static string LeesbareFout(Exception fout) => fout switch
    {
        ArgumentException => fout.Message,
        FileNotFoundException => "Een noodzakelijk bestand ontbreekt. Controleer de programmabestanden en probeer opnieuw.",
        DirectoryNotFoundException => "Een vereiste map is niet gevonden. Controleer de programmabestanden.",
        UnauthorizedAccessException => "Het programma heeft geen toegang tot een benodigd bestand of map. Controleer de rechten.",
        SqliteException => "De databasebewerking is mislukt. Controleer de database en probeer het opnieuw.",
        IOException => "Er is een probleem bij het lezen of opslaan van bestanden. Controleer of de bestanden beschikbaar zijn.",
        InvalidOperationException => fout.Message,
        _ => "Er is een onverwachte fout opgetreden. Probeer het opnieuw of bekijk de technische details."
    };

    /// <summary>Opent een foutvenster zonder de GUI-thread te blokkeren.</summary>
    public static void ToonFout(Window eigenaar, string actie, Exception fout)
    {
        var venster = MaakVenster($"Fout bij {actie}", 570);
        var inhoud = new StackPanel { Margin = new Thickness(20), Spacing = 12 };
        inhoud.Children.Add(new TextBlock
        {
            Text = $"Fout bij {actie}", FontSize = 20, FontWeight = FontWeight.SemiBold
        });
        inhoud.Children.Add(new TextBlock
        {
            Text = LeesbareFout(fout), TextWrapping = TextWrapping.Wrap
        });

        var details = new TextBox
        {
            Text = fout.ToString(), IsReadOnly = true, AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap, Height = 195, IsVisible = false
        };
        var detailsKnop = new Button { Content = "Technische details tonen" };
        detailsKnop.Click += (_, _) =>
        {
            details.IsVisible = !details.IsVisible;
            detailsKnop.Content = details.IsVisible
                ? "Technische details verbergen" : "Technische details tonen";
        };

        var sluiten = new Button { Content = "Sluiten", MinWidth = 100 };
        sluiten.Click += (_, _) => venster.Close();
        var knoppen = new StackPanel
        {
            Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };
        knoppen.Children.Add(detailsKnop);
        knoppen.Children.Add(sluiten);
        inhoud.Children.Add(details);
        inhoud.Children.Add(knoppen);
        venster.Content = inhoud;
        _ = venster.ShowDialog(eigenaar);
    }

    /// <summary>Vraag eerst bevestiging voordat de database definitief opnieuw wordt opgebouwd.</summary>
    public static async Task<bool> BevestigResetAsync(Window eigenaar)
    {
        var venster = MaakVenster("Database resetten?", 490);
        var inhoud = new StackPanel { Margin = new Thickness(20), Spacing = 16 };
        inhoud.Children.Add(new TextBlock
        {
            Text = "Database volledig opnieuw opbouwen?",
            FontSize = 19, FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });
        inhoud.Children.Add(new TextBlock
        {
            Text = "Alle toegevoegde en gewijzigde personen worden verwijderd. " +
                   "De oorspronkelijke 10 personen, 10 opleidingen en 10 beroepen worden teruggezet.",
            TextWrapping = TextWrapping.Wrap
        });
        var annuleren = new Button { Content = "Annuleren", MinWidth = 100 };
        var resetten = new Button { Content = "Ja, reset database", MinWidth = 145 };
        annuleren.Click += (_, _) => venster.Close(false);
        resetten.Click += (_, _) => venster.Close(true);
        var knoppen = new StackPanel
        {
            Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8
        };
        knoppen.Children.Add(annuleren);
        knoppen.Children.Add(resetten);
        inhoud.Children.Add(knoppen);
        venster.Content = inhoud;
        return await venster.ShowDialog<bool>(eigenaar);
    }

    private static Window MaakVenster(string titel, double breedte) => new()
    {
        Title = titel, Width = breedte, MinHeight = 170, MaxHeight = 540,
        SizeToContent = SizeToContent.Height, WindowStartupLocation = WindowStartupLocation.CenterOwner,
        CanResize = false, ShowInTaskbar = false
    };
}
