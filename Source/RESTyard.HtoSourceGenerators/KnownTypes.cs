using RESTyard.HtoSourceGenerators.Attributes;

namespace RESTyard.HtoSourceGenerators;

public static class KnownTypes
{
    public static string HypermediaObjectTypeName = typeof(HypermediaObjectAttribute).FullName!;
    ///public const string HypermediaActionBaseTypeName = "RESTyard.AspNetCore.Hypermedia.Actions.HypermediaActionBase";
    public static string IgnorePropertyAttributeTypeName = typeof(IgnoreHypermediaPropertyAttribute).FullName!;
    public static string HypermediaPropertyAttributeTypeName = typeof(HypermediaPropertyAttribute).FullName!;
}