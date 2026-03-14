namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Constants for type and property names from <c>RESTyard.Schema.Model</c>.
/// The generator emits source code referencing these types but cannot take a project
/// dependency on RESTyard.Schema (source generators must minimize dependencies to
/// avoid assembly loading issues in the compiler host).
/// </summary>
internal static class SchemaTypeNames
{
    internal const string Namespace = "RESTyard.Schema.Model";

    internal const string EntityTypeSchema = "EntityTypeSchema";
    internal const string EntityTypeSchema_Name = "Name";
    internal const string EntityTypeSchema_Title = "Title";
    internal const string EntityTypeSchema_Classes = "Classes";
}
