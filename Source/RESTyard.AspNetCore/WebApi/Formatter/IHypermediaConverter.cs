using System;
using RESTyard.AspNetCore.Hypermedia;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public interface IHypermediaConverter
    {
        string ConvertToString(IHypermediaObject hypermediaObject);
    }
}