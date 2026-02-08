using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomaine.Exceptions.NoteExceptions;
using UniversiteDomaine.Exceptions.UeExceptions;

namespace UniversiteDomaine.UseCases.NoteUseCases;

public class AddNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(long idEtudiant, long idUe, float valeur)
    {
        await CheckBusinessRules(idEtudiant, idUe, valeur);
        return await repositoryFactory.NoteRepository().AddNoteAsync(idEtudiant, idUe, valeur);
    }

    // 🆕 Surcharge pour plus de flexibilité (optionnel mais recommandé)
    public async Task<Note> ExecuteAsync(Etudiant etudiant, Ue ue, float valeur)
    {
        ArgumentNullException.ThrowIfNull(etudiant);
        ArgumentNullException.ThrowIfNull(ue);
        return await ExecuteAsync(etudiant.Id, ue.Id, valeur);
    }

    private async Task CheckBusinessRules(long idEtudiant, long idUe, float valeur)
    {
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idEtudiant);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idUe);

        // Vérifier les repositories
        var etudiantRepo = repositoryFactory.EtudiantRepository();
        var ueRepo = repositoryFactory.UeRepository();
        var noteRepo = repositoryFactory.NoteRepository();

        ArgumentNullException.ThrowIfNull(etudiantRepo);
        ArgumentNullException.ThrowIfNull(ueRepo);
        ArgumentNullException.ThrowIfNull(noteRepo);

        // ⚡ OPTIMISATION : Vérifier la note AVANT d'interroger la DB
        if (valeur < 0 || valeur > 20)
            throw new InvalidNoteValueException($"La note {valeur} n'est pas valide (doit être comprise entre 0 et 20)");

        //  Vérifier que l'étudiant existe
        var etudiant = await etudiantRepo.FindByConditionAsync(e => e.Id.Equals(idEtudiant));
        if (etudiant is { Count: 0 })
            throw new EtudiantNotFoundException($"Étudiant {idEtudiant} introuvable");

        //  Vérifier que l'UE existe
        var ue = await ueRepo.FindByConditionAsync(u => u.Id.Equals(idUe));
        if (ue is { Count: 0 })
            throw new UeNotFoundException($"UE {idUe} introuvable");

        //  Vérifier que l'étudiant a un parcours
        var parcoursEtudiant = etudiant[0].ParcoursSuivi;
        if (parcoursEtudiant == null)
            throw new EtudiantNotInParcoursException($"L'étudiant {idEtudiant} n'est inscrit dans aucun parcours");

        //  Vérifier que l'UE fait partie du parcours de l'étudiant
        if (parcoursEtudiant.UesEnseignees?.Find(u => u.Id == idUe) == null)
            throw new UeNotInParcoursException($"L'UE {idUe} ne fait pas partie du parcours de l'étudiant {idEtudiant}");

        //  Vérifier qu'il n'a pas déjà une note dans cette UE
        var noteExistante = await noteRepo.FindByConditionAsync(n =>
            n.IdEtudiant == idEtudiant && n.IdUe == idUe);
        if (noteExistante is { Count: > 0 })
            throw new DuplicateNoteException($"L'étudiant {idEtudiant} a déjà une note dans l'UE {idUe}");
    }
}