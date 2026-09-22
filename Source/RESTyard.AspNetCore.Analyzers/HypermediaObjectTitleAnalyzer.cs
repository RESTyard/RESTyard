using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RESTyard.AspNetCore.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class HypermediaObjectTitleAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RY0002";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Move HypermediaObject title to SirenTitle",
        messageFormat: "Move the HypermediaObject Title attribute argument to a SirenTitle property",
        category: "Migration",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax { Parent: AttributeListSyntax { Parent: ClassDeclarationSyntax } } attribute)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol { ContainingType: { } attributeType })
        {
            return;
        }

        if (attributeType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != "global::RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaObjectAttribute")
        {
            return;
        }

        var titleArgument = attribute.ArgumentList?.Arguments
            .FirstOrDefault(argument => argument.NameEquals?.Name.Identifier.ValueText == "Title");

        if (titleArgument is not null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, titleArgument.GetLocation()));
        }
    }
}
