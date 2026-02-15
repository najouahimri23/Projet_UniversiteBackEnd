using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomaine.Entities;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class ParcoursRepository : Repository<Parcours>, IParcoursRepository
{
    public ParcoursRepository(UniversiteDbContext context) : base(context) { }

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long idEtudiant)
    {
        var parcours = await Context.Parcours!.FindAsync(idParcours)
            ?? throw new Exception("Parcours introuvable");

        var etudiant = await Context.Etudiants!.FindAsync(idEtudiant)
            ?? throw new Exception("Étudiant introuvable");

        parcours.Inscrits!.Add(etudiant);
        await Context.SaveChangesAsync();
        return parcours;
    }

    public async Task<Parcours> AddEtudiantAsync(long idParcours, long[] idEtudiants)
    {
        foreach (var id in idEtudiants)
            await AddEtudiantAsync(idParcours, id);

        return await Context.Parcours!.FindAsync(idParcours)
            ?? throw new Exception("Parcours introuvable");
    }

    public async Task<Parcours> AddEtudiantAsync(Parcours parcours, Etudiant etudiant)
    {
        return await AddEtudiantAsync(parcours.Id, etudiant.Id);
    }

    public async Task<Parcours> AddEtudiantAsync(Parcours? parcours, List<Etudiant> etudiants)
    {
        if (parcours == null) throw new ArgumentNullException(nameof(parcours));

        foreach (var etu in etudiants)
            await AddEtudiantAsync(parcours.Id, etu.Id);

        return parcours;
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long idUe)
    {
        var parcours = await Context.Parcours!.FindAsync(idParcours)
            ?? throw new Exception("Parcours introuvable");

        var ue = await Context.Ues!.FindAsync(idUe)
            ?? throw new Exception("UE introuvable");

        parcours.UesEnseignees!.Add(ue);
        await Context.SaveChangesAsync();
        return parcours;
    }

    public async Task<Parcours> AddUeAsync(long idParcours, long[] idUes)
    {
        foreach (var id in idUes)
            await AddUeAsync(idParcours, id);

        return await Context.Parcours!.FindAsync(idParcours)
            ?? throw new Exception("Parcours introuvable");
    }

    
    
    public async Task<List<Parcours>> GetAllWithUesAsync()
    {
        return await Context.Parcours
            .Include(p => p.UesEnseignees)  // ← Charge les UEs avec les parcours
            .ToListAsync();
    }
}