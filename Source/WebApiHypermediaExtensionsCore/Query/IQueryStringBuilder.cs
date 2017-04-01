namespace WebApiHypermediaExtensionsCore.Query
{
    public interface IQueryStringBuilder
    {
        /// <summary>
        /// Serializes a object to a query string with leading '?'.
        /// </summary>
        /// <param name="sourceObject">The object to serialize, can be null.</param>
        /// <param name="objectPrefix">Prefix prepended to all serialized object properties.</param>
        /// <returns></returns>
        string CreateQueryString(object sourceObject, string objectPrefix = "");
    }
}