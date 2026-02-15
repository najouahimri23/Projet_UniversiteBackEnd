using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases;

public class GenerateNotesCsvUseCase
{
    private readonly IRepositoryFactory _repositoryFactory;

    public GenerateNotesCsvUseCase(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public bool IsAuthorized(string role)
    {
        // Seule la scolarité peut générer le fichier CSV
        return role.Equals(Roles.Scolarite);
    }

    public async Task<byte[]> ExecuteAsync(long ueId)
    {
        // Vérifier que l'UE existe
        var ue = await _repositoryFactory.UeRepository().FindAsync(ueId);
        if (ue == null)
            throw new ArgumentException($"L'UE avec l'ID {ueId} n'existe pas");

        // Récupérer tous les étudiants inscrits dans les parcours qui contiennent cette UE
        var etudiants = await GetEtudiantsInscritsDansUeAsync(ueId);
        
        // Récupérer les notes existantes pour cette UE
        var notesExistantes = await _repositoryFactory.NoteRepository().GetNotesByUeIdAsync(ueId);

        // Créer le fichier CSV
        using var memoryStream = new MemoryStream();
        using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true
        });

        // Écrire l'en-tête avec les informations de l'UE
        csv.WriteField($"# UE: {ue.NumeroUe} - {ue.Intitule}");
        await csv.NextRecordAsync();
        
        // Écrire les en-têtes de colonnes
        csv.WriteField("NumEtud");
        csv.WriteField("Nom");
        csv.WriteField("Prenom");
        csv.WriteField("Note");
        await csv.NextRecordAsync();

        // Écrire les lignes pour chaque étudiant
        foreach (var etudiant in etudiants)
        {
            csv.WriteField(etudiant.NumEtud);
            csv.WriteField(etudiant.Nom);
            csv.WriteField(etudiant.Prenom);
            
            // Chercher si une note existe déjà
            var noteExistante = notesExistantes.FirstOrDefault(n => n.EtudiantId == etudiant.Id);
            csv.WriteField(noteExistante != null ? noteExistante.Valeur.ToString(CultureInfo.InvariantCulture) : "");
            
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync();
        return memoryStream.ToArray();
    }

    /*private async Task<List<Etudiant>> GetEtudiantsInscritsDansUeAsync(long ueId)
    {
        // Récupérer tous les parcours qui contiennent cette UE
        var tousLesParcours = await _repositoryFactory.ParcoursRepository().FindAllAsync();
        var parcoursAvecUe = tousLesParcours.Where(p => p.UesEnseignees != null && p.UesEnseignees.Any(u => u.Id == ueId)).ToList();

        // Récupérer tous les étudiants inscrits dans ces parcours
        var etudiants = new List<Etudiant>();
        foreach (var parcours in parcoursAvecUe)
        {
            var etudsInParcours = await _repositoryFactory.EtudiantRepository()
                .FindByConditionAsync(e => e.ParcoursSuivi != null && e.ParcoursSuivi.Id == parcours.Id);
            etudiants.AddRange(etudsInParcours);
        }

        return etudiants.OrderBy(e => e.Nom).ThenBy(e => e.Prenom).ToList();
    }*/
    private async Task<List<Etudiant>> GetEtudiantsInscritsDansUeAsync(long ueId)
    {
        try
        {
            //  CHANGEMENT ICI : GetAllWithUesAsync au lieu de FindAllAsync
            var tousLesParcours = await _repositoryFactory.ParcoursRepository().GetAllWithUesAsync();
        
            // Filtrer les parcours qui contiennent cette UE
            var parcoursIds = tousLesParcours
                .Where(p => p.UesEnseignees != null && p.UesEnseignees.Any(u => u.Id == ueId))
                .Select(p => p.Id)
                .ToHashSet();

            if (parcoursIds.Any())
            {
                // Récupérer les étudiants inscrits dans ces parcours
                var etudiantsParParcours = await _repositoryFactory.EtudiantRepository()
                    .FindByConditionAsync(e => e.ParcoursSuivi != null && parcoursIds.Contains(e.ParcoursSuivi.Id));

                var etudiantsFiltres = etudiantsParParcours
                    .Where(e => !string.IsNullOrWhiteSpace(e.NumEtud))
                    .OrderBy(e => e.Nom)
                    .ThenBy(e => e.Prenom)
                    .ToList();

                if (etudiantsFiltres.Any())
                    return etudiantsFiltres;
            }
        }
        catch
        {
            // Si erreur, continuer avec le fallback
        }

        // FALLBACK : Si aucun étudiant trouvé par parcours, retourner tous les étudiants
        var tousEtudiants = await _repositoryFactory.EtudiantRepository().FindAllAsync();
        return tousEtudiants
            .Where(e => !string.IsNullOrWhiteSpace(e.NumEtud))
            .OrderBy(e => e.Nom)
            .ThenBy(e => e.Prenom)
            .ToList();
    }
    
}