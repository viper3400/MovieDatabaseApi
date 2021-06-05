using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Jaxx.WebApi.Shared.Identity.Services
{
    public interface IIdentityService
    {
        void InitIdentity(string password, CancellationToken ct);
        Task<IdentityResult> AddRole(string role);
        Task<IdentityResult> AddUser(IdentityUser user, string password, CancellationToken ct);
        Task<IdentityResult> ChangePassword(string user, string currentPassword, string newPassword);
        Task<IdentityResult> CheckPassword(string name, string password);
        Task<IdentityResult> DeleteRole(string roleId);
        Task<IdentityResult> DeleteUser(string userId);
        Task<List<IdentityRole>> GetRoles(CancellationToken ct);
        Task<List<IdentityUser>> GetUsers();
        Task<IdentityResult> AddUserToRole(string username, string rolename);
        Task<IList<string>> GetRolesForUser(string username);
        Task<IList<IdentityUser>> GetUsersForRole(string role);
        Task<IdentityResult> RemoveRoleFromUser(string username, string rolename);
    }
}