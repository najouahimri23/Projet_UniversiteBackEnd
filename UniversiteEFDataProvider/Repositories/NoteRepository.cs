using Microsoft.EntityFrameworkCore;
using UniversiteEFDataProvider.Data;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context)
    : Repository<Note>(context), INoteRepository
{
    public async Task<Note> AddNoteAsync(long idEtudiant, long idUe, float valeur)
    {
        var note = new Note
        {
            EtudiantId = idEtudiant,
            UeId = idUe,
            Valeur = valeur
        };

        Context.Notes.Add(note);
        await Context.SaveChangesAsync();
        return note;
    }

    //  méthode obligatoire de l’interface
    public async Task<Note?> GetNoteAsync(long idEtudiant, long idUe)
    {
        return await Context.Notes.FindAsync(idEtudiant, idUe);
    }

    //  NOUVELLES MÉTHODES pour l'import CSV 
    public async Task<List<Note>> GetNotesByUeIdAsync(long ueId)
    {
        ArgumentNullException.ThrowIfNull(Context.Notes);
        return await Context.Notes
            .Include(n => n.Etudiant)
            .Include(n => n.Ue)
            .Where(n => n.UeId == ueId)
            .ToListAsync();
    }

    public async Task UpdateNoteAsync(Note note)
    {
        ArgumentNullException.ThrowIfNull(Context.Notes);
        Context.Notes.Update(note);
        await Context.SaveChangesAsync();
    }

    //add note 
    public async Task CreateNoteAsync(Note note)
    {
        ArgumentNullException.ThrowIfNull(Context.Notes);
        await Context.Notes.AddAsync(note);
        await Context.SaveChangesAsync();
    }
}