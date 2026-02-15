


using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Util;
using UniversiteDomaine.Entities;
using UniversiteDomaine.Exceptions.EtudiantExceptions;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Update;

public class UpdateEtudiantUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(Etudiant etudiant)
    {
        await CheckBusinessRules(etudiant);

        await factory.EtudiantRepository().UpdateAsync(etudiant);
        await factory.SaveChangesAsync();
    }

    private async Task CheckBusinessRules(Etudiant etudiant)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(etudiant);
        ArgumentNullException.ThrowIfNull(etudiant.Email);
        ArgumentNullException.ThrowIfNull(etudiant.NumEtud);

        var repository = factory.EtudiantRepository();

        // Vérifier que l'étudiant existe
        var existing = await repository.FindAsync(etudiant.Id);
        if (existing == null)
            throw new InvalidOperationException("Etudiant introuvable");

        // Vérifier email valide
        if (!CheckEmail.IsValidEmail(etudiant.Email))
            throw new InvalidEmailException("Email mal formé");

        // Vérifier nom minimum 3 caractères
        if (etudiant.Nom.Length < 3)
            throw new InvalidNomEtudiantException(
                etudiant.Nom + " incorrect - Le nom d'un étudiant doit contenir plus de 3 caractères");
    }

    public bool IsAuthorized(string role)
    {
        return Roles.Responsable.Equals(role) 
               || Roles.Scolarite.Equals(role);
    }
}