namespace RESTyard.Schema;

/// <summary>
/// Media type constants for RESTyard schema responses.
/// </summary>
public static class SchemaMediaTypes
{
    /// <summary>
    /// Media type for the hypermedia API schema endpoint: <c>application/vnd.restyard.hypermedia-schema+json</c>.
    /// </summary>
    public const string HypermediaApiSchema = "application/vnd.restyard.hypermedia-schema+json";

    /// <summary>
    /// Media type for the access groups discovery endpoint: <c>application/vnd.restyard.hypermedia-schema-access-groups+json</c>.
    /// </summary>
    public const string HypermediaSchemaAccessGroups = "application/vnd.restyard.hypermedia-schema-access-groups+json";
}
