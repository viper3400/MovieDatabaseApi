using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jaxx.WebApi.Shared.Controllers
{
    [Route("/")]
    public class RootController : Controller
    {
        [HttpGet(Name = nameof(GetRoot))]
        [Authorize]
        public IActionResult GetRoot()
        {
            var response = new RootResource
            {
                Self = Link.To(nameof(GetRoot)),
                //MovieData = Link.ToCollection(nameof(MovieDataController.GetMovieDataAsync))
            };

            return Ok(response);
        }
    }
}
