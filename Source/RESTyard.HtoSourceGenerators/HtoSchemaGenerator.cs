using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Incremental source generator that analyzes HTO (HypermediaTypedObject) classes
/// and emits <c>GetSchema()</c> methods producing <c>EntityTypeSchema</c> instances,
/// as well as per-assembly schema registries.
/// </summary>
[Generator]
public class HtoSchemaGenerator : IIncrementalGenerator
{
    private const string HypermediaObjectAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaObjectAttribute";

    private const string IHypermediaObjectFullName =
        "RESTyard.AspNetCore.Hypermedia.IHypermediaObject";

    private const string FormatterIgnoreAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.FormatterIgnoreHypermediaPropertyAttribute";

    private const string RelationsAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.RelationsAttribute";

    private const string HypermediaActionAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaActionAttribute";

    private const string HypermediaPropertyAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaPropertyAttribute";

    private const string ILinkFullName =
        "RESTyard.AspNetCore.Hypermedia.ILink<THto>";

    private const string HypermediaActionBaseFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.HypermediaActionBase";

    private const string FileUploadHypermediaActionFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.FileUploadHypermediaAction";

    private const string FileUploadHypermediaActionGenericFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.FileUploadHypermediaAction<TParameter>";

    private const string IEmbeddedEntityFullName =
        "RESTyard.AspNetCore.Hypermedia.IEmbeddedEntity<THto>";

    private const string IEmbeddedEntityBaseFullName =
        "RESTyard.AspNetCore.Hypermedia.IEmbeddedEntity";

    private const string HypermediaAssemblyAttributeFullName =
        "RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaAssemblyAttribute";

    private const string HypermediaAccessGroupAttributeFullName =
        "RESTyard.Schema.Model.HypermediaAccessGroupAttribute";

    private const string ObsoleteAttributeFullName =
        "System.ObsoleteAttribute";

    private const string HypermediaActionEndpointAttributePrefix =
        "RESTyard.AspNetCore.WebApi.AttributedRoutes.HypermediaActionEndpointAttribute<";

    // Legacy attribute support — remove this block when HttpMethodHypermediaAction is removed.
    // If you remove the legacy attribute, also remove the InheritsFrom scan in ExtractActionResultMappings
    // and the Has201ResponseAttribute check for legacy patterns.
    private const string HttpMethodHypermediaActionBaseFullName =
        "RESTyard.AspNetCore.WebApi.AttributedRoutes.HttpMethodHypermediaAction";

    private const string KeyAttributeFullName =
        "RESTyard.AspNetCore.WebApi.RouteResolver.KeyAttribute";

    private const string TitleAttributeFullName =
        "Json.Schema.Generation.TitleAttribute";

    private const string DescriptionAttributeFullName =
        "Json.Schema.Generation.DescriptionAttribute";

    /// <summary>
    /// RESTyard-specific attributes that should NOT be forwarded to the generated properties POCO.
    /// These are consumed by the source generator and applied structurally.
    /// </summary>
    private static readonly HashSet<string> RestyardAttributeFullNames = new()
    {
        HypermediaObjectAttributeFullName,
        FormatterIgnoreAttributeFullName,
        RelationsAttributeFullName,
        HypermediaActionAttributeFullName,
        HypermediaPropertyAttributeFullName,
        KeyAttributeFullName,
    };

    private const string HypermediaActionGenericFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.HypermediaAction<TParameter>";

    private static readonly DiagnosticDescriptor SirenRequiresSchema = new(
        id: "RY0030",
        title: "Siren = true requires Schema generation",
        messageFormat: "[HypermediaAssembly] has Siren = true but Schema = false — Schema has been forced to true because Siren mappers depend on the generated properties POCOs",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingResultTypeWith201 = new(
        id: "RY0031",
        title: "Action endpoint returns 201 but has no ResultType",
        messageFormat: "Action endpoint '{0}.{1}' on '{2}' has a 201 response annotation but no ResultType — consider adding ResultType to declare the result entity for schema generation.",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ResultTypeNotHypermediaObject = new(
        id: "RY0032",
        title: "ResultType is not a HypermediaObject",
        messageFormat: "ResultType '{0}' on action endpoint for '{1}.{2}' is not decorated with [HypermediaObject] — schema cannot describe the result entity. If this action returns a non-hypermedia resource, consider removing ResultType or suppress this warning.",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor EmbeddedEntityMissingRelations = new(
        id: "RY0020",
        title: "Embedded entity property missing [Relations] attribute",
        messageFormat: "Property '{0}' on '{1}' is of type IEmbeddedEntity but has no [Relations] attribute — it will be ignored in the schema",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor LinkMissingRelations = new(
        id: "RY0021",
        title: "Link property missing [Relations] attribute",
        messageFormat: "Property '{0}' on '{1}' is of type ILink but has no [Relations] attribute — it will be ignored in the schema",
        category: "RESTyard.Schema",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DuplicateLinkRelations = new(
        id: "RY0040",
        title: "Duplicate link relations",
        messageFormat: "Properties '{0}' and '{1}' on '{2}' have identical [Relations] — the last one will win at runtime (Siren relations identify a unique link)",
        category: "RESTyard.Siren",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Extract [assembly: HypermediaAssembly] configuration from the compilation.
        // Returns null if the attribute is absent (no generation), or the Schema/Siren settings.
        var assemblyConfig = context.CompilationProvider.Select(static (compilation, _) =>
        {
            var attr = compilation.Assembly.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == HypermediaAssemblyAttributeFullName);

            if (attr == null)
            {
                return ((bool Schema, bool Siren)?)null;
            }

            var schema = true;
            var siren = false;

            foreach (var named in attr.NamedArguments)
            {
                if (named.Key == "Schema" && named.Value.Value is bool s)
                {
                    schema = s;
                }
                else if (named.Key == "Siren" && named.Value.Value is bool si)
                {
                    siren = si;
                }
            }

            return (schema, siren);
        });

        var htoTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                HypermediaObjectAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => ExtractHtoMetadata(ctx))
            .Where(static m => m.HasValue)
            .Select(static (m, _) => m!.Value);

        // Extract action result mappings from controller [HypermediaActionEndpoint] attributes with ResultType
        var actionResultMappings = context.CompilationProvider.Select(static (compilation, _) =>
            ExtractActionResultMappings(compilation));

        // Combine each HTO with the assembly configuration and action result mappings
        var htosWithConfig = htoTypes.Combine(assemblyConfig).Combine(actionResultMappings);

        context.RegisterSourceOutput(htosWithConfig, static (spc, combined) =>
        {
            var ((metadata, config), resultData) = combined;
            var resultMappings = resultData.Mappings;

            // Emit warnings for ResultType not being a HypermediaObject
            foreach (var (resultTypeName, htoClassName, actionPropName) in resultData.NotHtoWarnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    ResultTypeNotHypermediaObject,
                    Location.None,
                    resultTypeName, htoClassName, actionPropName));
            }

            // Emit warnings for 201 response without ResultType
            foreach (var (controllerName, methodName, actionPropName) in resultData.Missing201Warnings)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    MissingResultTypeWith201,
                    Location.None,
                    controllerName, methodName, actionPropName));
            }

            // No [HypermediaAssembly] attribute — emit nothing
            if (config == null)
            {
                return;
            }

            var schema = config.Value.Schema;
            var siren = config.Value.Siren;

            // Siren = true forces Schema = true
            if (siren && !schema)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    SirenRequiresSchema,
                    Location.None));
                schema = true;
            }

            // Schema = false — emit nothing (safety hatch)
            if (!schema)
            {
                return;
            }

            // Enrich actions with ResultType from controller endpoint attributes
            metadata = EnrichActionsWithResultMappings(metadata, resultMappings);

            foreach (var propertyName in metadata.EmbeddedEntityPropertiesWithoutRelations)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    EmbeddedEntityMissingRelations,
                    Location.None,
                    propertyName,
                    metadata.ClassName));
            }

            foreach (var propertyName in metadata.LinkPropertiesWithoutRelations)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    LinkMissingRelations,
                    Location.None,
                    propertyName,
                    metadata.ClassName));
            }

            // Detect duplicate link relations
            var seenLinkRelations = new Dictionary<string, string>(); // relKey → first property name
            foreach (var link in metadata.Links)
            {
                var relKey = string.Join(",", link.Relations.OrderBy(r => r, System.StringComparer.Ordinal));
                if (seenLinkRelations.TryGetValue(relKey, out var firstPropertyName))
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        DuplicateLinkRelations,
                        Location.None,
                        firstPropertyName,
                        link.PropertyName,
                        metadata.ClassName));
                }
                else
                {
                    seenLinkRelations[relKey] = link.PropertyName;
                }
            }

            spc.AddSource(
                $"{metadata.ClassName}Schema.g.cs",
                GenerateSchemaSource(metadata));

            if (metadata.Properties.Length > 0)
            {
                spc.AddSource(
                    $"{metadata.ClassName}Properties.g.cs",
                    GeneratePropertiesPoco(metadata));
            }

            // Siren = true — emit ToSiren() and ToSirenEmbedded() extension methods
            if (siren)
            {
                spc.AddSource(
                    $"{metadata.ClassName}SirenExtensions.g.cs",
                    GenerateSirenSource(metadata));
            }
        });

        // Collect all HTOs and emit a per-assembly registry + assembly attribute.
        var assemblyName = context.CompilationProvider.Select(
            static (compilation, _) => SanitizeAssemblyName(compilation.AssemblyName ?? "Unknown"));

        var allHtosWithConfig = htoTypes.Collect().Combine(assemblyConfig).Combine(assemblyName).Combine(actionResultMappings);

        context.RegisterSourceOutput(allHtosWithConfig, static (spc, combined) =>
        {
            var (((allHtos, config), assemblyNameSafe), resultData) = combined;

            // No [HypermediaAssembly] or Schema = false — no registry
            if (config == null)
            {
                return;
            }

            var schema = config.Value.Schema;
            var siren = config.Value.Siren;
            if (siren && !schema)
            {
                schema = true;
            }

            if (!schema || allHtos.IsEmpty)
            {
                return;
            }

            spc.AddSource(
                $"HypermediaSchemaRegistry.g.cs",
                GenerateRegistrySource(allHtos, assemblyNameSafe));

            // Siren = true — emit shared SirenHelper class with AddLink, AddAction, etc.
            if (siren)
            {
                spc.AddSource(
                    "SirenHelper.g.cs",
                    GenerateSirenHelper());
            }

            // Emit action result registry for multi-assembly support
            if (!resultData.Mappings.IsEmpty)
            {
                spc.AddSource(
                    $"HypermediaActionResultRegistry.g.cs",
                    GenerateActionResultRegistrySource(resultData.Mappings, assemblyNameSafe));
            }
        });
    }

    private static HtoMetadata? ExtractHtoMetadata(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (!ImplementsInterface(symbol, IHypermediaObjectFullName))
        {
            return null;
        }

        var attribute = context.Attributes[0];

        var title = GetNamedArgumentString(attribute, "Title");
        if (string.IsNullOrEmpty(title))
        {
            title = null;
        }

        // Title fallback: [Title] attribute > XML doc <summary>
        if (title == null)
        {
            title = GetAttributeStringArgument(symbol, TitleAttributeFullName);
        }

        if (title == null)
        {
            title = GetXmlDocElement(symbol, "summary");
        }

        // Description: [Description] attribute > XML doc <remarks>
        var description = GetAttributeStringArgument(symbol, DescriptionAttributeFullName);
        if (description == null)
        {
            description = GetXmlDocElement(symbol, "remarks");
        }

        // Deprecation: [Obsolete("message")]
        var (isDeprecated, deprecationMessage) = GetDeprecation(symbol);

        var classes = GetNamedArgumentStringArray(attribute, "Classes");
        var accessGroups = GetAccessGroups(symbol);
        var properties = ExtractProperties(symbol);
        var links = ExtractLinks(symbol);
        var actions = ExtractActions(symbol);
        var embeddedEntities = ExtractEmbeddedEntities(symbol);
        var embeddedWithoutRelations = FindEmbeddedEntityPropertiesWithoutRelations(symbol);
        var linksWithoutRelations = FindLinkPropertiesWithoutRelations(symbol);

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        return new HtoMetadata(
            ns,
            symbol.Name,
            DeriveSchemaName(symbol.Name),
            title,
            description,
            isDeprecated,
            deprecationMessage,
            new EquatableArray<string>(classes),
            new EquatableArray<string>(accessGroups),
            properties,
            links,
            actions,
            embeddedEntities,
            embeddedWithoutRelations,
            linksWithoutRelations);
    }

    private static EquatableArray<PropertyMetadata> ExtractProperties(INamedTypeSymbol symbol)
    {
        var properties = new List<PropertyMetadata>();
        var seen = new HashSet<string>();

        var current = symbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Public
                    || member.IsStatic
                    || member.IsIndexer)
                {
                    continue;
                }

                if (!seen.Add(member.Name))
                {
                    continue; // Already seen in derived class (override)
                }

                if (ShouldExcludeProperty(member))
                {
                    continue;
                }

                var name = GetPropertySerializationName(member);
                var typeFullName = member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var forwardedAttributes = GetForwardedAttributes(member);
                var xmlDocComment = GetXmlDocComment(member);
                properties.Add(new PropertyMetadata(name, member.Name, typeFullName, forwardedAttributes, xmlDocComment));
            }

            current = current.BaseType;
        }

        return new EquatableArray<PropertyMetadata>(properties.ToImmutableArray());
    }

    private static EquatableArray<LinkMetadata> ExtractLinks(INamedTypeSymbol symbol)
    {
        var links = new List<LinkMetadata>();
        var seen = new HashSet<string>();

        var current = symbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Public
                    || member.IsStatic
                    || member.IsIndexer)
                {
                    continue;
                }

                if (!seen.Add(member.Name))
                {
                    continue;
                }

                var targetType = GetLinkTargetType(member);
                if (targetType == null)
                {
                    continue;
                }

                var relationsAttr = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == RelationsAttributeFullName);
                if (relationsAttr == null)
                {
                    continue;
                }

                var relations = GetRelationsFromAttribute(relationsAttr);
                var targetSchemaName = DeriveSchemaName(targetType.Name);
                var targetClasses = GetTargetClasses(targetType);
                var isMandatory = member.NullableAnnotation != NullableAnnotation.Annotated;

                // Title: [Title] attribute > XML doc <summary>
                var linkTitle = GetAttributeStringArgument(member, TitleAttributeFullName)
                                ?? GetXmlDocElement(member, "summary");

                // Description: [Description] attribute > XML doc <remarks>
                var linkDescription = GetAttributeStringArgument(member, DescriptionAttributeFullName)
                                      ?? GetXmlDocElement(member, "remarks");

                var (linkIsDeprecated, linkDeprecationMessage) = GetDeprecation(member);
                var linkAccessGroups = GetAccessGroups(member);

                links.Add(new LinkMetadata(
                    member.Name,
                    new EquatableArray<string>(relations),
                    targetSchemaName,
                    new EquatableArray<string>(targetClasses),
                    linkTitle,
                    linkDescription,
                    linkIsDeprecated,
                    linkDeprecationMessage,
                    isMandatory,
                    new EquatableArray<string>(linkAccessGroups)));
            }

            current = current.BaseType;
        }

        return new EquatableArray<LinkMetadata>(links.ToImmutableArray());
    }

    private static EquatableArray<ActionMetadata> ExtractActions(INamedTypeSymbol symbol)
    {
        var actions = new List<ActionMetadata>();
        var seen = new HashSet<string>();

        var current = symbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Public
                    || member.IsStatic
                    || member.IsIndexer)
                {
                    continue;
                }

                if (!seen.Add(member.Name))
                {
                    continue;
                }

                var actionAttr = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == HypermediaActionAttributeFullName);
                if (actionAttr == null)
                {
                    continue;
                }

                if (!IsActionType(member.Type))
                {
                    continue;
                }

                var name = GetNamedArgumentString(actionAttr, "Name") ?? member.Name;

                // Title: [HypermediaAction(Title)] > [Title] attribute > XML doc <summary>
                var actionTitle = GetNamedArgumentString(actionAttr, "Title")
                                  ?? GetAttributeStringArgument(member, TitleAttributeFullName)
                                  ?? GetXmlDocElement(member, "summary");

                // Description: [Description] attribute > XML doc <remarks>
                var actionDescription = GetAttributeStringArgument(member, DescriptionAttributeFullName)
                                        ?? GetXmlDocElement(member, "remarks");

                var (actionIsDeprecated, actionDeprecationMessage) = GetDeprecation(member);

                var parameterTypeFullName = GetActionParameterType(member.Type);
                var isFileUpload = IsFileUploadAction(member.Type);
                var isMandatory = member.NullableAnnotation != NullableAnnotation.Annotated;
                var actionAccessGroups = GetAccessGroups(member);

                // User-defined classes from [HypermediaAction(Classes = [...])]
                var userClasses = GetNamedArgumentStringArray(actionAttr, "Classes");

                actions.Add(new ActionMetadata(member.Name, name, actionTitle, actionDescription, parameterTypeFullName, isFileUpload, actionIsDeprecated, actionDeprecationMessage, isMandatory, null, null, new EquatableArray<string>(userClasses), new EquatableArray<string>(actionAccessGroups)));
            }

            current = current.BaseType;
        }

        return new EquatableArray<ActionMetadata>(actions.ToImmutableArray());
    }

    private static EquatableArray<EmbeddedEntityMetadata> ExtractEmbeddedEntities(INamedTypeSymbol symbol)
    {
        var embeddedEntities = new List<EmbeddedEntityMetadata>();
        var seen = new HashSet<string>();

        var current = symbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Public
                    || member.IsStatic
                    || member.IsIndexer)
                {
                    continue;
                }

                if (!seen.Add(member.Name))
                {
                    continue;
                }

                var relationsAttr = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == RelationsAttributeFullName);
                if (relationsAttr == null)
                {
                    continue;
                }

                // Skip link properties — those are handled by ExtractLinks
                if (GetLinkTargetType(member) != null)
                {
                    continue;
                }

                // Try single embedded entity: IEmbeddedEntity<THto>
                var (targetType, isCollection) = GetEmbeddedEntityTargetType(member);
                if (targetType == null)
                {
                    continue;
                }

                var relations = GetRelationsFromAttribute(relationsAttr);
                var targetSchemaName = DeriveSchemaName(targetType.Name);
                var targetClasses = GetTargetClasses(targetType);
                var isMandatory = member.NullableAnnotation != NullableAnnotation.Annotated;

                // Title: [Title] attribute > XML doc <summary>
                var embeddedTitle = GetAttributeStringArgument(member, TitleAttributeFullName)
                                    ?? GetXmlDocElement(member, "summary");

                // Description: [Description] attribute > XML doc <remarks>
                var embeddedDescription = GetAttributeStringArgument(member, DescriptionAttributeFullName)
                                          ?? GetXmlDocElement(member, "remarks");

                var (embeddedIsDeprecated, embeddedDeprecationMessage) = GetDeprecation(member);
                var embeddedAccessGroups = GetAccessGroups(member);

                embeddedEntities.Add(new EmbeddedEntityMetadata(
                    member.Name,
                    new EquatableArray<string>(relations),
                    targetSchemaName,
                    targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    new EquatableArray<string>(targetClasses),
                    isCollection,
                    embeddedTitle,
                    embeddedDescription,
                    embeddedIsDeprecated,
                    embeddedDeprecationMessage,
                    isMandatory,
                    new EquatableArray<string>(embeddedAccessGroups)));
            }

            current = current.BaseType;
        }

        return new EquatableArray<EmbeddedEntityMetadata>(embeddedEntities.ToImmutableArray());
    }

    /// <summary>
    /// Extracts the target HTO type from an embedded entity property.
    /// Returns the target type and whether it's a collection.
    /// Supports <c>IEmbeddedEntity&lt;THto&gt;</c> (single) and
    /// <c>List&lt;IEmbeddedEntity&lt;THto&gt;&gt;</c> / <c>IList&lt;...&gt;</c> / etc. (collection).
    /// </summary>
    private static (INamedTypeSymbol? TargetType, bool IsCollection) GetEmbeddedEntityTargetType(
        IPropertySymbol property)
    {
        var type = property.Type;

        // Unwrap nullable
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        // Check if the type itself is IEmbeddedEntity<THto>
        var singleTarget = GetIEmbeddedEntityTypeArgument(type);
        if (singleTarget != null)
        {
            return (singleTarget, false);
        }

        // Check if the type is a collection of IEmbeddedEntity<THto>
        var collectionTarget = GetCollectionEmbeddedEntityTarget(type);
        if (collectionTarget != null)
        {
            return (collectionTarget, true);
        }

        return (null, false);
    }

    /// <summary>
    /// If <paramref name="type"/> is or implements <c>IEmbeddedEntity&lt;THto&gt;</c>,
    /// returns THto. Otherwise returns null.
    /// </summary>
    private static INamedTypeSymbol? GetIEmbeddedEntityTypeArgument(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named && IsIEmbeddedEntityGeneric(named))
        {
            return named.TypeArguments[0] as INamedTypeSymbol;
        }

        if (type is INamedTypeSymbol namedType)
        {
            foreach (var iface in namedType.AllInterfaces)
            {
                if (IsIEmbeddedEntityGeneric(iface))
                {
                    return iface.TypeArguments[0] as INamedTypeSymbol;
                }
            }
        }

        return null;
    }

    private static bool IsIEmbeddedEntityGeneric(INamedTypeSymbol type)
    {
        return type.IsGenericType
               && type.OriginalDefinition.ToDisplayString() == IEmbeddedEntityFullName;
    }

    /// <summary>
    /// Checks if the type is a generic collection (List, IList, ICollection, IEnumerable,
    /// IReadOnlyList, IReadOnlyCollection) whose element type is <c>IEmbeddedEntity&lt;THto&gt;</c>.
    /// Returns THto if found.
    /// </summary>
    private static INamedTypeSymbol? GetCollectionEmbeddedEntityTarget(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || !named.IsGenericType)
        {
            return null;
        }

        // Check the element type of the first type argument
        var elementType = named.TypeArguments[0];
        return GetIEmbeddedEntityTypeArgument(elementType);
    }

    private static bool IsActionType(ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            if (current is INamedTypeSymbol named)
            {
                var fullName = named.OriginalDefinition.ToDisplayString();
                if (fullName == HypermediaActionBaseFullName)
                {
                    return true;
                }
            }

            current = current.BaseType;
        }

        return false;
    }

    private static string? GetActionParameterType(ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            if (current is INamedTypeSymbol named && named.IsGenericType)
            {
                var fullName = named.OriginalDefinition.ToDisplayString();
                if (fullName == HypermediaActionGenericFullName
                    || fullName == FileUploadHypermediaActionGenericFullName)
                {
                    return named.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                }
            }

            current = current.BaseType;
        }

        return null;
    }

    private static bool IsFileUploadAction(ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            if (current is INamedTypeSymbol named)
            {
                var fullName = named.OriginalDefinition.ToDisplayString();
                if (fullName == FileUploadHypermediaActionFullName
                    || fullName == FileUploadHypermediaActionGenericFullName)
                {
                    return true;
                }
            }

            current = current.BaseType;
        }

        return false;
    }

    private static INamedTypeSymbol? GetLinkTargetType(IPropertySymbol property)
    {
        var type = property.Type;

        // Unwrap nullable: ILink<T>? -> ILink<T>
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        // Check if the type itself is ILink<T>
        if (type is INamedTypeSymbol named && IsILinkGeneric(named))
        {
            return named.TypeArguments[0] as INamedTypeSymbol;
        }

        // Check implemented interfaces for ILink<T>
        if (type is INamedTypeSymbol namedType)
        {
            foreach (var iface in namedType.AllInterfaces)
            {
                if (IsILinkGeneric(iface))
                {
                    return iface.TypeArguments[0] as INamedTypeSymbol;
                }
            }
        }

        return null;
    }

    private static bool IsILinkGeneric(INamedTypeSymbol type)
    {
        return type.IsGenericType
               && type.OriginalDefinition.ToDisplayString() == ILinkFullName;
    }

    private static ImmutableArray<string> GetRelationsFromAttribute(AttributeData relationsAttr)
    {
        if (relationsAttr.ConstructorArguments.Length > 0)
        {
            var arg = relationsAttr.ConstructorArguments[0];
            if (arg.Kind == TypedConstantKind.Array)
            {
                return arg.Values
                    .Where(v => v.Value is string)
                    .Select(v => (string)v.Value!)
                    .ToImmutableArray();
            }
        }

        return ImmutableArray<string>.Empty;
    }

    private static ImmutableArray<string> GetTargetClasses(INamedTypeSymbol targetType)
    {
        var htoAttr = targetType.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == HypermediaObjectAttributeFullName);

        if (htoAttr != null)
        {
            return GetNamedArgumentStringArray(htoAttr, "Classes");
        }

        return ImmutableArray<string>.Empty;
    }

    private static bool ShouldExcludeProperty(IPropertySymbol property)
    {
        var attributes = property.GetAttributes();
        return HasAttribute(attributes, FormatterIgnoreAttributeFullName)
               || HasAttribute(attributes, RelationsAttributeFullName)
               || HasAttribute(attributes, HypermediaActionAttributeFullName)
               || IsEmbeddedEntityType(property.Type)
               || IsLinkType(property.Type);
    }

    /// <summary>
    /// Checks whether a type is <c>ILink&lt;T&gt;</c>.
    /// Used to exclude link properties from the data properties schema
    /// even when they are missing the <c>[Relations]</c> attribute.
    /// </summary>
    private static bool IsLinkType(ITypeSymbol type)
    {
        // Unwrap nullable
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        return GetLinkTargetType(type) != null;
    }

    /// <summary>
    /// Overload of <see cref="GetLinkTargetType(IPropertySymbol)"/> that works on the type directly.
    /// </summary>
    private static INamedTypeSymbol? GetLinkTargetType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named && IsILinkGeneric(named))
        {
            return named.TypeArguments[0] as INamedTypeSymbol;
        }

        if (type is INamedTypeSymbol namedType)
        {
            foreach (var iface in namedType.AllInterfaces)
            {
                if (IsILinkGeneric(iface))
                {
                    return iface.TypeArguments[0] as INamedTypeSymbol;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether a type is or contains <c>IEmbeddedEntity</c> (the non-generic base).
    /// Used to exclude embedded entity properties from the data properties schema
    /// even when they are missing the <c>[Relations]</c> attribute.
    /// </summary>
    private static bool IsEmbeddedEntityType(ITypeSymbol type)
    {
        // Unwrap nullable
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            type = nullable.TypeArguments[0];
        }

        // Direct IEmbeddedEntity<T>
        if (GetIEmbeddedEntityTypeArgument(type) != null)
        {
            return true;
        }

        // Collection of IEmbeddedEntity<T>
        if (GetCollectionEmbeddedEntityTarget(type) != null)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Finds embedded entity properties (IEmbeddedEntity or collections thereof) that are
    /// missing <c>[Relations]</c>. These are reported as RY0020 warnings.
    /// </summary>
    private static EquatableArray<string> FindEmbeddedEntityPropertiesWithoutRelations(INamedTypeSymbol symbol)
    {
        var names = new List<string>();
        var seen = new HashSet<string>();

        var current = symbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Public
                    || member.IsStatic
                    || member.IsIndexer)
                {
                    continue;
                }

                if (!seen.Add(member.Name))
                {
                    continue;
                }

                if (!IsEmbeddedEntityType(member.Type))
                {
                    continue;
                }

                var hasRelations = member.GetAttributes()
                    .Any(a => a.AttributeClass?.ToDisplayString() == RelationsAttributeFullName);
                if (!hasRelations)
                {
                    names.Add(member.Name);
                }
            }

            current = current.BaseType;
        }

        return new EquatableArray<string>(names.ToImmutableArray());
    }

    /// <summary>
    /// Finds link properties (ILink&lt;T&gt;) that are missing <c>[Relations]</c>.
    /// These are reported as RY0021 warnings.
    /// </summary>
    private static EquatableArray<string> FindLinkPropertiesWithoutRelations(INamedTypeSymbol symbol)
    {
        var names = new List<string>();
        var seen = new HashSet<string>();

        var current = symbol;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility != Accessibility.Public
                    || member.IsStatic
                    || member.IsIndexer)
                {
                    continue;
                }

                if (!seen.Add(member.Name))
                {
                    continue;
                }

                if (!IsLinkType(member.Type))
                {
                    continue;
                }

                var hasRelations = member.GetAttributes()
                    .Any(a => a.AttributeClass?.ToDisplayString() == RelationsAttributeFullName);
                if (!hasRelations)
                {
                    names.Add(member.Name);
                }
            }

            current = current.BaseType;
        }

        return new EquatableArray<string>(names.ToImmutableArray());
    }

    private static string GetPropertySerializationName(IPropertySymbol property)
    {
        var attr = property.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == HypermediaPropertyAttributeFullName);

        if (attr != null)
        {
            var nameArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "Name");
            if (nameArg.Key == "Name" && nameArg.Value.Value is string customName
                && !string.IsNullOrEmpty(customName))
            {
                return customName;
            }
        }

        return property.Name;
    }

    private static bool HasAttribute(ImmutableArray<AttributeData> attributes, string fullName)
    {
        return attributes.Any(a => a.AttributeClass?.ToDisplayString() == fullName);
    }

    private static bool ImplementsInterface(INamedTypeSymbol symbol, string fullInterfaceName)
    {
        return symbol.AllInterfaces.Any(i => i.ToDisplayString() == fullInterfaceName);
    }

    private static string? GetNamedArgumentString(AttributeData attribute, string name)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(a => a.Key == name);
        return arg.Key == name && arg.Value.Value is string s ? s : null;
    }

    private static ImmutableArray<string> GetNamedArgumentStringArray(AttributeData attribute, string name)
    {
        var arg = attribute.NamedArguments.FirstOrDefault(a => a.Key == name);
        if (arg.Key != name || arg.Value.IsNull)
        {
            return ImmutableArray<string>.Empty;
        }

        return arg.Value.Values
            .Where(v => v.Value is string)
            .Select(v => (string)v.Value!)
            .ToImmutableArray();
    }

    internal static string DeriveSchemaName(string className)
    {
        var name = className;
        if (name.StartsWith("Hypermedia"))
        {
            name = name.Substring("Hypermedia".Length);
        }

        if (name.EndsWith("Hto"))
        {
            name = name.Substring(0, name.Length - "Hto".Length);
        }

        return name;
    }

    internal static string GenerateSchemaSource(HtoMetadata metadata)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.Append("using ").Append(SchemaTypeNames.SchemaModelNamespace).AppendLine(";");

        if (metadata.NeedsSchemaFactory)
        {
            sb.Append("using ").Append(SchemaTypeNames.JsonSchemaFactoryNamespace).AppendLine(";");
            sb.Append("using ").Append(SchemaTypeNames.JsonSchemaNamespace).AppendLine(";");
            sb.Append("using ").Append(SchemaTypeNames.JsonDocumentNamespace).AppendLine(";");
        }

        sb.AppendLine();

        if (!string.IsNullOrEmpty(metadata.Namespace))
        {
            sb.Append("namespace ").Append(metadata.Namespace).AppendLine(";");
            sb.AppendLine();
        }

        sb.Append("public static class ").Append(metadata.ClassName).AppendLine("Schema");
        sb.AppendLine("{");

        // GetSchema() accepts IJsonSchemaFactory when there are properties or parameterized actions
        if (metadata.NeedsSchemaFactory)
        {
            sb.Append("    public static ").Append(SchemaTypeNames.EntityTypeSchema)
                .Append(" GetSchema(").Append(SchemaTypeNames.IJsonSchemaFactory)
                .AppendLine(" schemaFactory)");
        }
        else
        {
            sb.Append("    public static ").Append(SchemaTypeNames.EntityTypeSchema).AppendLine(" GetSchema()");
        }

        sb.AppendLine("    {");

        if (metadata.Properties.Length > 0)
        {
            EmitPropertiesSchemaBuilder(sb, metadata);
        }

        sb.Append("        return new ").AppendLine(SchemaTypeNames.EntityTypeSchema);
        sb.AppendLine("        {");
        sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_Name)
            .Append(" = \"").Append(EscapeString(metadata.SchemaName)).AppendLine("\",");

        if (metadata.Title != null)
        {
            sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_Title)
                .Append(" = \"").Append(EscapeString(metadata.Title)).AppendLine("\",");
        }

        if (metadata.Description != null)
        {
            sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_Description)
                .Append(" = \"").Append(EscapeString(metadata.Description)).AppendLine("\",");
        }

        if (metadata.IsDeprecated)
        {
            sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_IsDeprecated)
                .AppendLine(" = true,");
            if (metadata.DeprecationMessage != null)
            {
                sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_DeprecationMessage)
                    .Append(" = \"").Append(EscapeString(metadata.DeprecationMessage)).AppendLine("\",");
            }
        }

        EmitAccessGroups(sb, metadata.AccessGroups, SchemaTypeNames.EntityTypeSchema_AccessGroups, "            ");

        var classLiterals = string.Join(", ", metadata.Classes.Select(c => $"\"{EscapeString(c)}\""));
        sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_Classes)
            .Append(" = new[] { ").Append(classLiterals).AppendLine(" },");

        if (metadata.Properties.Length > 0)
        {
            sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_PropertiesSchema)
                .AppendLine(" = propertiesSchema,");
        }

        if (metadata.Links.Length > 0)
        {
            EmitLinksArray(sb, metadata.Links);
        }

        if (metadata.Actions.Length > 0)
        {
            EmitActionsArray(sb, metadata.Actions);
        }

        if (metadata.EmbeddedEntities.Length > 0)
        {
            EmitEmbeddedEntitiesArray(sb, metadata.EmbeddedEntities);
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Emits code that generates the properties schema via the generated POCO type.
    /// Generates a local variable <c>propertiesSchema</c>.
    /// </summary>
    private static void EmitPropertiesSchemaBuilder(StringBuilder sb, HtoMetadata metadata)
    {
        sb.Append("        var propertiesSchema = schemaFactory.Generate(typeof(")
            .Append(metadata.ClassName).AppendLine("Properties));");
        sb.AppendLine();
    }

    private static void EmitLinksArray(StringBuilder sb, EquatableArray<LinkMetadata> links)
    {
        sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_Links)
            .Append(" = new ").Append(SchemaTypeNames.LinkDescription).AppendLine("[]");
        sb.AppendLine("            {");

        foreach (var link in links)
        {
            var relLiterals = string.Join(", ", link.Relations.Select(r => $"\"{EscapeString(r)}\""));
            var classLiterals = string.Join(", ", link.TargetClasses.Select(c => $"\"{EscapeString(c)}\""));

            sb.Append("                new ").AppendLine(SchemaTypeNames.LinkDescription);
            sb.AppendLine("                {");
            sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_Relations)
                .Append(" = new[] { ").Append(relLiterals).AppendLine(" },");
            sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_TargetName)
                .Append(" = \"").Append(EscapeString(link.TargetSchemaName)).AppendLine("\",");
            sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_TargetClasses)
                .Append(" = new[] { ").Append(classLiterals).AppendLine(" },");
            if (link.Title != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_Title)
                    .Append(" = \"").Append(EscapeString(link.Title)).AppendLine("\",");
            }

            if (link.Description != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_Description)
                    .Append(" = \"").Append(EscapeString(link.Description)).AppendLine("\",");
            }

            if (link.IsDeprecated)
            {
                sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_IsDeprecated)
                    .AppendLine(" = true,");
                if (link.DeprecationMessage != null)
                {
                    sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_DeprecationMessage)
                        .Append(" = \"").Append(EscapeString(link.DeprecationMessage)).AppendLine("\",");
                }
            }

            EmitAccessGroups(sb, link.AccessGroups, SchemaTypeNames.LinkDescription_AccessGroups, "                    ");
            sb.Append("                    ").Append(SchemaTypeNames.LinkDescription_IsMandatory)
                .Append(" = ").Append(link.IsMandatory ? "true" : "false").AppendLine(",");
            sb.AppendLine("                },");
        }

        sb.AppendLine("            },");
    }

    private static void EmitActionsArray(StringBuilder sb, EquatableArray<ActionMetadata> actions)
    {
        sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_Actions)
            .Append(" = new ").Append(SchemaTypeNames.ActionDescription).AppendLine("[]");
        sb.AppendLine("            {");

        foreach (var action in actions)
        {
            sb.Append("                new ").AppendLine(SchemaTypeNames.ActionDescription);
            sb.AppendLine("                {");
            sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_Name)
                .Append(" = \"").Append(EscapeString(action.Name)).AppendLine("\",");

            if (action.Title != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_Title)
                    .Append(" = \"").Append(EscapeString(action.Title)).AppendLine("\",");
            }

            if (action.Description != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_Description)
                    .Append(" = \"").Append(EscapeString(action.Description)).AppendLine("\",");
            }

            if (action.IsFileUpload)
            {
                sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_ContentType)
                    .AppendLine(" = \"multipart/form-data\",");
                sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_IsFileUpload)
                    .AppendLine(" = true,");
            }

            if (action.ParameterTypeFullName != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_ParameterSchema)
                    .Append(" = schemaFactory.Generate(typeof(")
                    .Append(action.ParameterTypeFullName).AppendLine(")),");
            }

            if (action.IsDeprecated)
            {
                sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_IsDeprecated)
                    .AppendLine(" = true,");
                if (action.DeprecationMessage != null)
                {
                    sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_DeprecationMessage)
                        .Append(" = \"").Append(EscapeString(action.DeprecationMessage)).AppendLine("\",");
                }
            }

            if (action.ResultSchemaName != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_ResultName)
                    .Append(" = \"").Append(EscapeString(action.ResultSchemaName)).AppendLine("\",");

                if (action.ResultClasses is { Length: > 0 } rc)
                {
                    var classLiterals = string.Join(", ", rc.Select(c => $"\"{EscapeString(c)}\""));
                    sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_ResultClasses)
                        .Append(" = new[] { ").Append(classLiterals).AppendLine(" },");
                }
            }

            EmitAccessGroups(sb, action.AccessGroups, SchemaTypeNames.ActionDescription_AccessGroups, "                    ");
            sb.Append("                    ").Append(SchemaTypeNames.ActionDescription_IsMandatory)
                .Append(" = ").Append(action.IsMandatory ? "true" : "false").AppendLine(",");
            sb.AppendLine("                },");
        }

        sb.AppendLine("            },");
    }

    private static void EmitEmbeddedEntitiesArray(StringBuilder sb, EquatableArray<EmbeddedEntityMetadata> embeddedEntities)
    {
        sb.Append("            ").Append(SchemaTypeNames.EntityTypeSchema_EmbeddedEntities)
            .Append(" = new ").Append(SchemaTypeNames.EmbeddedEntityDescription).AppendLine("[]");
        sb.AppendLine("            {");

        foreach (var embedded in embeddedEntities)
        {
            var relLiterals = string.Join(", ", embedded.Relations.Select(r => $"\"{EscapeString(r)}\""));
            var classLiterals = string.Join(", ", embedded.TargetClasses.Select(c => $"\"{EscapeString(c)}\""));

            sb.Append("                new ").AppendLine(SchemaTypeNames.EmbeddedEntityDescription);
            sb.AppendLine("                {");
            sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_Relations)
                .Append(" = new[] { ").Append(relLiterals).AppendLine(" },");
            sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_TargetName)
                .Append(" = \"").Append(EscapeString(embedded.TargetSchemaName)).AppendLine("\",");
            sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_TargetClasses)
                .Append(" = new[] { ").Append(classLiterals).AppendLine(" },");
            sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_IsCollection)
                .Append(" = ").Append(embedded.IsCollection ? "true" : "false").AppendLine(",");

            if (embedded.Title != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_Title)
                    .Append(" = \"").Append(EscapeString(embedded.Title)).AppendLine("\",");
            }

            if (embedded.Description != null)
            {
                sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_Description)
                    .Append(" = \"").Append(EscapeString(embedded.Description)).AppendLine("\",");
            }

            if (embedded.IsDeprecated)
            {
                sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_IsDeprecated)
                    .AppendLine(" = true,");
                if (embedded.DeprecationMessage != null)
                {
                    sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_DeprecationMessage)
                        .Append(" = \"").Append(EscapeString(embedded.DeprecationMessage)).AppendLine("\",");
                }
            }

            EmitAccessGroups(sb, embedded.AccessGroups, SchemaTypeNames.EmbeddedEntityDescription_AccessGroups, "                    ");
            sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_IsMandatory)
                .Append(" = ").Append(embedded.IsMandatory ? "true" : "false").AppendLine(",");
            sb.AppendLine("                },");
        }

        sb.AppendLine("            },");
    }

    /// <summary>
    /// Emits a <c>AccessGroups = new[] { "group1", "group2" }</c> assignment
    /// when the access groups array is non-empty. Emits nothing when empty (null in schema = public).
    /// </summary>
    private static void EmitAccessGroups(StringBuilder sb, EquatableArray<string> accessGroups, string propertyName, string indent)
    {
        if (accessGroups.Length == 0)
        {
            return;
        }

        var literals = string.Join(", ", accessGroups.Select(g => $"\"{EscapeString(g)}\""));
        sb.Append(indent).Append(propertyName)
            .Append(" = new[] { ").Append(literals).AppendLine(" },");
    }

    /// <summary>
    /// Reads <c>[Obsolete("message")]</c> from a symbol.
    /// Returns (true, message) if present, (false, null) otherwise.
    /// </summary>
    private static (bool IsDeprecated, string? DeprecationMessage) GetDeprecation(ISymbol symbol)
    {
        var attr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == ObsoleteAttributeFullName);

        if (attr == null)
        {
            return (false, null);
        }

        string? message = null;
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string msg
            && !string.IsNullOrEmpty(msg))
        {
            message = msg;
        }

        return (true, message);
    }

    /// <summary>
    /// Reads <c>[HypermediaAccessGroup("group1", "group2")]</c> from a symbol.
    /// Returns the access group names as an immutable array, or empty if not present.
    /// The attribute uses a <c>params string[]</c> constructor argument.
    /// </summary>
    private static ImmutableArray<string> GetAccessGroups(ISymbol symbol)
    {
        var attr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == HypermediaAccessGroupAttributeFullName);

        if (attr == null)
        {
            return ImmutableArray<string>.Empty;
        }

        // params string[] is passed as a single constructor argument containing an array of TypedConstants
        if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Kind == TypedConstantKind.Array)
        {
            return attr.ConstructorArguments[0].Values
                .Where(v => v.Value is string)
                .Select(v => (string)v.Value!)
                .ToImmutableArray();
        }

        return ImmutableArray<string>.Empty;
    }

    /// <summary>
    /// Reads the first constructor string argument from an attribute (e.g., <c>[Title("value")]</c>).
    /// Returns null if the attribute is not present or has no string argument.
    /// </summary>
    private static string? GetAttributeStringArgument(ISymbol symbol, string attributeFullName)
    {
        var attr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeFullName);

        if (attr == null || attr.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var value = attr.ConstructorArguments[0].Value as string;
        return string.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>
    /// Extracts the text content of the specified XML documentation element
    /// (e.g., "summary", "remarks") from a symbol's XML doc comment.
    /// Returns null if the element is not present or empty.
    /// </summary>
    private static string? GetXmlDocElement(ISymbol symbol, string elementName)
    {
        var xml = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrEmpty(xml))
        {
            return null;
        }

        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var node = doc.SelectSingleNode($"//{elementName}");
            if (node == null)
            {
                return null;
            }

            var text = node.InnerText.Trim();
            // Normalize internal whitespace (multi-line XML docs)
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            return string.IsNullOrEmpty(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Collects non-RESTyard attributes from a property symbol, serialized as source code strings.
    /// </summary>
    private static EquatableArray<string> GetForwardedAttributes(IPropertySymbol property)
    {
        var result = new List<string>();

        foreach (var attr in property.GetAttributes())
        {
            var attrClass = attr.AttributeClass;
            if (attrClass == null)
            {
                continue;
            }

            var fullName = attrClass.ToDisplayString();
            if (RestyardAttributeFullNames.Contains(fullName))
            {
                continue;
            }

            var serialized = SerializeAttribute(attr);
            if (serialized != null)
            {
                result.Add(serialized);
            }
        }

        return new EquatableArray<string>(result.ToImmutableArray());
    }

    /// <summary>
    /// Serializes an <see cref="AttributeData"/> to a source code string (e.g., <c>[JsonConverter(typeof(MyConverter))]</c>).
    /// </summary>
    private static string? SerializeAttribute(AttributeData attr)
    {
        var attrClass = attr.AttributeClass;
        if (attrClass == null)
        {
            return null;
        }

        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(attrClass.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        var hasArgs = attr.ConstructorArguments.Length > 0 || attr.NamedArguments.Length > 0;
        if (hasArgs)
        {
            sb.Append('(');
            var first = true;

            foreach (var arg in attr.ConstructorArguments)
            {
                if (!first) sb.Append(", ");
                first = false;
                sb.Append(FormatTypedConstant(arg));
            }

            foreach (var named in attr.NamedArguments)
            {
                if (!first) sb.Append(", ");
                first = false;
                sb.Append(named.Key).Append(" = ").Append(FormatTypedConstant(named.Value));
            }

            sb.Append(')');
        }

        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// Formats a <see cref="TypedConstant"/> as a C# source code literal.
    /// </summary>
    private static string FormatTypedConstant(TypedConstant constant)
    {
        if (constant.Kind == TypedConstantKind.Error)
        {
            return "default";
        }

        if (constant.Kind == TypedConstantKind.Array)
        {
            var elements = string.Join(", ", constant.Values.Select(FormatTypedConstant));
            return $"new[] {{ {elements} }}";
        }

        if (constant.Kind == TypedConstantKind.Type && constant.Value is INamedTypeSymbol typeSymbol)
        {
            return $"typeof({typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";
        }

        if (constant.Kind == TypedConstantKind.Enum)
        {
            var enumType = constant.Type;
            if (enumType != null)
            {
                return $"({enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}){constant.Value}";
            }
        }

        if (constant.Value is string s)
        {
            return $"\"{EscapeString(s)}\"";
        }

        if (constant.Value is bool b)
        {
            return b ? "true" : "false";
        }

        if (constant.Value == null)
        {
            return "null";
        }

        return constant.Value.ToString();
    }

    /// <summary>
    /// Gets the raw XML doc comment from a symbol's declaring syntax,
    /// formatted as lines of <c>/// </c> comments ready to emit in generated source.
    /// Returns null when no XML doc comment is present.
    /// </summary>
    private static string? GetXmlDocComment(ISymbol symbol)
    {
        var xml = symbol.GetDocumentationCommentXml();
        if (string.IsNullOrEmpty(xml))
        {
            return null;
        }

        // The XML returned by GetDocumentationCommentXml() wraps content in <member>...</member>.
        // Extract the inner elements and format as /// comments.
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            var memberNode = doc.SelectSingleNode("//member");
            if (memberNode == null || !memberNode.HasChildNodes)
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach (XmlNode child in memberNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    var outerXml = child.OuterXml.Trim();
                    sb.Append("/// ").AppendLine(outerXml);
                }
            }

            var result = sb.ToString().TrimEnd();
            return string.IsNullOrEmpty(result) ? null : result;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Emits a properties POCO class for the given HTO metadata.
    /// The POCO contains only data properties with non-RESTyard attributes forwarded
    /// and XML doc comments copied verbatim.
    /// </summary>
    internal static string GeneratePropertiesPoco(HtoMetadata metadata)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(metadata.Namespace))
        {
            sb.Append("namespace ").Append(metadata.Namespace).AppendLine(";");
            sb.AppendLine();
        }

        sb.Append("public class ").Append(metadata.ClassName).AppendLine("Properties");
        sb.AppendLine("{");

        foreach (var prop in metadata.Properties)
        {
            if (prop.XmlDocComment != null)
            {
                // Emit each line of the XML doc comment with proper indentation
                foreach (var line in prop.XmlDocComment.Split('\n'))
                {
                    var trimmed = line.TrimEnd('\r');
                    sb.Append("    ").AppendLine(trimmed);
                }
            }

            foreach (var attr in prop.ForwardedAttributes)
            {
                sb.Append("    ").AppendLine(attr);
            }

            sb.Append("    public ").Append(prop.TypeFullName).Append(' ').Append(prop.Name)
                .AppendLine(" { get; set; } = default!;");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Emits ToSiren() and ToSirenEmbedded() extension methods for an HTO class.
    /// Step 6.1: basic entity mapping — class, title, properties POCO, self link.
    /// Links, actions, and embedded entities are added in later steps.
    /// </summary>
    internal static string GenerateSirenSource(HtoMetadata metadata)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.Append("using ").Append(SchemaTypeNames.SirenModelNamespace).AppendLine(";");
        sb.Append("using ").Append(SchemaTypeNames.SirenNamespace).AppendLine(";");
        sb.Append("using ").Append(SchemaTypeNames.RouteResolverNamespace).AppendLine(";");
        sb.Append("using ").Append(SchemaTypeNames.QueryNamespace).AppendLine(";");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(metadata.Namespace))
        {
            sb.Append("namespace ").Append(metadata.Namespace).AppendLine(";");
            sb.AppendLine();
        }

        var propertiesType = metadata.Properties.Length > 0
            ? metadata.ClassName + "Properties"
            : SchemaTypeNames.NoProperties;

        sb.Append("public static class ").Append(metadata.ClassName).AppendLine("SirenExtensions");
        sb.AppendLine("{");

        // --- ToSiren() ---
        EmitToSirenMethod(sb, metadata, propertiesType, isEmbedded: false);

        sb.AppendLine();

        // --- OkSiren() controller convenience extension ---
        EmitOkSirenExtension(sb, metadata, propertiesType);

        sb.AppendLine();

        // --- ToSirenEmbedded() ---
        EmitToSirenMethod(sb, metadata, propertiesType, isEmbedded: true);

        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Emits an OkSiren() controller extension that resolves services from HttpContext,
    /// calls ToSiren(), sets the Siren content type, and returns OkObjectResult.
    /// </summary>
    private static void EmitOkSirenExtension(
        StringBuilder sb, HtoMetadata metadata, string propertiesType)
    {
        var returnType = $"Microsoft.AspNetCore.Mvc.ActionResult<{SchemaTypeNames.SirenEntity}<{propertiesType}>>";

        sb.AppendLine("    /// <summary>");
        sb.Append("    /// Converts the HTO to a <see cref=\"").Append(SchemaTypeNames.SirenEntity).Append("{T}\"/> and returns an ");
        sb.AppendLine("<see cref=\"Microsoft.AspNetCore.Mvc.ActionResult{T}\"/>");
        sb.AppendLine("    /// with <c>application/vnd.siren+json</c> content type.");
        sb.AppendLine("    /// Resolves route resolver, query string builder, and mapper options from DI.");
        sb.AppendLine("    /// </summary>");
        sb.Append("    public static ").Append(returnType).AppendLine(" OkSiren(");
        sb.AppendLine("        this Microsoft.AspNetCore.Mvc.ControllerBase controller,");
        sb.Append("        ").Append(metadata.ClassName).AppendLine(" hto)");
        sb.AppendLine("    {");
        sb.AppendLine("        var services = controller.HttpContext.RequestServices;");
        sb.AppendLine("        var resolver = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<RESTyard.AspNetCore.WebApi.RouteResolver.IHypermediaRouteResolver>(services);");
        sb.AppendLine("        var queryStringBuilder = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<RESTyard.AspNetCore.Query.IQueryStringBuilder>(services);");
        sb.Append("        var options = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetService<")
            .Append(SchemaTypeNames.SirenMapperOptions).Append(">(services) ?? ")
            .Append(SchemaTypeNames.SirenMapperOptions).AppendLine(".Default;");
        sb.Append("        controller.HttpContext.Response.ContentType = ").AppendLine("RESTyard.MediaTypes.DefaultMediaTypes.Siren;");
        sb.AppendLine("        return hto.ToSiren(resolver, queryStringBuilder, options);");
        sb.AppendLine("    }");
    }

    private static void EmitToSirenMethod(
        StringBuilder sb,
        HtoMetadata metadata,
        string propertiesType,
        bool isEmbedded)
    {
        var returnType = isEmbedded
            ? $"{SchemaTypeNames.SirenEmbeddedEntity}<{propertiesType}>"
            : $"{SchemaTypeNames.SirenEntity}<{propertiesType}>";
        var methodName = isEmbedded ? "ToSirenEmbedded" : "ToSiren";

        // EditorBrowsable(Never) for ToSirenEmbedded
        if (isEmbedded)
        {
            sb.Append("    [").Append(SchemaTypeNames.EditorBrowsableAttribute)
                .Append('(').Append(SchemaTypeNames.EditorBrowsableStateNever).AppendLine(")]");
        }

        sb.Append("    public static ").Append(returnType).Append(' ').Append(methodName).AppendLine("(");
        sb.Append("        this ").Append(metadata.ClassName).AppendLine(" hto,");
        sb.Append("        ").Append(SchemaTypeNames.IHypermediaRouteResolver).AppendLine(" resolver,");
        sb.Append("        ").Append(SchemaTypeNames.IQueryStringBuilder).AppendLine(" queryStringBuilder,");
        sb.Append("        ").Append(SchemaTypeNames.SirenMapperOptions).AppendLine("? options = null)");
        sb.AppendLine("    {");

        // Resolve options — fall back to static default
        sb.Append("        var effectiveOptions = options ?? ")
            .Append(SchemaTypeNames.SirenMapperOptions).AppendLine(".Default;");
        sb.AppendLine();

        // Resolve self route
        sb.AppendLine("        var selfRoute = resolver.ObjectToRoute(hto);");
        sb.AppendLine();

        // Create entity
        sb.Append("        var entity = new ").Append(returnType).AppendLine();
        sb.AppendLine("        {");

        // Rel — required by SirenSubEntity, initialized empty for embedded (caller sets it)
        if (isEmbedded)
        {
            sb.AppendLine("            Rel = System.Array.Empty<string>(),");
        }

        // Class
        EmitClassAssignment(sb, metadata);

        // Title
        if (metadata.Title != null)
        {
            sb.Append("            Title = \"").Append(EscapeString(metadata.Title)).AppendLine("\",");
        }

        // Properties
        if (metadata.Properties.Length > 0)
        {
            sb.Append("            Properties = new ").Append(propertiesType).AppendLine();
            sb.AppendLine("            {");
            foreach (var prop in metadata.Properties)
            {
                sb.Append("                ").Append(prop.Name)
                    .Append(" = hto.").Append(prop.OriginalName).AppendLine(",");
            }
            sb.AppendLine("            },");
        }

        // Initialize collections
        sb.Append("            Entities = new List<").Append(SchemaTypeNames.SirenSubEntity).AppendLine(">(),");
        sb.Append("            Actions = new List<").Append(SchemaTypeNames.SirenAction).AppendLine(">(),");
        sb.Append("            Links = new List<").Append(SchemaTypeNames.SirenLink).AppendLine(">(),");

        sb.AppendLine("        };");
        sb.AppendLine();

        // Self link — skip if the HTO already has an explicit self link property
        var hasExplicitSelfLink = metadata.Links.Any(l =>
            l.Relations.Any(r => string.Equals(r, "self", System.StringComparison.OrdinalIgnoreCase)));
        if (!hasExplicitSelfLink)
        {
            sb.AppendLine("        if (effectiveOptions.AutoSelfLink)");
            sb.AppendLine("        {");
            sb.Append("            entity.Links.Add(new ").Append(SchemaTypeNames.SirenLink)
                .AppendLine(" { Rel = new[] { \"self\" }, Href = selfRoute.Url });");
            sb.AppendLine("        }");
            sb.AppendLine();
        }

        // Links — resolve URLs at runtime, append query string, deduplicate by relations
        EmitLinkResolution(sb, metadata);

        // Actions — null-safe check, resolve route, build fields
        EmitActionResolution(sb, metadata);
        // Embedded entities — resolved → inline, unresolved → linked sub-entity
        EmitEmbeddedEntityResolution(sb, metadata);

        sb.AppendLine("        return entity;");
        sb.AppendLine("    }");
    }

    private static void EmitClassAssignment(StringBuilder sb, HtoMetadata metadata)
    {
        if (metadata.Classes.Length > 0)
        {
            sb.Append("            Class = new[] { ");
            for (int i = 0; i < metadata.Classes.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append('"').Append(EscapeString(metadata.Classes[i])).Append('"');
            }
            sb.AppendLine(" },");
        }
        else
        {
            // Fallback to type name when Classes is null (matches SirenConverter behavior)
            sb.Append("            Class = new[] { \"").Append(EscapeString(metadata.ClassName)).AppendLine("\" },");
        }
    }

    /// <summary>
    /// Emits link resolution code for all ILink properties.
    /// Handles nullable links, query string appending, media types, and deduplication by relations.
    /// </summary>
    private static void EmitActionResolution(StringBuilder sb, HtoMetadata metadata)
    {
        if (metadata.Actions.Length == 0)
        {
            return;
        }

        foreach (var action in metadata.Actions)
        {
            var classesArray = EmitStringArray(action.UserClasses);

            if (!action.IsMandatory)
            {
                // Nullable action — CanExecute check
                sb.Append("        if (hto.").Append(action.PropertyName).AppendLine("?.CanExecute() == true)");
                sb.AppendLine("        {");
                sb.Append("            SirenHelper.AddAction(entity.Actions, hto, hto.")
                    .Append(action.PropertyName).Append(", ");
                EmitActionArgs(sb, action, classesArray);
                sb.AppendLine(");");
                sb.AppendLine("        }");
            }
            else
            {
                // Mandatory action — still check CanExecute
                sb.Append("        if (hto.").Append(action.PropertyName).AppendLine("?.CanExecute() == true)");
                sb.AppendLine("        {");
                sb.Append("            SirenHelper.AddAction(entity.Actions, hto, hto.")
                    .Append(action.PropertyName).Append(", ");
                EmitActionArgs(sb, action, classesArray);
                sb.AppendLine(");");
                sb.AppendLine("        }");
            }

            sb.AppendLine();
        }
    }

    private static void EmitActionArgs(StringBuilder sb, ActionMetadata action, string classesArray)
    {
        sb.Append('"').Append(EscapeString(action.Name)).Append('"');
        sb.Append(", ");
        if (action.Title != null)
        {
            sb.Append('"').Append(EscapeString(action.Title)).Append('"');
        }
        else
        {
            sb.Append("null");
        }
        sb.Append(", ").Append(classesArray);
        sb.Append(", resolver");
    }

    private static void EmitEmbeddedEntityResolution(StringBuilder sb, HtoMetadata metadata)
    {
        if (metadata.EmbeddedEntities.Length == 0)
        {
            return;
        }

        foreach (var embedded in metadata.EmbeddedEntities)
        {
            var relArray = EmitStringArray(embedded.Relations);
            // Strip 'global::' prefix for cleaner generated code
            var targetFqn = embedded.TargetFullyQualifiedName;
            if (targetFqn.StartsWith("global::"))
            {
                targetFqn = targetFqn.Substring("global::".Length);
            }

            if (embedded.IsCollection)
            {
                EmitCollectionEmbeddedEntity(sb, embedded, relArray, targetFqn, metadata.ClassName);
            }
            else
            {
                EmitSingleEmbeddedEntity(sb, embedded, relArray, targetFqn, metadata.ClassName);
            }

            sb.AppendLine();
        }
    }

    private static void EmitSingleEmbeddedEntity(
        StringBuilder sb, EmbeddedEntityMetadata embedded, string relArray, string targetFqn, string parentFqn)
    {
        if (!embedded.IsMandatory)
        {
            // Nullable single — skip when null
            sb.Append("        if (hto.").Append(embedded.PropertyName).AppendLine(" is { } " + embedded.PropertyName + "Value)");
            sb.AppendLine("        {");
            EmitEmbeddedEntityBody(sb, embedded.PropertyName + "Value", relArray, embedded, targetFqn, "            ");
            sb.AppendLine("        }");
        }
        else
        {
            // Mandatory single — null guard
            sb.Append("        if (hto.").Append(embedded.PropertyName).AppendLine(" is null)");
            sb.AppendLine("        {");
            sb.Append("            throw new System.InvalidOperationException(\"Mandatory embedded entity '")
                .Append(EscapeString(embedded.PropertyName))
                .Append("' on '").Append(EscapeString(parentFqn))
                .AppendLine("' is null.\");");
            sb.AppendLine("        }");
            EmitEmbeddedEntityBody(sb, "hto." + embedded.PropertyName, relArray, embedded, targetFqn, "        ");
        }
    }

    private static void EmitCollectionEmbeddedEntity(
        StringBuilder sb, EmbeddedEntityMetadata embedded, string relArray, string targetFqn, string parentFqn)
    {
        if (!embedded.IsMandatory)
        {
            // Nullable collection — skip when null
            sb.Append("        if (hto.").Append(embedded.PropertyName).AppendLine(" is { } " + embedded.PropertyName + "List)");
            sb.AppendLine("        {");
            sb.Append("            foreach (var item in ").Append(embedded.PropertyName).AppendLine("List)");
            sb.AppendLine("            {");
            EmitEmbeddedEntityBody(sb, "item", relArray, embedded, targetFqn, "                ");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }
        else
        {
            // Mandatory collection — null guard
            sb.Append("        if (hto.").Append(embedded.PropertyName).AppendLine(" is null)");
            sb.AppendLine("        {");
            sb.Append("            throw new System.InvalidOperationException(\"Mandatory embedded entity '")
                .Append(EscapeString(embedded.PropertyName))
                .Append("' on '").Append(EscapeString(parentFqn))
                .AppendLine("' is null.\");");
            sb.AppendLine("        }");
            sb.Append("        foreach (var item in hto.").Append(embedded.PropertyName).AppendLine(")");
            sb.AppendLine("        {");
            EmitEmbeddedEntityBody(sb, "item", relArray, embedded, targetFqn, "            ");
            sb.AppendLine("        }");
        }
    }

    /// <summary>
    /// Emits the body that processes a single embedded entity reference:
    /// resolved → call ToSirenEmbedded() on the instance,
    /// unresolved external → SirenLinkedEntity with URI,
    /// unresolved internal → SirenLinkedEntity via resolver.
    /// </summary>
    private static void EmitEmbeddedEntityBody(
        StringBuilder sb, string varName, string relArray,
        EmbeddedEntityMetadata embedded, string targetFqn, string indent)
    {
        var classesArray = EmitStringArray(embedded.TargetClasses);

        // Access the Reference property (EmbeddedEntity<THto> has public Reference)
        sb.Append(indent).Append("var reference = ((RESTyard.AspNetCore.Hypermedia.EmbeddedEntity<")
            .Append(targetFqn).Append(">)").Append(varName).AppendLine(").Reference;");

        sb.Append(indent).AppendLine("if (reference.IsResolved())");
        sb.Append(indent).AppendLine("{");
        // Resolved — call ToSirenEmbedded() on the instance
        sb.Append(indent).Append("    var embedded = ((").Append(targetFqn)
            .AppendLine(")reference.GetInstance()!).ToSirenEmbedded(resolver, queryStringBuilder, options);");
        sb.Append(indent).Append("    embedded.Rel = ").Append(relArray).AppendLine(";");
        sb.Append(indent).AppendLine("    entity.Entities.Add(embedded);");
        sb.Append(indent).AppendLine("}");

        sb.Append(indent).AppendLine("else");
        sb.Append(indent).AppendLine("{");
        // Unresolved — resolve via route resolver and emit a linked sub-entity.
        // Note: HypermediaExternalObjectReference is NOT handled here because its constructor
        // throws (internal ExternalObject class lacks [HypermediaObject]). It is dead code in
        // SirenConverter too. For external links, use ExternalReference via Link.External() instead.
        sb.Append(indent).AppendLine("    var resolvedRoute = resolver.ReferenceToRoute(reference);");
        sb.Append(indent).AppendLine("    entity.Entities.Add(new SirenLinkedEntity");
        sb.Append(indent).AppendLine("    {");
        sb.Append(indent).Append("        Rel = ").Append(relArray).AppendLine(",");
        sb.Append(indent).Append("        Class = ").Append(classesArray).AppendLine(",");
        sb.Append(indent).AppendLine("        Href = resolvedRoute.Url,");
        sb.Append(indent).AppendLine("    });");
        sb.Append(indent).AppendLine("}");
    }

    private static void EmitLinkResolution(StringBuilder sb, HtoMetadata metadata)
    {
        if (metadata.Links.Length == 0)
        {
            return;
        }

        foreach (var link in metadata.Links)
        {
            var relArray = EmitStringArray(link.Relations);

            if (!link.IsMandatory)
            {
                sb.Append("        if (hto.").Append(link.PropertyName)
                    .AppendLine(" is { } " + link.PropertyName + "Link)");
                sb.AppendLine("        {");
                sb.Append("            SirenHelper.AddLink(entity.Links, ")
                    .Append(link.PropertyName).Append("Link, ")
                    .Append(relArray).Append(", \"").Append(EscapeString(link.PropertyName))
                    .AppendLine("\", resolver, queryStringBuilder);");
                sb.AppendLine("        }");
            }
            else
            {
                sb.Append("        SirenHelper.AddLink(entity.Links, hto.")
                    .Append(link.PropertyName).Append(", ")
                    .Append(relArray).Append(", \"").Append(EscapeString(link.PropertyName))
                    .AppendLine("\", resolver, queryStringBuilder);");
            }

            sb.AppendLine();
        }
    }

    private static string EmitStringArray(EquatableArray<string> items)
    {
        if (items.Length == 0)
        {
            return "System.Array.Empty<string>()";
        }

        var sb = new StringBuilder("new[] { ");
        for (int i = 0; i < items.Length; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('"').Append(EscapeString(items[i])).Append('"');
        }
        sb.Append(" }");
        return sb.ToString();
    }

    /// <summary>
    /// Emits a shared SirenHelper class with utility methods (AddLink, etc.) used by all
    /// per-HTO SirenExtensions classes. Emitted once per assembly when Siren = true.
    /// </summary>
    internal static string GenerateSirenHelper()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.Append("using ").Append(SchemaTypeNames.SirenModelNamespace).AppendLine(";");
        sb.Append("using ").Append(SchemaTypeNames.RouteResolverNamespace).AppendLine(";");
        sb.Append("using ").Append(SchemaTypeNames.QueryNamespace).AppendLine(";");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using RESTyard.AspNetCore.Hypermedia;");
        sb.AppendLine();
        sb.AppendLine("internal static class SirenHelper");
        sb.AppendLine("{");

        // AddLink
        sb.AppendLine("    internal static void AddLink(");
        sb.AppendLine("        IList<SirenLink> links,");
        sb.AppendLine("        ILink? link,");
        sb.AppendLine("        string[] rel,");
        sb.AppendLine("        string propertyName,");
        sb.AppendLine("        IHypermediaRouteResolver resolver,");
        sb.AppendLine("        IQueryStringBuilder queryStringBuilder)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (link is null)");
        sb.AppendLine("        {");
        sb.AppendLine("            throw new System.InvalidOperationException(");
        sb.AppendLine("                $\"Non-nullable link property '{propertyName}' is null. \" +");
        sb.AppendLine("                \"Ensure the property is initialized or marked as nullable.\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var route = resolver.ReferenceToRoute(link.Reference);");
        sb.AppendLine("        var query = link.Reference.GetQuery();");
        sb.AppendLine("        var href = route.Url + queryStringBuilder.CreateQueryString(query);");
        sb.AppendLine("        links.Add(new SirenLink");
        sb.AppendLine("        {");
        sb.AppendLine("            Rel = rel,");
        sb.AppendLine("            Href = href,");
        sb.AppendLine("            Type = route.AvailableMediaTypes.Count > 0");
        sb.AppendLine("                ? string.Join(\",\", route.AvailableMediaTypes)");
        sb.AppendLine("                : null,");
        sb.AppendLine("        });");
        sb.AppendLine("    }");

        sb.AppendLine();

        // AddAction
        sb.AppendLine("    internal static void AddAction(");
        sb.AppendLine("        IList<SirenAction> actions,");
        sb.AppendLine("        RESTyard.AspNetCore.Hypermedia.IHypermediaObject hto,");
        sb.AppendLine("        RESTyard.AspNetCore.Hypermedia.Actions.HypermediaActionBase action,");
        sb.AppendLine("        string actionName,");
        sb.AppendLine("        string? actionTitle,");
        sb.AppendLine("        string[] userClasses,");
        sb.AppendLine("        IHypermediaRouteResolver resolver)");
        sb.AppendLine("    {");
        // Resolve route — external vs internal
        sb.AppendLine("        ResolvedRoute resolvedRoute;");
        sb.AppendLine("        if (action is RESTyard.AspNetCore.Hypermedia.Actions.HypermediaExternalActionBase externalAction)");
        sb.AppendLine("        {");
        sb.AppendLine("            resolvedRoute = new ResolvedRoute(");
        sb.AppendLine("                externalAction.ExternalUri.ToString(),");
        sb.AppendLine("                externalAction.HttpMethod,");
        sb.AppendLine("                acceptableMediaType: externalAction.AcceptedMediaType);");
        sb.AppendLine("        }");
        sb.AppendLine("        else");
        sb.AppendLine("        {");
        sb.AppendLine("            resolvedRoute = resolver.ActionToRoute(hto, action);");
        sb.AppendLine("        }");
        sb.AppendLine();
        // Determine action class marker and type
        sb.AppendLine("        string classField;");
        sb.AppendLine("        string? actionType = null;");
        sb.AppendLine("        var fields = new List<SirenField>();");
        sb.AppendLine();
        sb.AppendLine("        if (action is RESTyard.AspNetCore.Hypermedia.Actions.IFileUploadConfiguration fileUploadConfig)");
        sb.AppendLine("        {");
        sb.AppendLine("            actionType = resolvedRoute.AcceptableMediaType ?? RESTyard.MediaTypes.DefaultMediaTypes.MultipartFormData;");
        sb.AppendLine("            var uploadConfig = fileUploadConfig.FileUploadConfiguration;");
        sb.AppendLine("            var fileField = new SirenField { Name = \"UploadFiles\", Type = \"file\" };");
        sb.AppendLine("            if (uploadConfig.Accept.Any())");
        sb.AppendLine("            {");
        sb.AppendLine("                fileField.Accept = string.Join(\",\", uploadConfig.Accept);");
        sb.AppendLine("            }");
        sb.AppendLine("            if (uploadConfig.MaxFileSizeBytes >= 0)");
        sb.AppendLine("            {");
        sb.AppendLine("                fileField.MaxFileSizeBytes = uploadConfig.MaxFileSizeBytes;");
        sb.AppendLine("            }");
        sb.AppendLine("            if (uploadConfig.AllowMultiple)");
        sb.AppendLine("            {");
        sb.AppendLine("                fileField.AllowMultiple = true;");
        sb.AppendLine("            }");
        sb.AppendLine("            fields.Add(fileField);");
        sb.AppendLine();
        sb.AppendLine("            if (action.TryGetParameterType(out var paramType))");
        sb.AppendLine("            {");
        sb.AppendLine("                classField = RESTyard.AspNetCore.WebApi.Formatter.ActionClasses.FileUploadActionWithParameterClass;");
        sb.AppendLine("                fields.Add(BuildParameterField(action, paramType, resolver));");
        sb.AppendLine("            }");
        sb.AppendLine("            else");
        sb.AppendLine("            {");
        sb.AppendLine("                classField = RESTyard.AspNetCore.WebApi.Formatter.ActionClasses.FileUploadActionClass;");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("        else if (action.TryGetParameterType(out var paramType))");
        sb.AppendLine("        {");
        sb.AppendLine("            actionType = resolvedRoute.AcceptableMediaType ?? RESTyard.MediaTypes.DefaultMediaTypes.ApplicationJson;");
        sb.AppendLine("            classField = RESTyard.AspNetCore.WebApi.Formatter.ActionClasses.ParameterActionClass;");
        sb.AppendLine("            fields.Add(BuildParameterField(action, paramType, resolver));");
        sb.AppendLine("        }");
        sb.AppendLine("        else");
        sb.AppendLine("        {");
        sb.AppendLine("            classField = RESTyard.AspNetCore.WebApi.Formatter.ActionClasses.ParameterLessActionClass;");
        sb.AppendLine("        }");
        sb.AppendLine();
        // Build class array: user classes + action class marker
        sb.AppendLine("        var classList = new List<string>();");
        sb.AppendLine("        classList.AddRange(userClasses);");
        sb.AppendLine("        classList.Add(classField);");
        sb.AppendLine();
        // Build the SirenAction
        sb.AppendLine("        actions.Add(new SirenAction");
        sb.AppendLine("        {");
        sb.AppendLine("            Name = actionName,");
        sb.AppendLine("            Href = resolvedRoute.Url,");
        sb.AppendLine("            Method = resolvedRoute.HttpMethod ?? \"Undefined\",");
        sb.AppendLine("            Title = actionTitle,");
        sb.AppendLine("            Class = classList,");
        sb.AppendLine("            Type = actionType,");
        sb.AppendLine("            Fields = fields.Count > 0 ? fields : null,");
        sb.AppendLine("        });");
        sb.AppendLine("    }");
        sb.AppendLine();

        // BuildParameterField — helper for JSON parameter fields
        sb.AppendLine("    private static SirenField BuildParameterField(");
        sb.AppendLine("        RESTyard.AspNetCore.Hypermedia.Actions.HypermediaActionBase action,");
        sb.AppendLine("        System.Type parameterType,");
        sb.AppendLine("        IHypermediaRouteResolver resolver)");
        sb.AppendLine("    {");
        sb.AppendLine("        var paramName = RESTyard.AspNetCore.Util.TypeExtension.BeautifulName(parameterType);");
        sb.AppendLine("        var field = new SirenField");
        sb.AppendLine("        {");
        sb.AppendLine("            Name = paramName,");
        sb.AppendLine("            Type = RESTyard.MediaTypes.DefaultMediaTypes.ApplicationJson,");
        sb.AppendLine("        };");
        sb.AppendLine();
        // Schema URL resolution — dynamic schema route keys first, then fallback
        sb.AppendLine("        object? routeKeys = action is RESTyard.AspNetCore.Hypermedia.IDynamicSchema dynamicSchema");
        sb.AppendLine("            ? dynamicSchema.SchemaRouteKeys : null;");
        sb.AppendLine("        resolver.TryGetRouteByType(parameterType, routeKeys).Match(");
        sb.AppendLine("            some: classRoute => field.Class = new[] { classRoute.Url },");
        sb.AppendLine("            none: () =>");
        sb.AppendLine("            {");
        sb.AppendLine("                var generatedUrl = resolver.RouteUrl(");
        sb.AppendLine("                    \"ActionParameterTypes\",");
        sb.AppendLine("                    new { parameterTypeName = paramName });");
        sb.AppendLine("                generatedUrl.Match(");
        sb.AppendLine("                    url => field.Class = new[] { url },");
        sb.AppendLine("                    error => throw new System.InvalidOperationException(");
        sb.AppendLine("                        $\"No route found for action parameter type '{paramName}'. \" +");
        sb.AppendLine("                        $\"Ensure 'AutoDeliverJsonSchemaForActionParameterTypes' is true in HypermediaExtensionsOptions, \" +");
        sb.AppendLine("                        $\"or register a custom route for this type. Error: {error}\"));");
        sb.AppendLine("            });");
        sb.AppendLine();
        // Prefilled values
        sb.AppendLine("        var prefilled = action.GetPrefilledParameter();");
        sb.AppendLine("        if (prefilled is string str)");
        sb.AppendLine("        {");
        sb.AppendLine("            field.Value = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(str);");
        sb.AppendLine("        }");
        sb.AppendLine("        else if (prefilled != null)");
        sb.AppendLine("        {");
        sb.AppendLine("            field.Value = prefilled;");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        return field;");
        sb.AppendLine("    }");

        sb.AppendLine();
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>
    /// Generates a per-assembly schema registry class and assembly attribute.
    /// </summary>
    internal static string GenerateRegistrySource(
        ImmutableArray<HtoMetadata> allHtos,
        string assemblyNameSafe)
    {
        var registryClassName = SchemaTypeNames.HypermediaSchemaRegistryPrefix + assemblyNameSafe;

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.Append("using ").Append(SchemaTypeNames.SchemaModelNamespace).AppendLine(";");
        sb.Append("using ").Append(SchemaTypeNames.JsonSchemaFactoryNamespace).AppendLine(";");
        sb.AppendLine();

        // Assembly attribute for runtime discovery
        sb.Append("[assembly: global::RESTyard.Schema.Model.HypermediaSchemaRegistryAttribute(typeof(")
            .Append(registryClassName).AppendLine("))]");
        sb.AppendLine();

        sb.Append("public static class ").AppendLine(registryClassName);
        sb.AppendLine("{");
        sb.Append("    public static System.Collections.Generic.IReadOnlyList<")
            .Append(SchemaTypeNames.EntityTypeSchema).Append("> GetSchemas(")
            .Append(SchemaTypeNames.IJsonSchemaFactory).AppendLine(" schemaFactory)");
        sb.AppendLine("    {");
        sb.Append("        return new ").Append(SchemaTypeNames.EntityTypeSchema).AppendLine("[]");
        sb.AppendLine("        {");

        foreach (var hto in allHtos)
        {
            var fullMapperName = string.IsNullOrEmpty(hto.Namespace)
                ? $"{hto.ClassName}Schema"
                : $"global::{hto.Namespace}.{hto.ClassName}Schema";

            if (hto.NeedsSchemaFactory)
            {
                sb.Append("            ").Append(fullMapperName).AppendLine(".GetSchema(schemaFactory),");
            }
            else
            {
                sb.Append("            ").Append(fullMapperName).AppendLine(".GetSchema(),");
            }
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    /// Sanitizes an assembly name for use as a C# identifier suffix.
    /// Replaces non-alphanumeric characters with underscores.
    /// </summary>
    /// <summary>
    /// Scans all types in the compilation for methods with [HypermediaActionEndpoint] that have ResultType set.
    /// Returns a dictionary keyed by (HtoClassName, ActionPropertyName) → (ResultSchemaName, ResultClasses).
    /// </summary>
    private static (
        ImmutableDictionary<(string HtoClassName, string ActionPropertyName), (string ResultSchemaName, ImmutableArray<string> ResultClasses)> Mappings,
        ImmutableArray<(string ResultTypeName, string HtoClassName, string ActionPropertyName)> NotHtoWarnings,
        ImmutableArray<(string ControllerName, string MethodName, string ActionPropertyName)> Missing201Warnings)
        ExtractActionResultMappings(Compilation compilation)
    {
        var builder = ImmutableDictionary.CreateBuilder<(string, string), (string, ImmutableArray<string>)>();
        var notHtoWarnings = ImmutableArray.CreateBuilder<(string, string, string)>();
        var missing201Warnings = ImmutableArray.CreateBuilder<(string, string, string)>();

        foreach (var type in GetAllTypes(compilation))
        {
            foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
            {
                foreach (var attr in member.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;
                    if (attrClass == null || !attrClass.IsGenericType)
                        continue;

                    var originalDef = attrClass.OriginalDefinition.ToDisplayString();
                    if (!originalDef.StartsWith(HypermediaActionEndpointAttributePrefix))
                        continue;

                    // Get the HTO type argument
                    var htoType = attrClass.TypeArguments[0] as INamedTypeSymbol;
                    if (htoType == null)
                        continue;

                    // Get the action property name (first constructor arg)
                    if (attr.ConstructorArguments.Length == 0 || attr.ConstructorArguments[0].Value is not string actionPropName)
                        continue;

                    // Get ResultType (named argument)
                    var resultTypeArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "ResultType");
                    var hasResultType = resultTypeArg.Key == "ResultType" && resultTypeArg.Value.Value is INamedTypeSymbol;

                    var htoClassName = htoType.Name;

                    if (hasResultType)
                    {
                        var resultType = (INamedTypeSymbol)resultTypeArg.Value.Value!;

                        // Check if ResultType has [HypermediaObject]
                        var hasHypermediaObject = resultType.GetAttributes()
                            .Any(a => a.AttributeClass?.ToDisplayString() == HypermediaObjectAttributeFullName);

                        if (!hasHypermediaObject)
                        {
                            notHtoWarnings.Add((resultType.ToDisplayString(), htoClassName, actionPropName));
                        }
                        else
                        {
                            var resultSchemaName = DeriveSchemaName(resultType.Name);
                            var resultClasses = GetTargetClasses(resultType);
                            builder[(htoClassName, actionPropName)] = (resultSchemaName, resultClasses);
                        }
                    }
                    else
                    {
                        // No ResultType — check if method has 201-related attributes
                        if (Has201ResponseAttribute(member))
                        {
                            missing201Warnings.Add((type.Name, member.Name, actionPropName));
                        }
                    }
                }
            }
        }

        // Also scan legacy HttpMethodHypermediaAction attributes for ResultType
        foreach (var type in GetAllTypes(compilation))
        {
            foreach (var member in type.GetMembers().OfType<IMethodSymbol>())
            {
                foreach (var attr in member.GetAttributes())
                {
                    var attrClass = attr.AttributeClass;
                    if (attrClass == null)
                        continue;

                    // Check if this attribute inherits from HttpMethodHypermediaAction
                    if (!InheritsFrom(attrClass, HttpMethodHypermediaActionBaseFullName))
                        continue;

                    // Get ResultType (named argument)
                    var resultTypeArg = attr.NamedArguments.FirstOrDefault(a => a.Key == "ResultType");
                    if (resultTypeArg.Key != "ResultType" || resultTypeArg.Value.Value is not INamedTypeSymbol resultType)
                        continue;

                    // Get the action type from the second constructor argument: typeof(HtoName.ActionOp)
                    if (attr.ConstructorArguments.Length < 2 || attr.ConstructorArguments[1].Value is not INamedTypeSymbol actionOpType)
                        continue;

                    var declaringType = actionOpType.ContainingType;
                    if (declaringType == null)
                        continue;

                    var htoClassName = declaringType.Name;
                    var actionName = actionOpType.Name.EndsWith("Op")
                        ? actionOpType.Name.Substring(0, actionOpType.Name.Length - 2)
                        : actionOpType.Name;

                    var hasHypermediaObject = resultType.GetAttributes()
                        .Any(a => a.AttributeClass?.ToDisplayString() == HypermediaObjectAttributeFullName);

                    if (!hasHypermediaObject)
                    {
                        notHtoWarnings.Add((resultType.ToDisplayString(), htoClassName, actionName));
                    }
                    else
                    {
                        var resultSchemaName = DeriveSchemaName(resultType.Name);
                        var resultClasses = GetTargetClasses(resultType);
                        builder[(htoClassName, actionName)] = (resultSchemaName, resultClasses);
                    }
                }
            }
        }

        return (builder.ToImmutable(), notHtoWarnings.ToImmutable(), missing201Warnings.ToImmutable());
    }

    private static bool InheritsFrom(INamedTypeSymbol type, string baseFullName)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.ToDisplayString() == baseFullName)
                return true;
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Checks if a method has a 201-related response attribute (ProducesResponseType, SwaggerResponse, etc.)
    /// by checking attribute name and constructor argument for value 201.
    /// </summary>
    private static bool Has201ResponseAttribute(IMethodSymbol method)
    {
        foreach (var attr in method.GetAttributes())
        {
            var name = attr.AttributeClass?.Name;
            if (name == null) continue;

            // Match ProducesResponseType, ProducesResponseTypeAttribute, SwaggerResponse, SwaggerResponseAttribute, etc.
            if (!name.Contains("ProducesResponseType") && !name.Contains("SwaggerResponse"))
                continue;

            // Check constructor arguments for integer value 201
            foreach (var arg in attr.ConstructorArguments)
            {
                if (arg.Value is int intVal && intVal == 201)
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(Compilation compilation)
    {
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(compilation.GlobalNamespace);

        while (stack.Count > 0)
        {
            var ns = stack.Pop();
            foreach (var type in ns.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var childNs in ns.GetNamespaceMembers())
            {
                stack.Push(childNs);
            }
        }
    }

    /// <summary>
    /// Enriches HTO action metadata with ResultType information from controller endpoint attributes.
    /// </summary>
    private static HtoMetadata EnrichActionsWithResultMappings(
        HtoMetadata metadata,
        ImmutableDictionary<(string HtoClassName, string ActionPropertyName), (string ResultSchemaName, ImmutableArray<string> ResultClasses)> resultMappings)
    {
        if (resultMappings.IsEmpty)
            return metadata;

        var enrichedActions = new List<ActionMetadata>();
        var changed = false;

        foreach (var action in metadata.Actions)
        {
            // Try to find a result mapping for this action.
            // The action's Name may differ from the property name (via [HypermediaAction(Name)]),
            // so we try both the action Name and look through all mappings for this HTO.
            if (TryFindResultMapping(metadata.ClassName, action.Name, resultMappings, out var resultSchemaName, out var resultClasses))
            {
                enrichedActions.Add(action with { ResultSchemaName = resultSchemaName, ResultClasses = new EquatableArray<string>(resultClasses) });
                changed = true;
            }
            else
            {
                enrichedActions.Add(action);
            }
        }

        return changed
            ? metadata with { Actions = new EquatableArray<ActionMetadata>(enrichedActions.ToImmutableArray()) }
            : metadata;
    }

    private static bool TryFindResultMapping(
        string htoClassName, string actionName,
        ImmutableDictionary<(string HtoClassName, string ActionPropertyName), (string ResultSchemaName, ImmutableArray<string> ResultClasses)> mappings,
        out string resultSchemaName, out ImmutableArray<string> resultClasses)
    {
        // The mapping key uses the property name on the HTO, which is the action's C# property name.
        // The action's Name might be overridden via [HypermediaAction(Name)], so also check by Name.
        foreach (var kvp in mappings)
        {
            if (kvp.Key.HtoClassName == htoClassName && kvp.Key.ActionPropertyName == actionName)
            {
                resultSchemaName = kvp.Value.ResultSchemaName;
                resultClasses = kvp.Value.ResultClasses;
                return true;
            }
        }

        resultSchemaName = "";
        resultClasses = ImmutableArray<string>.Empty;
        return false;
    }

    /// <summary>
    /// Generates an action result registry for multi-assembly support.
    /// Emits a static class with action-to-result mappings that <c>HypermediaSchemaBuilder</c>
    /// can discover and merge into the schema.
    /// </summary>
    internal static string GenerateActionResultRegistrySource(
        ImmutableDictionary<(string HtoClassName, string ActionPropertyName), (string ResultSchemaName, ImmutableArray<string> ResultClasses)> mappings,
        string assemblyNameSafe)
    {
        var registryClassName = "HypermediaActionResultRegistry_" + assemblyNameSafe;

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.Append("using ").Append(SchemaTypeNames.SchemaModelNamespace).AppendLine(";");
        sb.AppendLine();

        sb.Append("public static class ").AppendLine(registryClassName);
        sb.AppendLine("{");
        sb.AppendLine("    public static System.Collections.Generic.IReadOnlyList<ActionResultMapping> GetMappings()");
        sb.AppendLine("    {");
        sb.Append("        return new ActionResultMapping[]");
        sb.AppendLine();
        sb.AppendLine("        {");

        foreach (var kvp in mappings)
        {
            var classLiterals = string.Join(", ", kvp.Value.ResultClasses.Select(c => $"\"{EscapeString(c)}\""));
            sb.Append("            new ActionResultMapping { EntityName = \"")
                .Append(EscapeString(kvp.Key.HtoClassName))
                .Append("\", ActionName = \"")
                .Append(EscapeString(kvp.Key.ActionPropertyName))
                .Append("\", ResultName = \"")
                .Append(EscapeString(kvp.Value.ResultSchemaName))
                .Append("\", ResultClasses = new[] { ")
                .Append(classLiterals)
                .AppendLine(" } },");
        }

        sb.AppendLine("        };");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    internal static string SanitizeAssemblyName(string assemblyName)
    {
        var sb = new StringBuilder(assemblyName.Length);
        foreach (var c in assemblyName)
        {
            sb.Append(char.IsLetterOrDigit(c) ? c : '_');
        }

        return sb.ToString();
    }

    private static string EscapeString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
