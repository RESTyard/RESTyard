namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public class RouteInfo
    {
        public readonly string Name;
        public readonly HttpMethod HttpMethod;

        public static RouteInfo Empty()
        {
            return EmptyRoute;
        }

        private static readonly RouteInfo EmptyRoute = new RouteInfo(string.Empty, HttpMethod.Undefined);

        public RouteInfo(string name, HttpMethod httpMethod)
        {
            this.Name = name;
            this.HttpMethod = httpMethod;
        }

    }
}
