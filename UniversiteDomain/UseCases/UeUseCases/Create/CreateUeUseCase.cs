using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomaine.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.UeUseCases.Create;

public class CreateUeUseCase(IUeRepository ueRepository)
{
    public async Task<Ue> ExecuteAsync(string numeroUe, string intitule)
    {
        var ue = new Ue { NumeroUe = numeroUe, Intitule = intitule };
        return await ExecuteAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);
        Ue u = await ueRepository.CreateAsync(ue);
        ueRepository.SaveChangesAsync().Wait();
        return u;
    }

    private async Task CheckBusinessRules(Ue ue)
    {
        // Vérification des paramètres
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);
        ArgumentNullException.ThrowIfNull(ueRepository);

        // Règle 1 : L'intitulé doit avoir plus de 3 caractères
        if (ue.Intitule.Trim().Length <= 3)
        {
            throw new UeIntituleTooShortException(
                ue.Intitule + " incorrect - L'intitulé d'une UE doit contenir plus de 3 caractères"
            );
        }

        // Règle 2 : Deux UEs ne peuvent pas avoir le même numéro
        List<Ue> existe = await ueRepository.FindByConditionAsync(u => u.NumeroUe.Equals(ue.NumeroUe));
        
        if (existe is { Count: > 0 })
        {
            throw new DuplicateUeNumeroException(
                ue.NumeroUe + " - ce numéro d'UE est déjà affecté à une UE"
            );
        }
    }
}