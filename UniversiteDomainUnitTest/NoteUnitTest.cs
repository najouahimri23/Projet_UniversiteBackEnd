using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;
using UniversiteDomaine.UseCases.NoteUseCases;

namespace UniversiteDomainUnitTests;

public class NoteUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task AddNoteUseCase_Success()
    {
        // 🔹 Données simulées
        long idEtudiant = 1;
        long idUe = 10;
        float valeurNote = 15.5f;

        var ue = new Ue { Id = idUe, NumeroUe = "UE101", Intitule = "Programmation C#" };
        var parcours = new Parcours
        {
            Id = 3,
            NomParcours = "M1 MIAGE",
            AnneeFormation = 2024,
            UesEnseignees = new List<Ue> { ue }
        };
        var etudiant = new Etudiant
        {
            Id = idEtudiant,
            NumEtud = "E123",
            Nom = "saf",
            Prenom = "Dounia",
            Email = "saf.dounia@upjv.fr",
            ParcoursSuivi = parcours
        };

        var noteFinale = new Note
        {
            EtudiantId = idEtudiant,
            UeId = idUe,
            Valeur = valeurNote
        };

        // 🔹 Mocks des repositories
        var mockEtudiantRepo = new Mock<IEtudiantRepository>();
        var mockUeRepo = new Mock<IUeRepository>();
        var mockNoteRepo = new Mock<INoteRepository>();

        // L'étudiant existe
        mockEtudiantRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(new List<Etudiant> { etudiant });

        // L'UE existe
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ue });

        // Aucune note existante
        mockNoteRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Note, bool>>>()))
            .ReturnsAsync(new List<Note>());

        // L'ajout de la note renvoie l'objet noteFinale
        mockNoteRepo
            .Setup(repo => repo.AddNoteAsync(idEtudiant, idUe, valeurNote))
            .ReturnsAsync(noteFinale);

        // 🔹 Mock de la factory
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(f => f.EtudiantRepository()).Returns(mockEtudiantRepo.Object);
        mockFactory.Setup(f => f.UeRepository()).Returns(mockUeRepo.Object);
        mockFactory.Setup(f => f.NoteRepository()).Returns(mockNoteRepo.Object);

        // 🔹 Exécution du Use Case
        var useCase = new AddNoteUseCase(mockFactory.Object);
        var result = await useCase.ExecuteAsync(idEtudiant, idUe, valeurNote);

        // 🔹 Vérifications
        Assert.That(result, Is.Not.Null);
        Assert.That(result.EtudiantId, Is.EqualTo(idEtudiant));
        Assert.That(result.UeId, Is.EqualTo(idUe));
        Assert.That(result.Valeur, Is.EqualTo(valeurNote));

    }
}