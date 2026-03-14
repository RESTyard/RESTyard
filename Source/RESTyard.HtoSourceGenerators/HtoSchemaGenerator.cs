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
            spc.AddSource(
                $"{metadata.ClassName}SirenMapper.g.cs",
                GenerateSchemaSource(metadata)));
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

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        return new HtoMetadata(
            ns,
            symbol.Name,
            DeriveSchemaName(symbol.Name),
            title,
            new EquatableArray<string>(classes),
            properties);
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

    private static bool ShouldExcludeProperty(IPropertySymbol property)
    {
        var attributes = property.GetAttributes();
        return HasAttribute(attributes, FormatterIgnoreAttributeFullName)
               || HasAttribute(attributes, RelationsAttributeFullName)
               || HasAttribute(attributes, HypermediaActionAttributeFullName);
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

        if (metadata.Properties.Length > 0)
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

        // GetSchema() accepts IJsonSchemaFactory when there are properties to resolve
        if (metadata.Properties.Length > 0)
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

    private static string EscapeString(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
