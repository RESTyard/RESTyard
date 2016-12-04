namespace WebApiHypermediaExtensionsCore.Query
{
    public interface IQueryStringBuilder
    {
        string CreateQueryString(object sourceObject, string objectPrefix = "");
    }
}