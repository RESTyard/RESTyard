namespace WebApi.HypermediaExtensions.Exceptions
{
    public class RouteRegisterException : HypermediaException
    {
        public RouteRegisterException(string description) : base(description)
        {
        }
    }
}