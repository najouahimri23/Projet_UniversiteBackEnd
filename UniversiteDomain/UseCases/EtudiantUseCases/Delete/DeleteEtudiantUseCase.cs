using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;
using UniversiteDomaine.Exceptions.EtudiantExceptions;

namespace UniversiteDomaine.UseCases.EtudiantUseCases.Delete;

public class DeleteEtudiantUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task ExecuteAsync(long idEtudiant)
    {
        await CheckBusinessRules(idEtudiant);
        
        var etudiantRepo = repositoryFactory.EtudiantRepository();
        await etudiantRepo.DeleteAsync(idEtudiant);
        await etudiantRepo.SaveChangesAsync();
    }

    private async Task CheckBusinessRules(long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(idEtudiant);

        var etudiantRepo = repositoryFactory.EtudiantRepository();
        ArgumentNullException.ThrowIfNull(etudiantRepo);

        // Vérifier que l'étudiant existe
        var etudiant = await etudiantRepo.FindAsync(idEtudiant);
        if (etudiant == null)
            throw new EtudiantNotFoundException($"Étudiant {idEtudiant} introuvable");
    }

    // Méthode d'autorisation
    public bool IsAuthorized(string role)
    {
        // Seuls Responsable et Scolarité peuvent supprimer des étudiants
        return role.Equals(Roles.Responsable) || role.Equals(Roles.Scolarite);
    }
}