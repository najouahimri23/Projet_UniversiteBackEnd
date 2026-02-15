using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;
using UniversiteDomaine.Exceptions.NoteExceptions;
using UniversiteDomaine.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases;

public class ImportNotesCsvUseCase
{
    private readonly IRepositoryFactory _repositoryFactory;

    public ImportNotesCsvUseCase(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public bool IsAuthorized(string role)
    {
        // Seule la scolarité peut importer le fichier CSV
        return role.Equals(Roles.Scolarite);
    }

    public async Task<string> ExecuteAsync(long ueId, Stream csvStream)
    {
        // Vérifier que l'UE existe
        var ue = await _repositoryFactory.UeRepository().FindAsync(ueId);
        if (ue == null)
            throw new ArgumentException($"L'UE avec l'ID {ueId} n'existe pas");

        // Lire et valider le fichier CSV
        var notesDto = await LireEtValiderCsvAsync(csvStream, ueId);

        // Si on arrive ici, aucune erreur n'a été trouvée
        // On peut donc enregistrer les notes en base
        int nbNotesImportees = await EnregistrerNotesAsync(notesDto, ueId);

        return $"{nbNotesImportees} note(s) importée(s) avec succès pour l'UE {ue.NumeroUe} - {ue.Intitule}";
    }

    private async Task<List<NoteCsvDto>> LireEtValiderCsvAsync(Stream csvStream, long ueId)
    {
        var notesDto = new List<NoteCsvDto>();
        var erreurs = new List<string>();

        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            MissingFieldFound = null
        });

        
        // NOUVELLE VALIDATION : Lire et vérifier l'en-tête UE
        var headerLine = await reader.ReadLineAsync();
        var ue = await _repositoryFactory.UeRepository().FindAsync(ueId);
        if (headerLine != null && !headerLine.Contains(ue.NumeroUe))
        {
            throw new InvalidCsvFormatException(
                $"Ce fichier CSV est prévu pour l'UE {headerLine.Split(':')[1].Trim().Split('-')[0].Trim()}, " +
                $"mais vous essayez de l'importer pour l'UE {ue.NumeroUe}. " +
                $"Veuillez télécharger le bon fichier CSV."
            );
        }
        
        // Lire le header
        await csv.ReadAsync();
        csv.ReadHeader();

        int numeroLigne = 2; // On commence à 2 car ligne 1 = info UE, ligne 2 = header

        while (await csv.ReadAsync())
        {
            numeroLigne++;
            var noteDto = csv.GetRecord<NoteCsvDto>();

            // Validation 1 : Vérifier que l'étudiant existe
            var etudiantExists = await _repositoryFactory.EtudiantRepository()
                .ExistsByNumEtudAsync(noteDto.NumEtud);
            
            if (!etudiantExists)
            {
                erreurs.Add($"Ligne {numeroLigne}: L'étudiant {noteDto.NumEtud} n'existe pas");
                continue;
            }

            // Validation 2 : Si une note est présente, vérifier qu'elle est valide
            if (!string.IsNullOrWhiteSpace(noteDto.Note))
            {
                if (!float.TryParse(noteDto.Note, NumberStyles.Any, CultureInfo.InvariantCulture, out float valeurNote))
                {
                    erreurs.Add($"Ligne {numeroLigne}: La note '{noteDto.Note}' n'est pas un nombre valide pour {noteDto.NumEtud}");
                    continue;
                }

                // Validation 3 : Vérifier que la note est entre 0 et 20
                if (valeurNote < 0 || valeurNote > 20)
                {
                    erreurs.Add($"Ligne {numeroLigne}: La note {valeurNote} doit être comprise entre 0 et 20 pour {noteDto.NumEtud}");
                    continue;
                }
            }

            notesDto.Add(noteDto);
        }

        // Si des erreurs ont été trouvées, on lève une exception avec toutes les erreurs
        if (erreurs.Any())
        {
            throw new InvalidCsvFormatException(erreurs); //  Passer la liste complète
        }
        return notesDto;
    }

    private async Task<int> EnregistrerNotesAsync(List<NoteCsvDto> notesDto, long ueId)
    {
        int nbNotesImportees = 0;

        foreach (var noteDto in notesDto)
        {
            // Si pas de note, on skip
            if (string.IsNullOrWhiteSpace(noteDto.Note))
                continue;

            float valeurNote = float.Parse(noteDto.Note, CultureInfo.InvariantCulture);

            // Récupérer l'ID de l'étudiant
            var etudiant = await _repositoryFactory.EtudiantRepository()
                .FindByNumEtudAsync(noteDto.NumEtud);
            
            if (etudiant == null) continue; // Normalement impossible car déjà validé

            // Vérifier si une note existe déjà
            var noteExistante = await _repositoryFactory.NoteRepository()
                .GetNoteAsync(etudiant.Id, ueId);

            if (noteExistante != null)
            {
                // Mettre à jour la note existante
                noteExistante.Valeur = valeurNote;
                await _repositoryFactory.NoteRepository().UpdateNoteAsync(noteExistante);
            }
            else
            {
                // Créer une nouvelle note
                var nouvelleNote = new Note
                {
                    EtudiantId = etudiant.Id,
                    UeId = ueId,
                    Valeur = valeurNote
                };
                await _repositoryFactory.NoteRepository().CreateNoteAsync(nouvelleNote);
            }

            nbNotesImportees++;
        }

        return nbNotesImportees;
    }
}