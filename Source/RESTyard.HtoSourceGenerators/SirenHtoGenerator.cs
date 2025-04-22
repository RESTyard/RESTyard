using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RESTyard.HtoSourceGenerators.Attributes;
using RESTyard.HtoSourceGenerators.Siren;

namespace RESTyard.HtoSourceGenerators;

[Generator(LanguageNames.CSharp)]
public class SirenHtoGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var htoClasses = context.SyntaxProvider.CreateSyntaxProvider(
                (p, _) => p is ClassDeclarationSyntax,
                (syntaxContext, _) =>
                {
                    var classNode = (ClassDeclarationSyntax)syntaxContext.Node;
                    var declaredSymbol = syntaxContext.SemanticModel.GetDeclaredSymbol(classNode);
                    if (declaredSymbol!.GetAttributes().Any(a =>
                        {
                            if (a.AttributeClass == null) {return false;}
                            
                            var name = a.AttributeClass.Name;
                            // it "Attribute" is optional in syntax
                            return name == nameof(HypermediaObjectAttribute)
                                   || name == nameof(HypermediaObjectAttribute).Replace("Attribute", string.Empty);
                        }))
                    {
                        return classNode;
                    }

                    return null;
                })
            .Where(classDeclarationSyntax => classDeclarationSyntax != null)
            .Select((a,_) => a!);
        
        var compilationAndClasses = context.CompilationProvider.Combine(htoClasses.Collect());

        context.RegisterSourceOutput(
            compilationAndClasses, 
            static (spc, source) => Execute(source.Left, source.Right, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> htoClasses, SourceProductionContext sourceProductionContext)
    {
        var htoInfos = htoClasses
            .Select(hto => HtoAnalyser.ExtractHtoInfo(compilation, hto));

        SirenHtoWriter.WriteAttributes(sourceProductionContext);
        foreach (var htoInfo in htoInfos)
        {
            SirenHtoWriter.AddSirenHtoSource(sourceProductionContext, htoInfo);
        }
    }
}