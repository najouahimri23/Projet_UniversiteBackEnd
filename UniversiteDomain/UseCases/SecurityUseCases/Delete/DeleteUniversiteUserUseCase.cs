using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.SecurityUseCases.Delete;

public class DeleteUniversiteUserUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(long idEtudiant)
    {
        await CheckBusinessRules();

        // On supprime le user associé à l'étudiant
        await factory.UniversiteUserRepository().DeleteAsync(idEtudiant);

        await factory.SaveChangesAsync();
    }

    private Task CheckBusinessRules()
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(factory.UniversiteUserRepository());
        return Task.CompletedTask;
    }

    public bool IsAuthorized(string role)
    {
        return Roles.Responsable.Equals(role) 
               || Roles.Scolarite.Equals(role);
    }
}