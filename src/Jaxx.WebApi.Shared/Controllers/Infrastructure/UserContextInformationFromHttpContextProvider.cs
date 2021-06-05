using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Controllers.Infrastructure
{
    public class UserContextInformationFromHttpContextProvider : IUserContextInformationProvider
    {
        IHttpContextAccessor _httpContext;

        public UserContextInformationFromHttpContextProvider(IHttpContextAccessor context)
        {
            _httpContext = context;
        }

        public string UserName
        {
            get
            {
                return GetUserNameFromHttpContext();
            }
        }

        private string GetUserNameFromHttpContext()
        {
            try

            {
                var currentUser = _httpContext.HttpContext.User;

                if (currentUser.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                {
                    return currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                }
            }
            catch (Exception)
            {

            }

            // if we are here, something went wrong
            throw new ArgumentNullException("UserName", "UserName not found in JWT Token");
        }

        public string GetViewGroup()
        {
            try

            {
                var currentUser = _httpContext.HttpContext.User;

                if (currentUser.HasClaim(c => c.Type == ClaimTypes.GroupSid))
                {
                    return currentUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GroupSid).Value;
                }
            }
            catch (Exception)
            {

            }
            
            // if we are here, something went wrong
            throw new ArgumentNullException("ViewGroup", "ViewGroup not found in JWT Token");
        }
    }
}
