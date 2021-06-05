using Newtonsoft.Json;

namespace Jaxx.WebApi.Shared.Models
{
    public abstract class Resource : Link
    {
        [JsonIgnore]
        public Link Self { get; set; }
    }
}
