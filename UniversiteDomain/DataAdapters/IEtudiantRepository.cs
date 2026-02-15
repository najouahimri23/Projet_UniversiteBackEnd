using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IEtudiantRepository : IRepository<Etudiant>
{
    Task AffecterParcoursAsync(long idEtudiant, long idParcours);
    Task AffecterParcoursAsync(Etudiant etudiant, Parcours parcours);
    Task<Etudiant?> FindEtudiantCompletAsync(long idEtudiant);
    
    // NOUVELLES MÉTHODES pour l'import CSV
    Task<bool> ExistsByNumEtudAsync(string numEtud);
    Task<Etudiant?> FindByNumEtudAsync(string numEtud);
}