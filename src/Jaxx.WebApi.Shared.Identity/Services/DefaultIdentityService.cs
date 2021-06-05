using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Identity.Services
{
    public class DefaultIdentityService : IIdentityService
    {
        private ILogger<DefaultIdentityService> logger;
        private IdentityDbContext context;
        private UserManager<IdentityUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        public DefaultIdentityService(IdentityDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<DefaultIdentityService> logger)
        {
            this.logger = logger;
            this.context = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async void InitIdentity(string password, CancellationToken ct)
        {
            var adminName = "Administrator";
            var roles = await GetRoles(ct);
            var existsAdminRole = roles.Any(r => r.Name == adminName);
            if (!existsAdminRole)
            {
                logger.LogInformation($"Creating role {adminName}");
                await AddRole(adminName);
            }

            var users = await GetUsers();
            var existsAdminUser = users.Any(u => u.UserName == adminName);
            if (!existsAdminUser)
            {
                logger.LogInformation($"Creating user {adminName}");
                var result = await AddUser(new IdentityUser { UserName = adminName }, password, ct);
                if (result.Errors.Count() > 0)
                {
                    foreach (var e in result.Errors)
                    {
                        logger.LogError($"{e.Code}: {e.Description}");
                    }
                    throw new ArgumentException(result.Errors.FirstOrDefault().Description);
                }
            }

            var adminUserRoles = await GetRolesForUser(adminName);
            var isAdminUserinAdminRole = adminUserRoles.Any(r =>  r == adminName);
            if (!isAdminUserinAdminRole)
            {
                logger.LogInformation($"Add user {adminName} to role {adminName}");
                await AddUserToRole(adminName, adminName);
            }
        }

        public async Task<IdentityResult> AddUser(IdentityUser user, string password, CancellationToken ct)
        {
            logger.LogDebug("Create new user: {0}", user.UserName);
            var result = await userManager.CreateAsync(user, password);
            return result;
        }

        public async Task<List<IdentityUser>> GetUsers()
        {
            var task = new Task<List<IdentityUser>>(() => userManager.Users.ToList());
            task.Start();
            return await task;
        }
        public async Task<IdentityResult> ChangePassword(string user, string currentPassword, string newPassword)
        {
            logger.LogDebug("Try to changed password for user: {0}.", user);
            var identityUser = await userManager.FindByNameAsync(user);

            // raise exception when no user was found.
            if (identityUser == null) new KeyNotFoundException($"Change password: No user found with name {user}");

            var result = await userManager.ChangePasswordAsync(identityUser, currentPassword, newPassword);
            logger.LogDebug("Changed password for user: {0}.", user);
            return result;
        }

        /// <summary>
        /// Delletes the user with the given Id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IdentityResult> DeleteUser(string userId)
        {
            logger.LogDebug("Try to delete user with id {0}", userId);
            var identityResult = new IdentityResult();
            var identityUser = await userManager.FindByIdAsync(userId);
            if (identityUser != null)
            {
                identityResult = await userManager.DeleteAsync(identityUser);
            }
            else
            {
                logger.LogError("{0}: No user found for id {1}^.", nameof(DeleteUser), userId);
                identityResult = IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"No user found for id {userId}" });
            }
            return identityResult;
        }

        /// <summary>
        /// Adds a role with the given name to repository.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddRole(string role)
        {
            logger.LogDebug("{0}: Try to add role {1}", nameof(AddRole), role);
            var IdentityRole = new IdentityRole { Name = role };
            var result = await roleManager.CreateAsync(IdentityRole);
            return result;
        }

        /// <summary>
        /// Gets all roles from repository.
        /// </summary>
        /// <returns></returns>
        public async Task<List<IdentityRole>> GetRoles(CancellationToken ct)
        {
            logger.LogDebug("{0}", nameof(GetRoles));
            var task = new Task<List<IdentityRole>>(() => roleManager.Roles.ToList(), ct);
            task.Start();
            return await task;
        }

        /// <summary>
        /// Delletes the tole with the given Id.
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<IdentityResult> DeleteRole(string roleId)
        {
            logger.LogDebug("Try to delete role with id {0}", roleId);
            var identityResult = new IdentityResult();
            var identityRole = await roleManager.FindByIdAsync(roleId);
            if (identityRole != null)
            {
                identityResult = await roleManager.DeleteAsync(identityRole);
            }
            else
            {
                logger.LogError("{0}: No role found for id {1}.", nameof(DeleteUser), roleId);
                identityResult = IdentityResult.Failed(new IdentityError { Code = "RoleNotFound", Description = $"No role found for id {roleId}" });
            }
            return identityResult;
        }

        /// <summary>
        /// Checks the password for the given user.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<IdentityResult> CheckPassword(string name, string password)
        {
            logger.LogDebug("Try to authenticate user with id {0}", name);
            var identityResult = new IdentityResult();
            var identityUser = await userManager.FindByNameAsync(name);
            if (identityUser != null)
            {
                var x = await userManager.GetLoginsAsync(identityUser);
                var isPasswordMatch = await userManager.CheckPasswordAsync(identityUser, password);
                if (isPasswordMatch) identityResult = IdentityResult.Success;
            }
            else
            {
                logger.LogError("{0}: No user found for name {1}.", nameof(CheckPassword), name);
                identityResult = IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = $"No user found for name {name}" });
            }
            return identityResult; ;
        }
        /// <summary>
        /// Adds the a role to a user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddUserToRole(string username, string rolename)
        {
            var identityUser = await userManager.FindByNameAsync(username);
            return await userManager.AddToRoleAsync(identityUser, rolename);
        }

        /// <summary>
        /// Get all roles for a given user.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<IList<string>> GetRolesForUser(string username)
        {
            IList<string> roles;
            var identityUser = await userManager.FindByNameAsync(username);
            if (identityUser != null)
            {
                roles = await userManager.GetRolesAsync(identityUser);
            }
            else roles = new List<string>();

            return roles;
        }

        /// <summary>
        /// Get all user for a given role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<IList<IdentityUser>> GetUsersForRole(string role)
        {
            var dbRole = await roleManager.FindByNameAsync(role);
            var dbUsersInRole = context.UserRoles.Where(r => r.RoleId == dbRole.Id).Select(u => u.UserId);
            var users = new List<IdentityUser>();
            foreach (var dbUser in dbUsersInRole)
            {
                var user = await userManager.FindByIdAsync(dbUser);
                users.Add(user);
            }

            return users;
        }

        /// <summary>
        /// Remove the given role from user.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public async Task<IdentityResult> RemoveRoleFromUser(string username, string rolename)
        {
            var identityUser = await userManager.FindByNameAsync(username);
            return await userManager.RemoveFromRoleAsync(identityUser, rolename);
        }
    }
}
