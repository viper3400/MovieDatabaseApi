using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Jaxx.WebApi.Shared.Infrastructure
{
    public sealed class LinkRewriter : ILinkRewriter
    {
        private IUrlHelper _urlHelper;
       
        public Link Rewrite(RouteLink original)
        {
            if (original == null) return null;
            return new Link
            {
                Href = _urlHelper.Link(original.RouteName, original.RouteValues),                
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
