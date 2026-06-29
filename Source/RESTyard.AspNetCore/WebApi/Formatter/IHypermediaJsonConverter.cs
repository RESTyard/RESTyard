using System.Text.Json.Nodes;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public interface IHypermediaJsonConverter
    {
        JsonObject ConvertToJson(IHypermediaObject hypermediaObject);
    }
}
