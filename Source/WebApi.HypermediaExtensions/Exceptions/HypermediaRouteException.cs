namespace WebApi.HypermediaExtensions.Exceptions
{
    public class HypermediaRouteException : HypermediaException
    {
        public HypermediaRouteException(string description) : base(description)
        {
        }
    }
}