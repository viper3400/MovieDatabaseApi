using Jaxx.WebApi.Shared.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Controllers.Infrastructure
{
    public static class ClaimsHelper
    {
        public static ClaimsPrincipal GetPrincipalFromToken(string token, string issuerSigningKey)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public static UserModel GetUserModelFromToken(string token, string issuerSigningKey)
        {
            var principal = GetPrincipalFromToken(token, issuerSigningKey);
            var username = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var group = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid).Value;
            return new UserModel { Name = username, Groups = new List<string> { group } };
        }
    }
}
