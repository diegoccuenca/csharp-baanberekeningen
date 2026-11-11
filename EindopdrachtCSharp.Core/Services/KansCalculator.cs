using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EindopdrachtCSharp.Services;

/// <summary>De voortgang van één afgeronde opleiding-beroepcombinatie.</summary>
/// <param name="Afgerond">Aantal verwerkte combinaties.</param>
/// <param name="Totaal">Totaal aantal te verwerken combinaties.</param>
/// <param name="Opleiding">Naam van de gebruikte opleiding.</param>
/// <param name="Beroep">Naam van het gebruikte beroep.</param>
/// <param name="OpleidingsKans">Gegenereerde kans voor de opleiding, van 0 tot 1.</param>
/// <param name="BeroepsKans">Gegenereerde kans voor het beroep, van 0 tot 1.</param>
/// <param name="EindKans">Product van beide kansen.</param>
public sealed record KansVoortgang(
    int Afgerond,
    int Totaal,
    string Opleiding,
    string Beroep,
    double OpleidingsKans,
    double BeroepsKans,
    double EindKans);

/// <summary>De opleiding en het beroep met de hoogste berekende combinatiekans.</summary>
/// <param name="BesteOpleiding">De bijbehorende opleiding.</param>
/// <param name="BesteBeroep">De bijbehorende baan.</param>
/// <param name="HoogsteKans">De hoogste combinatiekans als getal tussen 0 en 1.</param>
public sealed record KansResultaat(
    string BesteOpleiding,
    string BesteBeroep,
    double HoogsteKans);

/// <summary>
/// Voert 10 × 10 kansberekeningen (of het actuele aantal uit de database)
/// onafhankelijk van de GUI uit. De willekeurige wachttijd bij ieder paar
/// is 100–1000 milliseconden. Rapportage gebeurt via IProgress.
/// </summary>
public static class KansCalculator
{
    /// <summary>Doorloopt alle combinaties asynchroon, inclusief een willekeurige pauze van 100–1000 ms per combinatie.</summary>
    /// <param name="opleidingen">De beschikbare opleidingen uit de database.</param>
    /// <param name="beroepen">De beschikbare beroepen uit de database.</param>
    /// <param name="voortgang">Ontvanger voor voortgangsmeldingen na iedere combinatie.</param>
    /// <param name="cancellationToken">Optionele annulering van de lopende berekening.</param>
    /// <returns>De combinatie met de hoogste kans.</returns>
    /// <exception cref="InvalidOperationException">Wanneer opleidingen of beroepen ontbreken.</exception>
    public static async Task<KansResultaat> BerekenAsync(
        IReadOnlyList<string> opleidingen,
        IReadOnlyList<string> beroepen,
        IProgress<KansVoortgang> voortgang,
        CancellationToken cancellationToken = default)
    {
        if (opleidingen.Count == 0 || beroepen.Count == 0)
            throw new InvalidOperationException(
                "De database moet opleidingen en beroepen bevatten. Gebruik Reset Database om voorbeeldgegevens terug te zetten.");

        // Per opleiding en per beroep één willekeurige kans in [0, 1).
        // De kans van iedere combinatie = opleidingskans × beroepskans.
        var opleidingsKansen = new double[opleidingen.Count];
        for (int i = 0; i < opleidingen.Count; i++)
            opleidingsKansen[i] = Random.Shared.NextDouble();

        var beroepsKansen = new double[beroepen.Count];
        for (int i = 0; i < beroepen.Count; i++)
            beroepsKansen[i] = Random.Shared.NextDouble();

        var totaal = checked(opleidingen.Count * beroepen.Count);
        int afgerond = 0;
        double hoogsteKans = -1.0;
        string besteOpleiding = "";
        string besteBeroep = "";

        for (int i = 0; i < opleidingen.Count; i++)
        {
            for (int j = 0; j < beroepen.Count; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(Random.Shared.Next(100, 1001), cancellationToken)
                    .ConfigureAwait(false);

                double kans = opleidingsKansen[i] * beroepsKansen[j];
                if (kans > hoogsteKans)
                {
                    hoogsteKans = kans;
                    besteOpleiding = opleidingen[i];
                    besteBeroep = beroepen[j];
                }

                afgerond++;
                voortgang.Report(new KansVoortgang(
                    afgerond, totaal, opleidingen[i], beroepen[j],
                    opleidingsKansen[i], beroepsKansen[j], kans));
            }
        }

        return new KansResultaat(besteOpleiding, besteBeroep, hoogsteKans);
    }
}
