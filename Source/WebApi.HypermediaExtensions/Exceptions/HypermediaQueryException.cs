namespace WebApi.HypermediaExtensions.Exceptions
{
    public class HypermediaQueryException : HypermediaException
    {
        public HypermediaQueryException(string description) : base(description)
        {
        }
    }
}