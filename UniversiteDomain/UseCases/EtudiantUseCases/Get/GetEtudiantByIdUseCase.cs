using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Get;

public class GetEtudiantByIdUseCase(IRepositoryFactory factory)
{
    public async Task<Etudiant?> ExecuteAsync(long idEtudiant)
    {
        await CheckBusinessRules();
        return await factory.EtudiantRepository().FindAsync(idEtudiant);
    }

    private Task CheckBusinessRules()
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(factory.EtudiantRepository());
        return Task.CompletedTask;
    }

    // Autorisations personnalisées :
    // - Responsable / Scolarité : accès à tous
    // - Etudiant : accès seulement à sa propre fiche
    public bool IsAuthorized(string role, IUniversiteUser user, long idEtudiant)
    {
        if (Roles.Responsable.Equals(role) || Roles.Scolarite.Equals(role))
            return true;

        return Roles.Etudiant.Equals(role)
               && user?.Etudiant != null
               && user.Etudiant.Id == idEtudiant;
    }
}