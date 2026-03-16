using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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

    private const string HypermediaActionGenericFullName =
        "RESTyard.AspNetCore.Hypermedia.Actions.HypermediaAction<TParameter>";

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

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var htoTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                HypermediaObjectAttributeFullName,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => ExtractHtoMetadata(ctx))
            .Where(static m => m.HasValue)
            .Select(static (m, _) => m!.Value);

        context.RegisterSourceOutput(htoTypes, static (spc, metadata) =>
        {
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

            spc.AddSource(
                $"{metadata.ClassName}SirenMapper.g.cs",
                GenerateSchemaSource(metadata));
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

        var classes = GetNamedArgumentStringArray(attribute, "Classes");
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
            new EquatableArray<string>(classes),
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
                properties.Add(new PropertyMetadata(name, typeFullName));
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

                links.Add(new LinkMetadata(
                    new EquatableArray<string>(relations),
                    targetSchemaName,
                    new EquatableArray<string>(targetClasses),
                    isMandatory));
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
                var title = GetNamedArgumentString(actionAttr, "Title");
                var parameterTypeFullName = GetActionParameterType(member.Type);
                var isFileUpload = IsFileUploadAction(member.Type);
                var isMandatory = member.NullableAnnotation != NullableAnnotation.Annotated;

                actions.Add(new ActionMetadata(name, title, parameterTypeFullName, isFileUpload, isMandatory));
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

                embeddedEntities.Add(new EmbeddedEntityMetadata(
                    new EquatableArray<string>(relations),
                    targetSchemaName,
                    new EquatableArray<string>(targetClasses),
                    isCollection,
                    isMandatory));
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

        sb.Append("public static class ").Append(metadata.ClassName).AppendLine("SirenMapper");
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
            EmitPropertiesSchemaBuilder(sb, metadata.Properties);
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
    /// Emits code that builds a <c>JsonSchema</c> from property types using <c>IJsonSchemaFactory</c> at runtime.
    /// Generates a local variable <c>propertiesSchema</c>.
    /// </summary>
    private static void EmitPropertiesSchemaBuilder(StringBuilder sb, EquatableArray<PropertyMetadata> properties)
    {
        // Build a JSON Schema object by generating each property's schema via the factory,
        // then composing them into {"type":"object","properties":{...}}
        sb.AppendLine("        var propertySchemas = new System.Collections.Generic.Dictionary<string, JsonDocument>();");

        foreach (var prop in properties)
        {
            sb.Append("        propertySchemas[\"").Append(EscapeString(prop.Name)).Append("\"] = schemaFactory.Generate(typeof(")
                .Append(prop.TypeFullName).AppendLine("));");
        }

        sb.AppendLine("        var propertiesSchema = SchemaHelper.BuildPropertiesSchema(propertySchemas);");
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
            sb.Append("                    ").Append(SchemaTypeNames.EmbeddedEntityDescription_IsMandatory)
                .Append(" = ").Append(embedded.IsMandatory ? "true" : "false").AppendLine(",");
            sb.AppendLine("                },");
        }

        sb.AppendLine("            },");
    }

    private static string EscapeString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
