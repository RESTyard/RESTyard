using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CodeAnalysis;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Symbol → metadata analysis: turns an HTO class symbol into an <see cref="HtoMetadata"/>
/// snapshot (properties, links, actions, embedded entities, diagnostics inputs).
/// Pure analysis — no source emission.
/// </summary>
internal static class HtoMetadataExtractor
{
    internal static HtoMetadata? ExtractHtoMetadata(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol symbol)
        {
            return null;
        }

        if (!ImplementsInterface(symbol, WellKnownTypeNames.IHypermediaObjectFullName))
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
            title = GetAttributeStringArgument(symbol, WellKnownTypeNames.TitleAttributeFullName);
        }

        if (title == null)
        {
            title = GetXmlDocElement(symbol, "summary");
        }

        // Description: [Description] attribute > XML doc <remarks>
        var description = GetAttributeStringArgument(symbol, WellKnownTypeNames.DescriptionAttributeFullName);
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
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.RelationsAttributeFullName);
                if (relationsAttr == null)
                {
                    continue;
                }

                var relations = GetRelationsFromAttribute(relationsAttr);
                var targetSchemaName = DeriveSchemaName(targetType.Name);
                var targetClasses = GetTargetClasses(targetType);
                var isMandatory = member.NullableAnnotation != NullableAnnotation.Annotated;

                // Title: [Title] attribute > XML doc <summary>
                var linkTitle = GetAttributeStringArgument(member, WellKnownTypeNames.TitleAttributeFullName)
                                ?? GetXmlDocElement(member, "summary");

                // Description: [Description] attribute > XML doc <remarks>
                var linkDescription = GetAttributeStringArgument(member, WellKnownTypeNames.DescriptionAttributeFullName)
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
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaActionAttributeFullName);
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
                                  ?? GetAttributeStringArgument(member, WellKnownTypeNames.TitleAttributeFullName)
                                  ?? GetXmlDocElement(member, "summary");

                // Description: [Description] attribute > XML doc <remarks>
                var actionDescription = GetAttributeStringArgument(member, WellKnownTypeNames.DescriptionAttributeFullName)
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
                    .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.RelationsAttributeFullName);
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
                var embeddedTitle = GetAttributeStringArgument(member, WellKnownTypeNames.TitleAttributeFullName)
                                    ?? GetXmlDocElement(member, "summary");

                // Description: [Description] attribute > XML doc <remarks>
                var embeddedDescription = GetAttributeStringArgument(member, WellKnownTypeNames.DescriptionAttributeFullName)
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
               && type.OriginalDefinition.ToDisplayString() == WellKnownTypeNames.IEmbeddedEntityFullName;
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
                if (fullName == WellKnownTypeNames.HypermediaActionBaseFullName)
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
                if (fullName == WellKnownTypeNames.HypermediaActionGenericFullName
                    || fullName == WellKnownTypeNames.FileUploadHypermediaActionGenericFullName)
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
                if (fullName == WellKnownTypeNames.FileUploadHypermediaActionFullName
                    || fullName == WellKnownTypeNames.FileUploadHypermediaActionGenericFullName)
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

        return GetLinkTargetType(type);
    }

    private static bool IsILinkGeneric(INamedTypeSymbol type)
    {
        return type.IsGenericType
               && type.OriginalDefinition.ToDisplayString() == WellKnownTypeNames.ILinkFullName;
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

    internal static ImmutableArray<string> GetTargetClasses(INamedTypeSymbol targetType)
    {
        var htoAttr = targetType.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaObjectAttributeFullName);

        if (htoAttr != null)
        {
            return GetNamedArgumentStringArray(htoAttr, "Classes");
        }

        return ImmutableArray<string>.Empty;
    }

    private static bool ShouldExcludeProperty(IPropertySymbol property)
    {
        var attributes = property.GetAttributes();
        return HasAttribute(attributes, WellKnownTypeNames.FormatterIgnoreAttributeFullName)
               || HasAttribute(attributes, WellKnownTypeNames.RelationsAttributeFullName)
               || HasAttribute(attributes, WellKnownTypeNames.HypermediaActionAttributeFullName)
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
                    .Any(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.RelationsAttributeFullName);
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
                    .Any(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.RelationsAttributeFullName);
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
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaPropertyAttributeFullName);

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

    /// <summary>
    /// Reads <c>[Obsolete("message")]</c> from a symbol.
    /// Returns (true, message) if present, (false, null) otherwise.
    /// </summary>
    private static (bool IsDeprecated, string? DeprecationMessage) GetDeprecation(ISymbol symbol)
    {
        var attr = symbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.ObsoleteAttributeFullName);

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
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == WellKnownTypeNames.HypermediaAccessGroupAttributeFullName);

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
            if (WellKnownTypeNames.RestyardAttributeFullNames.Contains(fullName))
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
            return $"\"{EmitHelpers.EscapeString(s)}\"";
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
}
