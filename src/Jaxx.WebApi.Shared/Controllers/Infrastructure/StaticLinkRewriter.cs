using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Jaxx.WebApi.Shared.Infrastructure
{
    public sealed class StaticLinkRewriter : ILinkRewriter
    {
        private IUrlHelper _urlHelper;
        private readonly IRewriteConfiguration _configuration;

        public StaticLinkRewriter(IRewriteConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Link Rewrite(RouteLink original)
        {
            if (original == null) return null;
            
            var url =_urlHelper.RouteUrl(original.RouteName, original.RouteValues, _configuration.Protcol, _configuration.RewriteUrl);
            return new Link
            {                
                Href = url,
                Method = original.Method,
                Relations = original.Relations
            };
        }

        public void SetUrlHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }
    }
}
