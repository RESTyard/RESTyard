using System;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    public class RouteInfo
    {
        public readonly string Name;
        public readonly HttpMethod HttpMethod;
        public readonly string AcceptableMediaType;

        public static RouteInfo Empty()
        {
            return EmptyRoute;
        }

        private static readonly RouteInfo EmptyRoute = new RouteInfo(string.Empty, HttpMethod.Undefined);

        public RouteInfo(string name, HttpMethod httpMethod, string acceptableMediaType = null)
        {
            this.Name = name;
            this.HttpMethod = httpMethod;
            this.AcceptableMediaType = acceptableMediaType;
        }

    }
}
