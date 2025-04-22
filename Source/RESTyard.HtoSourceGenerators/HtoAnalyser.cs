using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RESTyard.HtoSourceGenerators.Attributes;

namespace RESTyard.HtoSourceGenerators;

public static class HtoAnalyser
{
    public static HtoInfo ExtractHtoInfo(Compilation compilation, ClassDeclarationSyntax hto)
    {
        if (compilation.GetSemanticModel(hto.SyntaxTree).GetDeclaredSymbol(hto) is not INamedTypeSymbol htoClassSymbol)
        {
            throw new Exception($"Symbol is null. Expected HTO to be a {nameof(INamedTypeSymbol)} but its not.");
        }
        
        var propertyInfos = new List<HtoPropertyInfo>();
        foreach (var propertySymbol in htoClassSymbol.GetPublicProperties())
        {
            var propertyType = (INamedTypeSymbol)propertySymbol.Type;
            if (propertySymbol.HasIgnoreAttribute(KnownTypes.IgnorePropertyAttributeTypeName))
            {
                continue;
            }

            // todo
            // if (propertyType.ImplementsInterfaceOrBaseClass(KnownTypes.HypermediaActionBaseTypeName))
            // {
            //     continue;
            // }

            var propertyOriginalName = propertySymbol.MetadataName;
            var mappedPropertyName = GetMappedName(propertySymbol);
            var propertyDocumentation = propertySymbol.GetDocumentationCommentXml();
            var attributes = propertySymbol.GetAttributes().ToList();
            //var defaultInit = propertySymbol.

            propertyInfos.Add(new HtoPropertyInfo(
                propertyOriginalName,
                mappedPropertyName,
                propertyType,
                propertyDocumentation,
                attributes));
        }

        var originalClassName = htoClassSymbol.Name;
        var usingsOfHto = htoClassSymbol.GetAllUsings();
        var namespaceOfHto = htoClassSymbol.ContainingNamespace;
        var classDocumentation = htoClassSymbol.GetDocumentationCommentXml();
        var classAttributes = htoClassSymbol.GetAttributes().ToList();

        return new HtoInfo(originalClassName, namespaceOfHto, usingsOfHto, classDocumentation, propertyInfos, classAttributes);
    }

    public static string GetMappedName(IPropertySymbol propertySymbol)
    {
        const string namePropertyOfAttribute = nameof(HypermediaPropertyAttribute.Name);

        var hypermediaPropertyAttribute = propertySymbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass != null
                                 && a.AttributeClass.FullTypeNameWithNamespace() == KnownTypes.HypermediaPropertyAttributeTypeName);
        if (hypermediaPropertyAttribute == null)
        {
            return propertySymbol.Name;
        }

        var nameArgument = hypermediaPropertyAttribute.NamedArguments
            .Where(n => n.Key == namePropertyOfAttribute)
            .ToList();

        if (!nameArgument.Any())
        {
            throw new Exception($"Expected attribute to have a property '{namePropertyOfAttribute}'");
        }

        var attributeValue = nameArgument.Single().Value.Value as string;
        if (attributeValue == null)
        {
            throw new Exception($"Expected attribute to have a string value for property '{namePropertyOfAttribute}'");
        }

        return attributeValue;
    }
}

public record HtoInfo(
    string OriginalClassName,
    INamespaceSymbol NamespaceOfHto,
    SyntaxList<UsingDirectiveSyntax> UsingsOfHto,
    string? ClassDocumentation,
    List<HtoPropertyInfo> PropertiesToMap,
    List<AttributeData> ClassAttributes);

public record HtoPropertyInfo(string OriginalName, string MappedName, INamedTypeSymbol Type, string? PropertyDocumentation, List<AttributeData> Attributes);