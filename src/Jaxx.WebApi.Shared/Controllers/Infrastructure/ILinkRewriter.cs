using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Jaxx.WebApi.Shared.Infrastructure
{
    public interface ILinkRewriter
    {
        Link Rewrite(RouteLink original);
        void SetUrlHelper(IUrlHelper urlHelper);
    }
}