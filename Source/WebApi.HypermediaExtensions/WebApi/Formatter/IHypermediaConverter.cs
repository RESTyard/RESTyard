using System;
using RESTyard.WebApi.Extensions.Hypermedia;

namespace RESTyard.WebApi.Extensions.WebApi.Formatter
{
    public interface IHypermediaConverter
    {
        string ConvertToString(HypermediaObject hypermediaObject);
    }
}