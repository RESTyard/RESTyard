namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Constants for type and property names from <c>RESTyard.Schema.Model</c>,
/// <c>RESTyard.AspNetCore.JsonSchema</c>, and <c>Json.Schema</c>.
/// The generator emits source code referencing these types but cannot take a project
/// dependency on them (source generators must minimize dependencies to
/// avoid assembly loading issues in the compiler host).
/// </summary>
internal static class SchemaTypeNames
{
    // RESTyard.Schema.Model
    internal const string SchemaModelNamespace = "RESTyard.Schema.Model";

    internal const string EntityTypeSchema = "EntityTypeSchema";
    internal const string EntityTypeSchema_Name = "Name";
    internal const string EntityTypeSchema_Title = "Title";
    internal const string EntityTypeSchema_Description = "Description";
    internal const string EntityTypeSchema_Classes = "Classes";
    internal const string EntityTypeSchema_AccessGroups = "AccessGroups";
    internal const string EntityTypeSchema_IsDeprecated = "IsDeprecated";
    internal const string EntityTypeSchema_DeprecationMessage = "DeprecationMessage";
    internal const string EntityTypeSchema_PropertiesSchema = "PropertiesSchema";
    internal const string EntityTypeSchema_Links = "Links";

    // LinkDescription
    internal const string LinkDescription = "LinkDescription";
    internal const string LinkDescription_Relations = "Relations";
    internal const string LinkDescription_TargetName = "TargetName";
    internal const string LinkDescription_TargetClasses = "TargetClasses";
    internal const string LinkDescription_MediaType = "MediaType";
    internal const string LinkDescription_Title = "Title";
    internal const string LinkDescription_Description = "Description";
    internal const string LinkDescription_AccessGroups = "AccessGroups";
    internal const string LinkDescription_IsMandatory = "IsMandatory";
    internal const string LinkDescription_IsDeprecated = "IsDeprecated";
    internal const string LinkDescription_DeprecationMessage = "DeprecationMessage";

    // ActionDescription
    internal const string ActionDescription = "ActionDescription";
    internal const string ActionDescription_Name = "Name";
    internal const string ActionDescription_Title = "Title";
    internal const string ActionDescription_Description = "Description";
    internal const string ActionDescription_ContentType = "ContentType";
    internal const string ActionDescription_ParameterSchema = "ParameterSchema";
    internal const string ActionDescription_IsMandatory = "IsMandatory";
    internal const string ActionDescription_IsFileUpload = "IsFileUpload";
    internal const string ActionDescription_IsDeprecated = "IsDeprecated";
    internal const string ActionDescription_DeprecationMessage = "DeprecationMessage";
    internal const string ActionDescription_AccessGroups = "AccessGroups";
    internal const string ActionDescription_ResultName = "ResultName";
    internal const string ActionDescription_ResultClasses = "ResultClasses";
    internal const string EntityTypeSchema_Actions = "Actions";

    // EmbeddedEntityDescription
    internal const string EmbeddedEntityDescription = "EmbeddedEntityDescription";
    internal const string EmbeddedEntityDescription_Relations = "Relations";
    internal const string EmbeddedEntityDescription_TargetName = "TargetName";
    internal const string EmbeddedEntityDescription_TargetClasses = "TargetClasses";
    internal const string EmbeddedEntityDescription_IsCollection = "IsCollection";
    internal const string EmbeddedEntityDescription_AccessGroups = "AccessGroups";
    internal const string EmbeddedEntityDescription_IsMandatory = "IsMandatory";
    internal const string EmbeddedEntityDescription_Title = "Title";
    internal const string EmbeddedEntityDescription_Description = "Description";
    internal const string EmbeddedEntityDescription_IsDeprecated = "IsDeprecated";
    internal const string EmbeddedEntityDescription_DeprecationMessage = "DeprecationMessage";
    internal const string EntityTypeSchema_EmbeddedEntities = "EmbeddedEntities";

    // RESTyard.Schema
    internal const string JsonSchemaFactoryNamespace = "RESTyard.Schema.SchemaGeneration";
    internal const string IJsonSchemaFactory = "IJsonSchemaFactory";

    // Registry
    internal const string HypermediaSchemaRegistryAttributeFullName =
        "RESTyard.Schema.Model.HypermediaSchemaRegistryAttribute";
    internal const string HypermediaSchemaRegistryPrefix = "HypermediaSchemaRegistry_";

    // Json.Schema (JsonSchema.Net)
    internal const string JsonSchemaNamespace = "Json.Schema";
    internal const string JsonSchema = "JsonSchema";

    // System.Text.Json
    internal const string JsonDocumentNamespace = "System.Text.Json";
    internal const string JsonDocument = "JsonDocument";

    // Siren POCO Model (RESTyard.AspNetCore.Hypermedia.Siren.Model)
    internal const string SirenModelNamespace = "RESTyard.AspNetCore.Hypermedia.Siren.Model";
    internal const string SirenEntity = "SirenEntity";
    internal const string SirenLink = "SirenLink";
    internal const string SirenAction = "SirenAction";
    internal const string SirenField = "SirenField";
    internal const string ISirenSubEntity = "ISirenSubEntity";
    internal const string SirenEmbeddedEntity = "SirenEmbeddedEntity";
    internal const string SirenLinkedEntity = "SirenLinkedEntity";
    internal const string NoProperties = "NoProperties";

    // Siren Mapper Options (RESTyard.AspNetCore.Hypermedia.Siren)
    internal const string SirenNamespace = "RESTyard.AspNetCore.Hypermedia.Siren";
    internal const string SirenMapperOptions = "SirenMapperOptions";

    // Route Resolver (RESTyard.AspNetCore.WebApi.RouteResolver)
    internal const string RouteResolverNamespace = "RESTyard.AspNetCore.WebApi.RouteResolver";
    internal const string IHypermediaRouteResolver = "IHypermediaRouteResolver";
    internal const string ResolvedRoute = "ResolvedRoute";

    // Query String Builder (RESTyard.AspNetCore.Query)
    internal const string QueryNamespace = "RESTyard.AspNetCore.Query";
    internal const string IQueryStringBuilder = "IQueryStringBuilder";

    // System.ComponentModel (for EditorBrowsable)
    internal const string EditorBrowsableAttribute = "System.ComponentModel.EditorBrowsable";
    internal const string EditorBrowsableStateNever = "System.ComponentModel.EditorBrowsableState.Never";
}
