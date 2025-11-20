using UniversiteDomain.Entities;
namespace UniversiteDomain.DataAdapters;

public interface INoteRepository : IRepository<Note>
{
    Task<Note> AddNoteAsync(long idEtudiant, long idUe, float valeur);
}