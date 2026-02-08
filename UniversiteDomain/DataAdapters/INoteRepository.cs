using UniversiteDomain.Entities;
namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task<Note> AddNoteAsync(long idEtudiant, long idUe, float valeur);
    
    // Méthode pour récupérer la note d'un étudiant dans une UE
    Task<Note?> GetNoteAsync(long idEtudiant, long idUe);
}