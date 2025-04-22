using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RESTyard.HtoSourceGenerators.Attributes;

namespace RESTyard.HtoSourceGenerators
{
    public class HtoSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateHtos { get; } = [];
        
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                classDeclarationSyntax.AttributeLists.Any(al =>
                        al.Attributes.Any(a =>
                        {
                            var nameString = a.Name.ToString();
                            return nameString == nameof(HypermediaObjectAttribute)
                                   || nameString == nameof(HypermediaObjectAttribute).Replace("Attribute", string.Empty);
                        })))
            {
                CandidateHtos.Add(classDeclarationSyntax);
            }
        }
    }
}