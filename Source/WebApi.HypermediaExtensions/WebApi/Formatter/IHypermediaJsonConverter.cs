using System;
using Newtonsoft.Json.Linq;
using RESTyard.WebApi.Extensions.Hypermedia;

namespace RESTyard.WebApi.Extensions.WebApi.Formatter
{
    public interface IHypermediaJsonConverter
    {
        JObject ConvertToJson(HypermediaObject hypermediaObject);
    }
}