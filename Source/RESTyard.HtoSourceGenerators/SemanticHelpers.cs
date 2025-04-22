using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RESTyard.HtoSourceGenerators.Attributes;

namespace RESTyard.HtoSourceGenerators
{
    public static class SemanticHelpers
    {
        public static SyntaxList<UsingDirectiveSyntax> GetAllUsings(this ITypeSymbol symbol)
        {
            // collect usings
            var allUsings = SyntaxFactory.List<UsingDirectiveSyntax>();
            foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
            {
                foreach (var parent in syntaxRef.GetSyntax().Ancestors(false))
                {
                    if (parent is NamespaceDeclarationSyntax syntax)
                    {
                        allUsings = allUsings.AddRange(syntax.Usings);
                    }
                    else if (parent is CompilationUnitSyntax unitSyntax)
                    {
                        allUsings = allUsings.AddRange(unitSyntax.Usings);
                    }
                }
            }

            return allUsings;
        }
        
        public static IList<IPropertySymbol> GetPublicProperties(this ITypeSymbol symbol)
        {
            if (symbol == null)
            {
                return new List<IPropertySymbol>();
            }
            
            var name = symbol.Name;
            var properties =  symbol
                .GetMembers()
                .Where(property => property.Kind == SymbolKind.Property && property.DeclaredAccessibility == Accessibility.Public) 
                .Cast<IPropertySymbol>()
                .ToList();

            // also get properties of base classes
            if (symbol.BaseType != null)
            {
                properties.AddRange(symbol.BaseType.GetPublicProperties());
            }

            return properties;
        }

        public static bool HasIgnoreAttribute(this IPropertySymbol propertySymbol, Type attributeType)
        {
            return propertySymbol.HasIgnoreAttribute(attributeType.Namespace);
        }
        
        public static bool HasIgnoreAttribute(this IPropertySymbol propertySymbol, string attributeTypeNameWithNamespace)
        {
            return propertySymbol.GetAttributes().Any(a => a.AttributeClass !=null &&  a.AttributeClass.FullTypeNameWithNamespace() == attributeTypeNameWithNamespace);
        }
        
        /// <summary>
        /// Gets all attributes which not derive from <see cref="IHypermediaAttribute"/>
        /// </summary>
        public static IEnumerable<AttributeData> RemoveHypermediaAttributes(this IEnumerable<AttributeData> propertySymbols)
        {
            return propertySymbols.Where(a =>
                a.AttributeClass != null &&
                !a.AttributeClass.ImplementsInterfaceOrBaseClass(typeof(IHypermediaAttribute)));
        }
        
        public static bool ImplementsInterfaceOrBaseClass(this INamedTypeSymbol typeSymbol, Type typeToCheck)
        {
            return typeSymbol.ImplementsInterfaceOrBaseClass(typeToCheck.FullName);
        }
        
        public static bool ImplementsInterfaceOrBaseClass(this INamedTypeSymbol typeSymbol, string typeNameWithNamespaceToCheck)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            if (typeSymbol.FullTypeNameWithNamespace() == typeNameWithNamespaceToCheck)
            {
                return true;
            }

            
            if (typeSymbol.BaseType != null &&
                typeSymbol.BaseType.FullTypeNameWithNamespace() == typeNameWithNamespaceToCheck || typeSymbol.BaseType!.ImplementsInterfaceOrBaseClass(typeNameWithNamespaceToCheck))
            {
                return true;
            }

            foreach (var @interface in typeSymbol.AllInterfaces)
            {
                if (@interface.FullTypeNameWithNamespace() == typeNameWithNamespaceToCheck)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static string GetOriginalAttributeCodeString(this AttributeData? attribute)
        {
            if (attribute.ApplicationSyntaxReference == null)
            {
                throw new Exception($"Can not retrieve attribute code in source generatrion for: {attribute}");
            }

            var attributeString = attribute.ApplicationSyntaxReference.SyntaxTree
                .GetText()
                .GetSubText(attribute.ApplicationSyntaxReference.Span)
                .ToString();
            return $"[{attributeString}]";
        }
        
        static readonly SymbolDisplayFormat FullTypeWithNamespaceDisplayFormat = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);
        static readonly SymbolDisplayFormat FullTypeDisplayFormat = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes);


        public static string FullTypeNameWithNamespace(this INamedTypeSymbol namedTypeSymbol) => namedTypeSymbol.ToDisplayString(FullTypeWithNamespaceDisplayFormat);
        
        public static string FullTypeName(this INamedTypeSymbol namedTypeSymbol) => namedTypeSymbol.ToDisplayString(FullTypeDisplayFormat);
        
        public static bool InheritsFrom(this INamedTypeSymbol symbol, string classNameWithNamespace)
        {
            while (true)
            {
                if (symbol.FullTypeNameWithNamespace() == classNameWithNamespace)
                {
                    return true;
                }
                if (symbol.BaseType != null)
                {
                    symbol = symbol.BaseType;
                    continue;
                }
                break;
            }
            return false;
        }

    }
}