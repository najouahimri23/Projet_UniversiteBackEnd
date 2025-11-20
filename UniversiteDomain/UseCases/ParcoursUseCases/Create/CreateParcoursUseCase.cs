using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase
{
    private readonly IParcoursRepository parcoursRepository;

    public CreateParcoursUseCase(IRepositoryFactory repositoryFactory)
    {
        parcoursRepository = repositoryFactory.ParcoursRepository();
    }
    public async Task<Parcours> ExecuteAsync(string nomParcours, int anneeFormation)
    {
        var parcours = new Parcours { NomParcours = nomParcours, AnneeFormation = anneeFormation };
        return await ExecuteAsync(parcours);
    }
    

    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);
        Parcours p = await parcoursRepository.CreateAsync(parcours);
        parcoursRepository.SaveChangesAsync().Wait();
        return p;
    }

    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);
        ArgumentNullException.ThrowIfNull(parcoursRepository);

        // Unicité : même nom + même année
        List<Parcours> existe = await parcoursRepository
            .FindByConditionAsync(p => p.NomParcours.Equals(parcours.NomParcours)
                                       && p.AnneeFormation.Equals(parcours.AnneeFormation));
        if (existe is { Count: > 0 })
            throw new DuplicateInscriptionException(parcours.NomParcours + " - ce parcours existe déjà pour l'année " + parcours.AnneeFormation);

        // Règles métier
        if (parcours.NomParcours.Length < 3)
            throw new ParcoursNotFoundException(parcours.NomParcours + " incorrect - Le nom d'un parcours doit contenir plus de 3 caractères");

        if (parcours.AnneeFormation < 1 || parcours.AnneeFormation > 5)
            throw new InvalidAnneeFormationException("Année de formation " + parcours.AnneeFormation + " incorrecte - doit être comprise entre 1 et 5");
    }
}