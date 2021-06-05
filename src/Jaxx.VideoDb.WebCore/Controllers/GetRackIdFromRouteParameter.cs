using Microsoft.AspNetCore.Mvc;

namespace Jaxx.VideoDb.WebCore.Controllers
{
    public class GetRackIdFromRouteParameter
    {
        [FromRoute]
        public string RackId { get; set; }
    }
}
