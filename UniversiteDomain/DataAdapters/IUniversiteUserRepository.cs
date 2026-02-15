using UniversiteDomain.Entities;
using UniversiteDomaine.Entities;

namespace UniversiteDomain.DataAdapters;

public interface IUniversiteUserRepository : IRepository<IUniversiteUser>
{
    // Ajout d'un utilisateur à la liste des utilisateurs avec son rôle et son lien vers l'étudiant le cas échéant 
    Task<IUniversiteUser?> AddUserAsync(string login, string email, string password, string role, Etudiant? etudiant);
    // Recherched'un utilisteur à partir de son email
    Task<IUniversiteUser> FindByEmailAsync(string email);
    // Mise à jour d'un user
    Task UpdateAsync(IUniversiteUser entity, string userName, string email);
    //Récupération des rôles  d'un user
    Task<List<string>> GetRolesAsync(IUniversiteUser user);
    // Vérification de laprésence d'un rôle affecté à un user
    Task<bool> IsInRoleAsync(string email, string role);
    // Vérification du mot de passe saisi par le user
    public Task<bool> CheckPasswordAsync(IUniversiteUser user, string password);
}