namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    public class ResolvedRoute
    {
        public string Url { get; }

        public HttpMethod HttpMethod { get; }

        public ResolvedRoute(string url, HttpMethod httpMethod)
        {
            Url = url;

            HttpMethod = httpMethod;
        }
    }
}
