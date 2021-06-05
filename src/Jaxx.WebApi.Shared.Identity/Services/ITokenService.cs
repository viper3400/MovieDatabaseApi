using System;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Identity.Services
{
    public interface ITokenService
    {
        Task<Tuple<string, string>> GenerateRefreshToken(string username);
        DateTime GetSessionStartTime(string uniqueIdentifier);
    }
}