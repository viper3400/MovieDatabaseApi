using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jaxx.WebApi.Shared.Identity.Services;
using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace Jaxx.WebApi.Shared.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    [OpenApiTag("Identity", Description = "ApiController to handle user identities.")]
    public class IdentityController : ControllerBase
    {
        private IIdentityService identityService;
        private ILogger<IdentityController> logger;
        private readonly UserManager<IdentityUser> userManager;
        public IdentityController(ILogger<IdentityController> logger, IIdentityService identityService, UserManager<IdentityUser> userManager)
        {
            this.identityService = identityService;
            this.logger = logger;
            this.userManager = userManager;
        }

        /// <summary>
        /// Adds a new user.
        /// </summary>
        /// <param name="userModel"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("user", Name = nameof(AddUser))]
        [Authorize(Policy = "Administrator")]
        [SwaggerResponse(200, typeof(Microsoft.AspNetCore.Identity.IdentityResult), Description = "If succeeded is false, erros will contain code and description of error.")]
        [OpenApiOperation("Add new user", "This api adds new user to user repository.")]
        public async Task<IActionResult> AddUser([FromBody] UserModel userModel, CancellationToken ct)
        {
            var x = await identityService.AddUser(new Microsoft.AspNetCore.Identity.IdentityUser
            {
                UserName = userModel.Name

            }, userModel.Password, ct);
            return Ok(x);
        }

        [HttpGet("user", Name = nameof(GetUsers))]
        [Authorize(Policy = "Administrator")]
        [OpenApiOperation("List all users", "This api adds new user to user repository.")]
        [SwaggerResponse(200, typeof(List<Microsoft.AspNetCore.Identity.IdentityUser>), Description = "List all users.")]
        public async Task<IActionResult> GetUsers()
        {
            var x = await identityService.GetUsers();
            return Ok(x);
        }

        [HttpDelete("user/{userId}", Name = nameof(DeleteUser))]
        [Authorize(Policy = "Administrator")]
        [OpenApiOperation("Delete user.", "Delete a user with given Id.")]
        [SwaggerResponse(200, typeof(List<Microsoft.AspNetCore.Identity.IdentityUser>), Description = "If succeeded is false, errosr will contain code and description of error, e.g. if a user with the given id dont exists.")]
        public async Task<IActionResult> DeleteUser(
            [FromRoute] string userId)
        {
            var result = await identityService.DeleteUser(userId);
            return (Ok(result));
        }

        [HttpPost("user/role", Name = nameof(AddUserToRole))]
        [Authorize(Policy = "Administrator")]
        [SwaggerResponse(200, typeof(Microsoft.AspNetCore.Identity.IdentityResult), Description = "If succeeded is false, erros will contain code and description of error.")]
        [OpenApiOperation("Add user to role", "This api adds a role to a user repository.")]
        public async Task<IActionResult> AddUserToRole([FromBody] UserModel userModel, CancellationToken ct)
        {
            var result = await identityService.AddUserToRole(userModel.Name, userModel.Groups.FirstOrDefault());
            return Ok(result);
        }

        [HttpGet("user/role/{username}", Name = nameof(GetRolesForUser))]
        [SwaggerResponse(200, typeof(List<string>), Description = "Returns a list of all roles for this user.")]
        [OpenApiOperation("Get Roles for use", "Returns all roles for the given user.")]
        public async Task<IActionResult> GetRolesForUser([FromRoute] string username, CancellationToken ct)
        {
            var result = await identityService.GetRolesForUser(username);
            return Ok(result);
        }

        [HttpDelete("user/role/{username}/{rolename}", Name = nameof(RemoveRoleFromUser))]
        [Authorize(Policy = "Administrator")]
        [OpenApiOperation("Remove role from user.", "Delete a role from user.")]
        [SwaggerResponse(200, typeof(List<Microsoft.AspNetCore.Identity.IdentityUser>), Description = "If succeeded is false, errosr will contain code and description of error, e.g. if a user with the given id dont exists.")]
        public async Task<IActionResult> RemoveRoleFromUser(
    [FromRoute] string username, [FromRoute] string rolename)
        {
            var result = await identityService.RemoveRoleFromUser(username, rolename);
            return (Ok(result));
        }

        /// <summary>
        /// Reset the password of a user.
        /// </summary>
        /// <param name="userModel"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut("password/reset", Name = nameof(ResetPassword))]
        [Authorize(Policy = "Administrator")]
        [SwaggerResponse(200, typeof(Microsoft.AspNetCore.Identity.IdentityResult), Description = "If succeeded is false, erros will contain code and description of error.")]
        [OpenApiOperation("Reset the password of a user", "This api resets the password of a given user with a given new password.")]
        public async Task<IActionResult> ResetPassword(
      [FromBody] UserModel userModel,
      CancellationToken ct)
        {
            var identityUser = await userManager.FindByNameAsync(userModel.Name);
            if (identityUser == null) return NotFound(userModel.Name);

            string resetToken = await userManager.GeneratePasswordResetTokenAsync(identityUser);
            var passwordChangeResult = await userManager.ResetPasswordAsync(identityUser, resetToken, userModel.Password);

            return Ok(passwordChangeResult);
        }

        /// <summary>
        /// Change the password of a user.
        /// </summary>
        /// <param name="userModel"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPut("password/change", Name = nameof(ChangePassword))]
        [SwaggerResponse(200, typeof(Microsoft.AspNetCore.Identity.IdentityResult), Description = "If succeeded is false, erros will contain code and description of error.")]
        [OpenApiOperation("Changes the password of a user", "This api changes the password of a given user with a given new password. Old password is required.")]
        public async Task<IActionResult> ChangePassword(
      [FromBody] UserPasswordChangeModel passwordChangeModel,
      CancellationToken ct)
        {
            var identityUser = await userManager.FindByNameAsync(passwordChangeModel.UserName);
            if (identityUser == null) return NotFound(passwordChangeModel.UserName);

            var result = await identityService.ChangePassword(passwordChangeModel.UserName, passwordChangeModel.CurrentPassword, passwordChangeModel.NewPassword);

            return Ok(result);
        }

        [HttpPost("role", Name = nameof(AddRole))]
        [Authorize(Policy = "Administrator")]
        [OpenApiOperation("Add role.", "Adds a new role to repository")]
        [SwaggerResponse(200, typeof(List<Microsoft.AspNetCore.Identity.IdentityResult>), Description = "-")]
        public async Task<IActionResult> AddRole(
            [FromBody] RoleModel roleModel)
        {
            var result = await identityService.AddRole(roleModel.RoleName);
            return Ok(result);
        }

        [HttpGet("role", Name = nameof(GetRoles))]
        [OpenApiOperation("Get roles.", "Gets all roles from repository")]
        [SwaggerResponse(200, typeof(List<Microsoft.AspNetCore.Identity.IdentityRole>), Description = "")]
        public async Task<IActionResult> GetRoles(CancellationToken ct)
        {
            var result = await identityService.GetRoles(ct);
            return Ok(result);
        }

        [HttpDelete("role/{roleId}", Name = nameof(DeleteRole))]
        [Authorize(Policy = "Administrator")]
        [OpenApiOperation("Delete role.", "Delete a role with given Id.")]
        [SwaggerResponse(200, typeof(List<Microsoft.AspNetCore.Identity.IdentityResult>), Description = "If succeeded is false, error will contain code and description of error, e.g. if a role with the given id dont exists.")]
        public async Task<IActionResult> DeleteRole(
            [FromRoute] string roleId)
        {
            var result = await identityService.DeleteRole(roleId);
            return (Ok(result));
        }
    }
}