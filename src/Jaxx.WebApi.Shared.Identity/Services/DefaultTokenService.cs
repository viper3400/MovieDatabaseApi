using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Identity.Services
{
    public class DefaultTokenService : ITokenService
    {
        private readonly ILogger logger;
        private readonly UserManager<IdentityUser> userManager;
        public DefaultTokenService(UserManager<IdentityUser> userManager, ILogger<DefaultTokenService> logger)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        public async Task<Tuple<string, string>> GenerateRefreshToken(string username)
        {
            var identityUser = await userManager.FindByNameAsync(username);
            var sessionIdentifier = Guid.NewGuid().ToString();
            var timeIdentifier = DateTime.Now.Ticks.ToString();
            var uniqueIdentifier = string.Join("|", "RefreshToken", sessionIdentifier, timeIdentifier);
            var refreshToken = await userManager.GenerateUserTokenAsync(identityUser, "HomeWebVideoDB", uniqueIdentifier);
            await userManager.SetAuthenticationTokenAsync(identityUser, "HomeWebVideoDB", uniqueIdentifier, refreshToken);
            return new Tuple<string, string>(uniqueIdentifier, refreshToken);
        }

        public DateTime GetSessionStartTime(string uniqueIdentifier)
        {
            var sessionInfo = uniqueIdentifier.Split("|");
            var sessionStartTime = Convert.ToDateTime(sessionInfo[2]);
            return sessionStartTime;
        }

    }
}
