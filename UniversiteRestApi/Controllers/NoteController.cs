using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomaine.Exceptions.NoteExceptions;
using UniversiteDomain.UseCases.NoteUseCases;
using UniversiteDomain.UseCases.SecurityUseCases.Get;

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NoteController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET: api/Note/csv/{ueId}
    // Génère un fichier CSV pour saisir les notes d'une UE
    [HttpGet("csv/{ueId}")]
    public async Task<IActionResult> GenerateCsv(long ueId)
    {
        string role = "";
        string email = "";
        IUniversiteUser user = null;
        
        try
        {
            CheckSecu(out role, out email, out user);
        }
        catch (Exception)
        {
            return Unauthorized();
        }

        GenerateNotesCsvUseCase uc = new GenerateNotesCsvUseCase(repositoryFactory);
        
        if (!uc.IsAuthorized(role))
            return Unauthorized("Seule la scolarité peut générer un fichier CSV de notes");

        try
        {
            byte[] csvBytes = await uc.ExecuteAsync(ueId);
            return File(csvBytes, "text/csv", $"notes_ue_{ueId}.csv");
        }
        catch (ArgumentException e)
        {
            //  Erreur métier claire (UE inexistante, etc.)
            return BadRequest(new { error = e.Message });
        }
        catch (Exception e)
        {
            //  Erreur inattendue
            return StatusCode(500, new { error = "Une erreur interne s'est produite", details = e.Message });
        }
    }

    // POST: api/Note/csv/{ueId}
    // Importe les notes depuis un fichier CSV
    [HttpPost("csv/{ueId}")]
    public async Task<IActionResult> ImportCsv(long ueId, IFormFile? file)
    {
        string role = "";
        string email = "";
        IUniversiteUser user = null;
        
        try
        {
            CheckSecu(out role, out email, out user);
        }
        catch (Exception)
        {
            return Unauthorized();
        }

        ImportNotesCsvUseCase uc = new ImportNotesCsvUseCase(repositoryFactory);
        
        if (!uc.IsAuthorized(role))
            return Unauthorized("Seule la scolarité peut importer un fichier CSV de notes");

        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier n'a été fourni");

        if (!file.FileName.EndsWith(".csv"))
            return BadRequest("Le fichier doit être au format CSV");

        try
        {
            using var stream = file.OpenReadStream();
            string resultat = await uc.ExecuteAsync(ueId, stream);
            return Ok(new { message = resultat });
        }
        catch (InvalidCsvFormatException e)
        {
            // ✅ Erreurs de validation CSV avec détails
            var erreurs = e.Message.Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            return BadRequest(new { 
                error = "Le fichier CSV contient des erreurs de validation", 
                details = erreurs 
            });
        }
        catch (ArgumentException e)
        {
            //  Erreur métier claire (UE inexistante, etc.)
            return BadRequest(new { error = e.Message });
        }
        catch (Exception e)
        {
            //  Erreur inattendue
            return StatusCode(500, new { error = "Une erreur interne s'est produite", details = e.Message });
        }
    }

    private void CheckSecu(out string role, out string email, out IUniversiteUser user)
    {
        role = "";
        ClaimsPrincipal claims = HttpContext.User;
        if (claims.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException();
        if (claims.FindFirst(ClaimTypes.Email) == null) throw new UnauthorizedAccessException();
        email = claims.FindFirst(ClaimTypes.Email).Value;
        if (email == null) throw new UnauthorizedAccessException();
        user = new FindUniversiteUserByEmailUseCase(repositoryFactory).ExecuteAsync(email).Result;
        if (user == null) throw new UnauthorizedAccessException();
        if (claims.FindFirst(ClaimTypes.Role) == null) throw new UnauthorizedAccessException();
        var ident = claims.Identities.FirstOrDefault();
        if (ident == null) throw new UnauthorizedAccessException();
        role = ident.FindFirst(ClaimTypes.Role).Value;
        if (role == null) throw new UnauthorizedAccessException();
        bool isInRole = new IsInRoleUseCase(repositoryFactory).ExecuteAsync(email, role).Result;
        if (!isInRole) throw new UnauthorizedAccessException();
    }
}