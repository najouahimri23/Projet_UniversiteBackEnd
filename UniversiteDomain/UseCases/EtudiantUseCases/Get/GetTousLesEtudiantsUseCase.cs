using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Get;

public class GetTousLesEtudiantsUseCase(IRepositoryFactory factory)
{
    public async Task<List<Etudiant>> ExecuteAsync()
    {
        await CheckBusinessRules();
        return await factory.EtudiantRepository().FindAllAsync();
    }

    private Task CheckBusinessRules()
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(factory.EtudiantRepository());
        return Task.CompletedTask;
    }

    // Autorisation :
    // Seuls Responsable et Scolarite peuvent voir tous les étudiants
    public bool IsAuthorized(string role)
    {
        return Roles.Responsable.Equals(role) 
               || Roles.Scolarite.Equals(role);
    }
}