using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class EtudiantRepository(UniversiteDbContext context) : Repository<Etudiant>(context), IEtudiantRepository
{
    public async Task AffecterParcoursAsync(long idEtudiant, long idParcours)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        Etudiant e = (await Context.Etudiants.FindAsync(idEtudiant))!;
        Parcours p = (await Context.Parcours.FindAsync(idParcours))!;
        e.ParcoursSuivi = p;
        await Context.SaveChangesAsync();
    }
    
    public async Task AffecterParcoursAsync(Etudiant etudiant, Parcours parcours)
    {
        await AffecterParcoursAsync(etudiant.Id, parcours.Id); 
    }
    public async Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        return await Context.Etudiants.Include(e => e.NotesObtenues).ThenInclude(n=>n.Ue).FirstOrDefaultAsync(e => e.Id == idEtudiant);
    }

    //  NOUVELLES MÉTHODES pour l'import CSV 
    public async Task<bool> ExistsByNumEtudAsync(string numEtud)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        return await Context.Etudiants.AnyAsync(e => e.NumEtud == numEtud);
    }

    public async Task<Etudiant?> FindByNumEtudAsync(string numEtud)
    {
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        return await Context.Etudiants.FirstOrDefaultAsync(e => e.NumEtud == numEtud);
    }
}
