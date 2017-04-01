using Newtonsoft.Json.Linq;
using WebApiHypermediaExtensionsCore.Hypermedia;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public interface IHypermediaJsonConverter
    {
        JObject ConvertToJson(HypermediaObject hypermediaObject);
    }
}