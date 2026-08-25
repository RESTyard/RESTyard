namespace RESTyard.Generator.Templates.csharp_base;

public static class VersionExtensions
{
    public static bool HypermediaQueryResultParentExists(this Version version)
        => version <= new Version(5, 0);

    public static bool HypermediaObjectParentExists(this Version version)
        => version < new Version(5, 0);
}