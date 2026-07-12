using System.Collections.Generic;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Fully qualified names of RESTyard (and related) types the generator recognizes in user code.
/// The generator cannot reference these assemblies directly, so all symbol matching is done by name.
/// </summary>
internal static class WellKnownTypeNames
{
    internal const string HypermediaObjectAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaObjectAttribute";

    internal const string IHypermediaObjectFullName =
        "RESTyard.AspNetCore.Hypermedia.IHypermediaObject";

    internal const string FormatterIgnoreAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.FormatterIgnoreHypermediaPropertyAttribute";

    internal const string RelationsAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.RelationsAttribute";

    internal const string HypermediaActionAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaActionAttribute";

    internal const string HypermediaPropertyAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaPropertyAttribute";

    internal const string ILinkFullName =
        "RESTyard.AspNetCore.Hypermedia.ILink<THto>";

    /// <summary>Non-generic <c>ILink</c> — implemented by <c>ExternalLink</c> (no HTO target).</summary>
    internal const string ILinkNonGenericFullName =
        "RESTyard.AspNetCore.Hypermedia.ILink";

    internal const string HypermediaActionBaseFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.HypermediaActionBase";

    internal const string FileUploadHypermediaActionFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.FileUploadHypermediaAction";

    internal const string FileUploadHypermediaActionGenericFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.FileUploadHypermediaAction<TParameter>";

    internal const string IEmbeddedEntityFullName =
        "RESTyard.AspNetCore.Hypermedia.IEmbeddedEntity<THto>";

    internal const string IEmbeddedEntityBaseFullName =
        "RESTyard.AspNetCore.Hypermedia.IEmbeddedEntity";

    internal const string HypermediaAssemblyAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaAssemblyAttribute";

    internal const string HypermediaAccessGroupAttributeFullName =
        "RESTyard.Schema.Model.HypermediaAccessGroupAttribute";

    internal const string HypermediaMediaTypeAttributeFullName =
        "RESTyard.Schema.Model.HypermediaMediaTypeAttribute";

    internal const string HypermediaSchemaNameAttributeFullName =
        "RESTyard.Schema.Model.HypermediaSchemaNameAttribute";

    internal const string ObsoleteAttributeFullName =
        "System.ObsoleteAttribute";

    /// <summary>Metadata name of the generic endpoint attribute for <c>ForAttributeWithMetadataName</c>.</summary>
    internal const string HypermediaActionEndpointAttributeMetadataName =
        "RESTyard.AspNetCore.WebApi.AttributedRoutes.HypermediaActionEndpointAttribute`1";

    /// <summary>Metadata name of the generic object endpoint attribute for <c>ForAttributeWithMetadataName</c>.</summary>
    internal const string HypermediaObjectEndpointAttributeMetadataName =
        "RESTyard.AspNetCore.WebApi.AttributedRoutes.HypermediaObjectEndpointAttribute`1";

    // Legacy attribute support — remove this block when HttpMethodHypermediaAction is removed.
    // If you remove the legacy attribute, also remove
    // ActionResultMappingExtractor.ExtractLegacyActionResults (and its InheritsFrom scan)
    // and the Has201ResponseAttribute check for legacy patterns.
    internal const string HttpMethodHypermediaActionBaseFullName =
        "RESTyard.AspNetCore.WebApi.AttributedRoutes.HttpMethodHypermediaAction";

    internal const string KeyAttributeFullName =
        "RESTyard.AspNetCore.WebApi.RouteResolver.KeyAttribute";

    internal const string TitleAttributeFullName =
        "Json.Schema.Generation.TitleAttribute";

    internal const string DescriptionAttributeFullName =
        "Json.Schema.Generation.DescriptionAttribute";

    internal const string HypermediaActionGenericFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.HypermediaAction<TParameter>";

    /// <summary>
    /// RESTyard-specific attributes that should NOT be forwarded to the generated properties POCO.
    /// These are consumed by the source generator and applied structurally.
    /// </summary>
    internal static readonly HashSet<string> RestyardAttributeFullNames = new()
    {
        HypermediaObjectAttributeFullName,
        FormatterIgnoreAttributeFullName,
        RelationsAttributeFullName,
        HypermediaActionAttributeFullName,
        HypermediaPropertyAttributeFullName,
        KeyAttributeFullName,
    };
}
