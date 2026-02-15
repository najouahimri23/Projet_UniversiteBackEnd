using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.SecurityUseCases.Update;

public class UpdateUniversiteUserUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(string oldEmail, string newEmail)
    {
        await CheckBusinessRules(oldEmail, newEmail);

        var repo = factory.UniversiteUserRepository();

        var user = await repo.FindByEmailAsync(oldEmail);
        if (user == null) throw new InvalidOperationException("Utilisateur introuvable");

        // On garde la convention: UserName = Email
        await repo.UpdateAsync(user, newEmail, newEmail);

        await factory.SaveChangesAsync();
    }

    private static Task CheckBusinessRules(string oldEmail, string newEmail)
    {
        ArgumentNullException.ThrowIfNull(oldEmail);
        ArgumentNullException.ThrowIfNull(newEmail);
        return Task.CompletedTask;
    }
    

    public bool IsAuthorized(string role)
        => Roles.Responsable.Equals(role) || Roles.Scolarite.Equals(role);
}