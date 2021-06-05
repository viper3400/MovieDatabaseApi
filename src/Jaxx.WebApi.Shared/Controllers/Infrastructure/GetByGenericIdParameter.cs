using Microsoft.AspNetCore.Mvc;

namespace Jaxx.WebApi.Shared.Controllers.Infrastructure
{
    public class GetByGenericIdParameter
    {
        [FromRoute]
        public int Id { get; set; }
    }
}
