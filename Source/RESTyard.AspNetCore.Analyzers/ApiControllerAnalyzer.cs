using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RESTyard.AspNetCore.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ApiControllerAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RY0001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Hypermedia endpoint not within ApiController",
        messageFormat: "Class {0} has Hypermedia endpoint but is not annotated with ApiController", 
        category: "Issue",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
    
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
    }

    private void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
        {
            return;
        }

        var assemblyAttributes = context.SemanticModel.Compilation.Assembly.GetAttributes();
        var hasAssemblyApiControllerAttribute = assemblyAttributes.Any(a => a.AttributeClass!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Microsoft.AspNetCore.Mvc.ApiControllerAttribute");
        if (hasAssemblyApiControllerAttribute)
        {
            return;
        }

        var type = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (type is null)
        {
            return;
        }
        var isController = EnumerateBaseTypes(type).Any(t =>
        {
            var displayString = t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return displayString is "global::Microsoft.AspNetCore.Mvc.ControllerBase";
        });

        if (!isController)
        {
            return;
        }

        var hasApiControllerAttribute = type.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
            "global::Microsoft.AspNetCore.Mvc.ApiControllerAttribute");

        if (hasApiControllerAttribute)
        {
            return;
        }

        var methods = classDeclarationSyntax.Members.AsEnumerable()
            .OfType<MethodDeclarationSyntax>()
            .Any(m => m.AttributeLists
                .Any(a => a.Attributes
                    .Any(aa => aa.Name is GenericNameSyntax
                    {
                        Identifier.ValueText: "HypermediaObjectEndpoint" or "HypermediaObjectEndpointAttribute"
                            or "HypermediaActionEndpoint" or "HypermediaActionEndpointAttribute"
                            or "HypermediaActionParameterInfoEndpoint" or "HypermediaActionParameterInfoEndpointAttribute"
                    })));

        if (methods)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                classDeclarationSyntax.Identifier.GetLocation(),
                classDeclarationSyntax.Identifier.Text);
            
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static IEnumerable<ITypeSymbol> EnumerateBaseTypes(ITypeSymbol? type)
    {
        if (type is null)
        {
            yield break;
        }

        while (type.BaseType is not null)
        {
            yield return type.BaseType;
            type = type.BaseType;
        }
    }
}