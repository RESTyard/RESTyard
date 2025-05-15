using System;
using Newtonsoft.Json.Linq;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public interface IHypermediaJsonConverter
    {
        JObject ConvertToJson(IHypermediaObject hypermediaObject);
    }
}