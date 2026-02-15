using UniversiteDomain.Entities;

namespace UniversiteDomaine.Entities;

public class Note
{
    public long EtudiantId { get; set; }
    public long UeId { get; set; }
    public float Valeur { get; set; }

    // ManyToOne : une note appartient à un étudiant
    public Etudiant? Etudiant { get; set; }

    // ManyToOne : une note appartient à une UE
    public Ue? Ue { get; set; }

    public override string ToString()
    {
        return $"Note {Valeur}/20 pour Etudiant={EtudiantId} dans Ue={UeId}";
    }
}