using System;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public class RouteInfo
    {
        public string Name { get; }
        public string? HttpMethod { get; }
        public string? AcceptableMediaType { get; }

        public static RouteInfo Empty()
        {
            return EmptyRoute;
        }

        private static readonly RouteInfo EmptyRoute = new RouteInfo(string.Empty, null);

        public RouteInfo(string name, string? httpMethod, string? acceptableMediaType = null)
        {
            this.Name = name;
            this.HttpMethod = httpMethod;
            this.AcceptableMediaType = acceptableMediaType;
        }

    }
}
