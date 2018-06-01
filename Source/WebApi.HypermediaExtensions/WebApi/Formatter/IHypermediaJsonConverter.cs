using Newtonsoft.Json.Linq;
using WebApi.HypermediaExtensions.Hypermedia;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    public interface IHypermediaJsonConverter
    {
        JObject ConvertToJson(HypermediaObject hypermediaObject);
    }
}