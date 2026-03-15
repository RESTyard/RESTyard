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
    internal const string EntityTypeSchema_Classes = "Classes";
    internal const string EntityTypeSchema_PropertiesSchema = "PropertiesSchema";
    internal const string EntityTypeSchema_Links = "Links";

    // LinkDescription
    internal const string LinkDescription = "LinkDescription";
    internal const string LinkDescription_Relations = "Relations";
    internal const string LinkDescription_TargetName = "TargetName";
    internal const string LinkDescription_TargetClasses = "TargetClasses";
    internal const string LinkDescription_IsMandatory = "IsMandatory";

    // ActionDescription
    internal const string ActionDescription = "ActionDescription";
    internal const string ActionDescription_Name = "Name";
    internal const string ActionDescription_Title = "Title";
    internal const string ActionDescription_ContentType = "ContentType";
    internal const string ActionDescription_ParameterSchema = "ParameterSchema";
    internal const string ActionDescription_IsMandatory = "IsMandatory";
    internal const string ActionDescription_IsFileUpload = "IsFileUpload";
    internal const string EntityTypeSchema_Actions = "Actions";

    // RESTyard.Schema
    internal const string JsonSchemaFactoryNamespace = "RESTyard.Schema";
    internal const string IJsonSchemaFactory = "IJsonSchemaFactory";
    internal const string SchemaHelper = "SchemaHelper";

    // Json.Schema (JsonSchema.Net)
    internal const string JsonSchemaNamespace = "Json.Schema";
    internal const string JsonSchema = "JsonSchema";

    // System.Text.Json
    internal const string JsonDocumentNamespace = "System.Text.Json";
    internal const string JsonDocument = "JsonDocument";
}
