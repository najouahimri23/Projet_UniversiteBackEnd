using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.UeUseCases.Create;
using UniversiteDomaine.Exceptions.UeExceptions;

namespace UniversiteDomainUnitTests;

public class UeUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CreateUeUseCase_Success()
    {
        // 🔹 Données de test
        long id = 1;
        string numeroUe = "INFO101";
        string intitule = "Programmation Orientée Objet";

        // On crée l'UE qui doit être ajoutée en base (sans ID)
        Ue ueSansId = new Ue
        {
            NumeroUe = numeroUe,
            Intitule = intitule
        };

        // 🔹 Créons le mock du repository
        var mockUeRepo = new Mock<IUeRepository>();

        // Simulation de FindByConditionAsync
        // L'UE n'existe pas encore (pas de doublon de numéro)
        var reponseFindByCondition = new List<Ue>();
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(reponseFindByCondition);

        // Simulation de CreateAsync
        // L'ajout renvoie l'UE avec son ID généré
        Ue ueCree = new Ue
        {
            Id = id,
            NumeroUe = numeroUe,
            Intitule = intitule
        };
        mockUeRepo
            .Setup(repo => repo.CreateAsync(ueSansId))
            .ReturnsAsync(ueCree);

        // Mock de SaveChangesAsync
        mockUeRepo
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // On crée le faux repository
        var fauxUeRepository = mockUeRepo.Object;

        // 🔹 Création du use case en injectant notre faux repository
        CreateUeUseCase useCase = new CreateUeUseCase(fauxUeRepository);

        // 🔹 Appel du use case
        var ueTeste = await useCase.ExecuteAsync(ueSansId);

        // 🔹 Vérification du résultat
        Assert.That(ueTeste.Id, Is.EqualTo(ueCree.Id));
        Assert.That(ueTeste.NumeroUe, Is.EqualTo(ueCree.NumeroUe));
        Assert.That(ueTeste.Intitule, Is.EqualTo(ueCree.Intitule));
    }

    [Test]
    public void CreateUeUseCase_IntituleTooShort_ThrowsException()
    {
        // 🔹 Données de test avec intitulé trop court (≤ 3 caractères)
        string numeroUe = "INFO101";
        string intitule = "POO"; // Seulement 3 caractères

        Ue ue = new Ue
        {
            NumeroUe = numeroUe,
            Intitule = intitule
        };

        // 🔹 Mock du repository
        var mockUeRepo = new Mock<IUeRepository>();

        // Mock de SaveChangesAsync
        mockUeRepo
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // 🔹 Création du use case
        CreateUeUseCase useCase = new CreateUeUseCase(mockUeRepo.Object);

        // 🔹 Vérification qu'une exception est levée
        Assert.ThrowsAsync<UeIntituleTooShortException>(async () =>
        {
            await useCase.ExecuteAsync(ue);
        });
    }

    [Test]
    public void CreateUeUseCase_DuplicateNumero_ThrowsException()
    {
        // 🔹 Données de test
        string numeroUe = "INFO101";
        string intitule = "Programmation Orientée Objet";

        // UE existante avec le même numéro
        Ue ueExistante = new Ue
        {
            Id = 1,
            NumeroUe = numeroUe,
            Intitule = "Bases de données"
        };

        // Nouvelle UE avec le même numéro (devrait échouer)
        Ue nouvelleUe = new Ue
        {
            NumeroUe = numeroUe,
            Intitule = intitule
        };

        // 🔹 Mock du repository
        var mockUeRepo = new Mock<IUeRepository>();

        // Une UE avec ce numéro existe déjà
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue> { ueExistante });

        // Mock de SaveChangesAsync
        mockUeRepo
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // 🔹 Création du use case
        CreateUeUseCase useCase = new CreateUeUseCase(mockUeRepo.Object);

        // 🔹 Vérification qu'une exception est levée
        Assert.ThrowsAsync<DuplicateUeNumeroException>(async () =>
        {
            await useCase.ExecuteAsync(nouvelleUe);
        });
    }

    [Test]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("AB")]
    [TestCase("C")]
    [TestCase("123")]
    public void CreateUeUseCase_InvalidIntitule_ThrowsException(string intituleInvalide)
    {
        // 🔹 Données de test avec différents intitulés invalides
        Ue ue = new Ue
        {
            NumeroUe = "INFO101",
            Intitule = intituleInvalide
        };

        // 🔹 Mock du repository
        var mockUeRepo = new Mock<IUeRepository>();

        // Mock de SaveChangesAsync
        mockUeRepo
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // 🔹 Création du use case
        CreateUeUseCase useCase = new CreateUeUseCase(mockUeRepo.Object);

        // 🔹 Vérification qu'une exception est levée
        Assert.ThrowsAsync<UeIntituleTooShortException>(async () =>
        {
            await useCase.ExecuteAsync(ue);
        });
    }

    [Test]
    public async Task CreateUeUseCase_WithValidParameters_Success()
    {
        // 🔹 Test avec la surcharge (string, string)
        long id = 2;
        string numeroUe = "MATH201";
        string intitule = "Analyse Numérique Avancée";

        // 🔹 Mock du repository
        var mockUeRepo = new Mock<IUeRepository>();

        // Pas de doublon
        mockUeRepo
            .Setup(repo => repo.FindByConditionAsync(It.IsAny<Expression<Func<Ue, bool>>>()))
            .ReturnsAsync(new List<Ue>());

        // Création réussie
        mockUeRepo
            .Setup(repo => repo.CreateAsync(It.IsAny<Ue>()))
            .ReturnsAsync((Ue u) => { u.Id = id; return u; });

        // Mock de SaveChangesAsync
        mockUeRepo
            .Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // 🔹 Création du use case
        CreateUeUseCase useCase = new CreateUeUseCase(mockUeRepo.Object);

        // 🔹 Appel avec paramètres string
        var ueTeste = await useCase.ExecuteAsync(numeroUe, intitule);

        // 🔹 Vérifications
        Assert.That(ueTeste.Id, Is.EqualTo(id));
        Assert.That(ueTeste.NumeroUe, Is.EqualTo(numeroUe));
        Assert.That(ueTeste.Intitule, Is.EqualTo(intitule));
    }
}