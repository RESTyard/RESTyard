namespace WebApi.HypermediaExtensions.Exceptions
{
    public class RouteResolverException : HypermediaException
    {
        public RouteResolverException(string description) : base(description)
        {
        }
    }
}