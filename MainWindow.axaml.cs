using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using EindopdrachtCSharp.Data;
using EindopdrachtCSharp.Dialogs;
using EindopdrachtCSharp.Models;
using EindopdrachtCSharp.Services;

namespace EindopdrachtCSharp;

public partial class MainWindow : Window
{
    private DatabaseManager db = null!;
    private bool databaseGereed;
    private readonly Dictionary<int, PersoonBerekeningUi> persoonRijen = new();
    private int actieveBerekeningen;

    /// <summary>UI-status van één persoon. Alleen wijzigen op de UI-thread.</summary>
    private sealed class PersoonBerekeningUi
    {
        public required Button StartKnop { get; init; }
        public required ProgressBar Voortgangsbalk { get; init; }
        public required TextBlock VoortgangTekst { get; init; }
        public required TextBlock ResultaatTekst { get; init; }
        public bool Gestart { get; set; }
        public int Afgerond { get; set; }
        public int Totaal { get; set; }
    }

    public MainWindow()
    {
        InitializeComponent();
        try
        {
            db = new DatabaseManager();
            db.InitializeDatabase();
            databaseGereed = true;
            ToonPersonen();
            StatusText.Text = "Database klaar voor gebruik.";
        }
        catch (Exception ex)
        {
            databaseGereed = false;
            ZetBewerkKnoppen(false);
            InitButton.IsEnabled = true; // Probeer initialiseren opnieuw na herstel van het probleem.
            BerekenButton.IsEnabled = false;
            MeldingFout("opstarten / database initialiseren", ex);
        }
    }

    private void InitializeDatabaseClick(object? sender, RoutedEventArgs e)
    {
        if (actieveBerekeningen > 0) return;
        try
        {
            db ??= new DatabaseManager();
            db.InitializeDatabase();
            databaseGereed = true;
            ToonPersonen();
            ZetBewerkKnoppen(true);
            BerekenButton.IsEnabled = true;
            StatusText.Text = "Database klaar. Bestaande gegevens zijn behouden.";
        }
        catch (Exception ex) { MeldingFout("Initialiseren", ex); }
    }

    private async void ResetDatabaseClick(object? sender, RoutedEventArgs e)
    {
        if (actieveBerekeningen > 0) return;
        try
        {
            if (!await GebruikersDialog.BevestigResetAsync(this))
            {
                StatusText.Text = "Reset geannuleerd. Alle gegevens zijn behouden.";
                return;
            }
            if (actieveBerekeningen > 0) return;
            db.ResetDatabase();
            LogListBox.Items.Clear();
            KansText.Text = "Nog geen kans berekend";
            BesteBaanText.Text = "Beste baan: nog niet berekend";
            ToonPersonen();
            Log("Database opnieuw opgebouwd met 10 personen, 10 opleidingen en 10 beroepen.");
            StatusText.Text = "Database is gereset!";
        }
        catch (Exception ex) { MeldingFout("Resetten", ex); }
    }

    private void LoadPersonenClick(object? sender, RoutedEventArgs e)
    {
        if (actieveBerekeningen > 0) return;
        try
        {
            ToonPersonen();
            StatusText.Text = "Personen geladen!";
        }
        catch (Exception ex) { MeldingFout("Personen laden", ex); }
    }

    private void ToonPersonen()
    {
        // Alleen aanroepen wanneer er geen actieve berekeningen zijn.
        PersonenListBox.Items.Clear();
        PersonStatusPanel.Children.Clear();
        persoonRijen.Clear();

        foreach (var persoon in db.GetPersonObjects())
        {
            PersonenListBox.Items.Add(persoon);

            var label = new TextBlock { Text = persoon.ToString(), Width = 220 };
            var knop = new Button { Content = "Bereken kans", Width = 112 };
            var balk = new ProgressBar
            {
                Minimum = 0, Maximum = 100, Value = 0, Width = 195, Height = 20
            };
            var voortgang = new TextBlock { Text = "0%", Width = 50 };
            var resultaat = new TextBlock
            {
                Text = "Nog niet berekend", TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            var rij = new StackPanel { Orientation = Orientation.Vertical, Spacing = 3 };
            var regel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            regel.Children.Add(label);
            regel.Children.Add(knop);
            regel.Children.Add(balk);
            regel.Children.Add(voortgang);
            rij.Children.Add(regel);
            rij.Children.Add(resultaat);
            PersonStatusPanel.Children.Add(rij);

            persoonRijen[persoon.Id] = new PersoonBerekeningUi
            {
                StartKnop = knop,
                Voortgangsbalk = balk,
                VoortgangTekst = voortgang,
                ResultaatTekst = resultaat
            };

            // Elke persoon krijgt zijn eigen knop en eigen asynchrone taak.
            knop.Click += async (_, _) => await StartBerekeningAsync(persoon);
        }

        UpdateHoofdVoortgang();
    }

    private void AddPersoonClick(object? sender, RoutedEventArgs e)
    {
        if (actieveBerekeningen > 0) return;
        try
        {
            var (naam, leeftijd) = LeesInvoer();
            db.AddPersoon(naam, leeftijd);
            ToonPersonen();
            StatusText.Text = "Persoon toegevoegd!";
        }
        catch (Exception ex) { MeldingFout("Toevoegen", ex); }
    }

    private void DeletePersoonClick(object? sender, RoutedEventArgs e)
    {
        if (actieveBerekeningen > 0) return;
        try
        {
            if (PersonenListBox.SelectedItem is not Persoon persoon)
            {
                StatusText.Text = "Selecteer eerst een persoon.";
                return;
            }
            db.DeletePersoon(persoon.Id);
            ToonPersonen();
            StatusText.Text = "Persoon verwijderd!";
        }
        catch (Exception ex) { MeldingFout("Verwijderen", ex); }
    }

    private void UpdatePersoonClick(object? sender, RoutedEventArgs e)
    {
        if (actieveBerekeningen > 0) return;
        try
        {
            if (PersonenListBox.SelectedItem is not Persoon persoon)
            {
                StatusText.Text = "Selecteer eerst een persoon.";
                return;
            }
            var (naam, leeftijd) = LeesInvoer();
            db.UpdatePersoon(persoon.Id, naam, leeftijd);
            ToonPersonen();
            StatusText.Text = "Persoon gewijzigd!";
        }
        catch (Exception ex) { MeldingFout("Wijzigen", ex); }
    }

    private (string Naam, int Leeftijd) LeesInvoer()
    {
        string naam = (NaamTextBox.Text ?? "").Trim();
        if (naam.Length == 0)
            throw new ArgumentException("Voer een naam in.");
        if (!int.TryParse(LeeftijdTextBox.Text, out int leeftijd) || leeftijd < 0 || leeftijd > 120)
            throw new ArgumentException("Voer een geldige leeftijd tussen 0 en 120 in.");
        return (naam, leeftijd);
    }

    private async void BerekenKansClick(object? sender, RoutedEventArgs e)
    {
        if (PersonenListBox.SelectedItem is not Persoon persoon)
        {
            StatusText.Text = "Selecteer eerst een persoon.";
            return;
        }
        await StartBerekeningAsync(persoon);
    }

    private async Task StartBerekeningAsync(Persoon persoon)
    {
        if (!persoonRijen.TryGetValue(persoon.Id, out var rij) || rij.Gestart)
            return;

        // Namen één keer vooraf inlezen. Iedere worker werkt vervolgens alleen
        // met een eigen snapshot en raakt de database niet aan.
        List<string> opleidingen;
        List<string> beroepen;
        try
        {
            opleidingen = db.GetOpleidingen();
            beroepen = db.GetBeroepen();
            if (opleidingen.Count == 0 || beroepen.Count == 0)
                throw new InvalidOperationException("Er zijn geen opleidingen of beroepen in de database.");
        }
        catch (Exception ex)
        {
            MeldingFout("Berekening voorbereiden", ex);
            return;
        }

        rij.Gestart = true;
        rij.StartKnop.IsEnabled = false; // Na een klik kan de persoon niet dubbel starten.
        rij.Afgerond = 0;
        rij.Totaal = checked(opleidingen.Count * beroepen.Count);
        rij.Voortgangsbalk.Value = 0;
        rij.VoortgangTekst.Text = "0%";
        rij.ResultaatTekst.Text = "Berekening loopt...";
        actieveBerekeningen++;
        ZetBewerkKnoppen(actieveBerekeningen == 0);
        UpdateHoofdVoortgang();
        Log($"Gestart: {persoon.Naam} ({rij.Totaal} combinaties, wachttijd 100–1000 ms per combinatie).");

        // Progress<T> op de UI-thread aanmaken: terugmeldingen worden via de
        // Avalonia SynchronizationContext weer op de UI-thread uitgevoerd.
        var progress = new Progress<KansVoortgang>(update =>
        {
            rij.Afgerond = update.Afgerond;
            double percentage = 100.0 * update.Afgerond / update.Totaal;
            rij.Voortgangsbalk.Value = percentage;
            rij.VoortgangTekst.Text = $"{percentage:0}%";
            UpdateHoofdVoortgang();
            Log($"{persoon.Naam}: {update.Opleiding} × {update.Beroep} | " +
                $"{update.OpleidingsKans:0.00} × {update.BeroepsKans:0.00} = {update.EindKans:P1}");
        });

        try
        {
            // De taak wordt op de threadpool gestart, nooit op de GUI-thread.
            // Task.Delay zorgt dat de overige knoppen responsief blijven.
            KansResultaat resultaat = await Task.Run(
                () => KansCalculator.BerekenAsync(opleidingen, beroepen, progress));

            rij.Afgerond = rij.Totaal;
            rij.Voortgangsbalk.Value = 100;
            rij.VoortgangTekst.Text = "100%";
            rij.ResultaatTekst.Text = $"Beste baan: {resultaat.BesteBeroep} " +
                $"({resultaat.HoogsteKans:P1}) via {resultaat.BesteOpleiding}";
            BesteBaanText.Text = $"Beste baan voor {persoon.Naam}: {resultaat.BesteBeroep}";
            KansText.Text = $"Hoogste kans: {resultaat.HoogsteKans:P1} " +
                $"(opleiding: {resultaat.BesteOpleiding})";
            Log($"KLAAR: {persoon.Naam} → {resultaat.BesteBeroep}, {resultaat.HoogsteKans:P1}.");
            StatusText.Text = $"Berekening van {persoon.Naam} afgerond.";
        }
        catch (Exception ex)
        {
            // Bij fouten kan de gebruiker de berekening opnieuw starten.
            rij.Gestart = false;
            rij.StartKnop.IsEnabled = true;
            rij.ResultaatTekst.Text = "Berekening mislukt; probeer opnieuw.";
            rij.Totaal = 0;
            rij.Afgerond = 0;
            rij.Voortgangsbalk.Value = 0;
            rij.VoortgangTekst.Text = "0%";
            MeldingFout($"Berekening voor {persoon.Naam}", ex);
        }
        finally
        {
            actieveBerekeningen--;
            ZetBewerkKnoppen(actieveBerekeningen == 0);
            UpdateHoofdVoortgang();
            if (actieveBerekeningen == 0)
                Log("Alle gestarte berekeningen zijn afgerond.");
        }
    }

    private void ZetBewerkKnoppen(bool enabled)
    {
        // Zonder werkende database kunnen geen CRUD-/resetacties plaatsvinden.
        bool toegestaan = enabled && databaseGereed;
        InitButton.IsEnabled = enabled; // Bij een opstartfout blijft herinitialiseren mogelijk.
        ResetButton.IsEnabled = toegestaan;
        LoadButton.IsEnabled = toegestaan;
        AddButton.IsEnabled = toegestaan;
        UpdateButton.IsEnabled = toegestaan;
        DeleteButton.IsEnabled = toegestaan;
        // Berekenen van ANDERE personen blijft mogelijk.
    }

    private void UpdateHoofdVoortgang()
    {
        int afgerond = 0;
        int totaal = 0;
        foreach (var rij in persoonRijen.Values)
        {
            if (!rij.Gestart) continue;
            afgerond += rij.Afgerond;
            totaal += rij.Totaal;
        }
        double percentage = totaal == 0 ? 0 : 100.0 * afgerond / totaal;
        MainProgressBar.Value = percentage;
        MainProgressText.Text = $"Voortgang: {percentage:0}% " +
            $"({actieveBerekeningen} actief, {afgerond}/{totaal} combinaties)";
    }

    private void Log(string bericht)
    {
        string regel = $"[{DateTime.Now:HH:mm:ss}] {bericht}";
        LogListBox.Items.Add(regel);
        // Voorkomt dat het logboek bij veel herhaalde berekeningen onbeperkt groeit.
        if (LogListBox.Items.Count > 1500)
            LogListBox.Items.RemoveAt(0);
        LogListBox.ScrollIntoView(regel);
    }

    private void MeldingFout(string actie, Exception ex)
    {
        string leesbaar = GebruikersDialog.LeesbareFout(ex);
        StatusText.Text = $"Fout bij {actie}: {leesbaar}";
        Log($"FOUT bij {actie}: {leesbaar}");
        // Tijdens opstarten is het hoofdvenster nog niet zichtbaar.
        // Toon de modale foutmelding pas zodra het venster geopend is.
        if (IsVisible)
        {
            GebruikersDialog.ToonFout(this, actie, ex);
        }
        else
        {
            void NaOpenen(object? afzender, EventArgs argumenten)
            {
                Opened -= NaOpenen;
                GebruikersDialog.ToonFout(this, actie, ex);
            }
            Opened += NaOpenen;
        }
    }
}