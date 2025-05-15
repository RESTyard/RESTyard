using System;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public class RouteInfo
    {
        public string Name { get; }
        public HttpMethod HttpMethod { get; }
        public string? AcceptableMediaType { get; }

        public static RouteInfo Empty()
        {
            return EmptyRoute;
        }

        private static readonly RouteInfo EmptyRoute = new RouteInfo(string.Empty, HttpMethod.Undefined);

        public RouteInfo(string name, HttpMethod httpMethod, string? acceptableMediaType = null)
        {
            this.Name = name;
            this.HttpMethod = httpMethod;
            this.AcceptableMediaType = acceptableMediaType;
        }

    }
}
