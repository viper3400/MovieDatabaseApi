using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Jaxx.WebApi.Shared.Identity.Services;
using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NSwag.Annotations;

namespace Jaxx.WebApi.Shared.Controllers
{

    [Route("/[controller]")]
    public class TokenController : Controller
    {
        private IConfiguration _config;
        private IIdentityService identityService;
        private ILogger<IdentityController> logger;
        private IUserContextInformationProvider userContextProvider;
        private readonly ITokenService tokenService;
        private readonly UserManager<IdentityUser> userManager;
        public TokenController(IConfiguration config, IIdentityService identityService, ILogger<IdentityController> logger, IUserContextInformationProvider userContextProvider, UserManager<IdentityUser> userManager, ITokenService tokenService)
        {
            _config = config;
            this.identityService = identityService;
            this.logger = logger;
            this.userContextProvider = userContextProvider;
            this.userManager = userManager;
            this.tokenService = tokenService;
        }

        [AllowAnonymous]
        [OpenApiOperation("Request new bearer token.", "This api returns a bearer token for given user information. If no password is given or password is emtpy, the apimasterkey will be used to authenticate the user.")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]LoginModel loginModel)
        {
            // https://auth0.com/blog/securing-asp-dot-net-core-2-applications-with-jwts/
            // https://piotrgankiewicz.com/2017/12/07/jwt-refresh-tokens-and-net-core/
            // https://www.blinkingcaret.com/2018/05/30/refresh-tokens-in-asp-net-core-web-api/

            var userAuthentificationMode = GetUserAuthentificationMode(loginModel);
            var token = "";
            Tuple<string,string> refreshTuple = null;

            var userModel = new UserModel { Name = loginModel.Username, Groups = new List<string> { loginModel.Group } };

            switch (userAuthentificationMode)
            {
                case UserAuthentificationMode.ApiMasterkeyAuthentification:
                    if (IsValidApiMasterkey(loginModel))
                    {
                        token = GenerateToken(userModel);
                    }
                    break;
                case UserAuthentificationMode.PasswordAuthentification:
                    if (await IsValidUserAndPasswordCombinationAsync(loginModel))
                    {
                        token = GenerateToken(userModel);
                        refreshTuple = await tokenService.GenerateRefreshToken(loginModel.Username);
                    }
                    break;
            }
            if (!string.IsNullOrWhiteSpace(token))
            {
                return Ok(new { token, refreshTuple });
            }
            else return BadRequest();
        }


        [AllowAnonymous]
        [OpenApiOperation("Refresh a bearer token.", "This api refreshs a token. The token may be expired. A valid refresh token must be provided.")]
        [HttpPost("refresh", Name = nameof(Refresh))]
        public async Task<IActionResult> Refresh([FromBody]RefreshTokenModel refreshTokenModel)
        {
            var principal = Infrastructure.ClaimsHelper.GetPrincipalFromToken(refreshTokenModel.Token, _config["Jwt:Key"]);;
            var username = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var identityUser = await userManager.FindByNameAsync(username);
            var savedRefreshToken = await userManager.GetAuthenticationTokenAsync(identityUser, "HomeWebVideoDB", refreshTokenModel.RefreshSessionIdentifier); //retrieve the refresh token from a data store
            if (savedRefreshToken != refreshTokenModel.RefreshToken)
            {
                logger.LogWarning("Invalid refresh token for user: {0}", username);
                return BadRequest($"Invalid refresh token for user: {username}");
            }
            else
            {
                var result = await userManager.RemoveAuthenticationTokenAsync(identityUser, "HomeWebVideoDB", refreshTokenModel.RefreshSessionIdentifier);
                logger.LogDebug($"Removed old refreshToken : {result.Succeeded}");
            }

            var newJwtToken = GenerateToken(principal.Claims);
            var newRefreshToken = await tokenService.GenerateRefreshToken(username);

            logger.LogDebug("Refreshed token for user {0}", username);

            return new ObjectResult(new
            {
                token = newJwtToken,
                refreshTuple = newRefreshToken
            });
        }

        [Authorize]
        [HttpPost("signout", Name = nameof(UserSignOut))]
        public async Task<IActionResult> UserSignOut()
        {
            logger.LogDebug("{0} | SignOutUser {1}", nameof(UserSignOut), userContextProvider.UserName);
            var task = new Task(() => HttpContext.Session.Clear());
            task.Start();
            await task;
            return Ok();
        }

        [Authorize]
        [HttpGet ("profile", Name = nameof(GetUserProfile))]
        public IActionResult GetUserProfile()
        {
            var userModel = new UserModel { Name = userContextProvider.UserName, Groups = new List<string> { userContextProvider.GetViewGroup()}};
            var json = JsonConvert.SerializeObject(userModel);
            return Ok(json);
        }
        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              claims: claims,
              expires: DateTime.Now.AddMinutes(30),
              signingCredentials: creds);

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.InboundClaimTypeMap.Clear();
            return tokenHandler.WriteToken(token);
        }

        private string GenerateToken(UserModel userModel)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userModel.Name),
                new Claim(ClaimTypes.GroupSid, userModel.Groups.FirstOrDefault()),
            };


            var identity = userManager.FindByNameAsync(userModel.Name).Result;
            var roles = userManager.GetRolesAsync(identity).Result;

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return GenerateToken(claims);
        }

        private UserAuthentificationMode GetUserAuthentificationMode(LoginModel loginModel)
        {
            // if no password is provided we are in legacy mode (using master key authorization instead
            // of user & password authorization
            if (string.IsNullOrWhiteSpace(loginModel.Password) && (!string.IsNullOrWhiteSpace(loginModel.ApiMasterkey)))
            {
                logger.LogDebug("{0} | Authorization mode is ApiMasterkey, user is {1}", nameof(GetUserAuthentificationMode), loginModel.Username);
                return UserAuthentificationMode.ApiMasterkeyAuthentification;
            }
            else
            {
                logger.LogDebug("{0} | Authorization mode is user & password, user is {1}", nameof(GetUserAuthentificationMode), loginModel.Username);
                return UserAuthentificationMode.PasswordAuthentification;
            }
        }
        private async Task<bool> IsValidUserAndPasswordCombinationAsync(LoginModel loginModel)
        {
            logger.LogDebug("{0} | Authorization mode is user & password, user is {1}", nameof(IsValidUserAndPasswordCombinationAsync), loginModel.Username);
            var isUserValid = await identityService.CheckPassword(loginModel.Username, loginModel.Password);
            return isUserValid.Succeeded;
        }
        private bool IsValidApiMasterkey(LoginModel loginModel)
        {
            if (loginModel.ApiMasterkey == _config["Jwt:ApiMasterkey"])
            {
                logger.LogDebug("{0} | Authorization mode is ApiMasterkey, user is {1}", nameof(IsValidApiMasterkey), loginModel.Username);
                return true;
            }
            else return false;
        }
        private enum UserAuthentificationMode { PasswordAuthentification, ApiMasterkeyAuthentification }
    }
}