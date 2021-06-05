using Newtonsoft.Json;

namespace Jaxx.WebApi.Shared.Models
{
    public sealed class RouteLink : Link
    {
        [JsonIgnore]
        public string RouteName { get; set; }

        [JsonIgnore]
        public object RouteValues { get; set; }
    }
}
