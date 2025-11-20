namespace UniversiteDomain.Entities;

public class Note
{
    public long IdEtudiant { get; set; }
    public long IdUe { get; set; }
    public float Valeur { get; set; }

    // ManyToOne : une note appartient à un étudiant
    public Etudiant? Etudiant { get; set; }

    // ManyToOne : une note appartient à une UE
    public Ue? Ue { get; set; }

    public override string ToString()
    {
        return $"Note {Valeur}/20 pour Etudiant={IdEtudiant} dans Ue={IdUe}";
    }
}