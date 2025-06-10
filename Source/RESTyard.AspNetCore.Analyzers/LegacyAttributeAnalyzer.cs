using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RESTyard.AspNetCore.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LegacyAttributeAnalyzer : DiagnosticAnalyzer
{
    public const string HttpGetDiagnosticId = "RY0010";
    public const string HttpPostDiagnosticId = "RY0011";
    public const string HttpPutDiagnosticId = "RY0012";
    public const string HttpPatchDiagnosticId = "RY0013";
    public const string HttpDeleteDiagnosticId = "RY0014";
    public const string HttpGetHypermediaActionParameterInfoDiagnosticId = "RY0015";

    private static readonly DiagnosticDescriptor HttpGetRule = CreateDescriptor(
        id: HttpGetDiagnosticId,
        attributeName: "HttpGetHypermediaObject");

    private static readonly DiagnosticDescriptor HttpPostRule = CreateDescriptor(
        id: HttpPostDiagnosticId,
        "HttpPostHypermediaAction");

    private static readonly DiagnosticDescriptor HttpPutRule = CreateDescriptor(
        id: HttpPutDiagnosticId,
        attributeName: "HttpPutHypermediaAction");

    private static readonly DiagnosticDescriptor HttpPatchRule = CreateDescriptor(
        id: HttpPatchDiagnosticId,
        attributeName: "HttpPatchHypermediaAction");

    private static readonly DiagnosticDescriptor HttpDeleteRule = CreateDescriptor(
        id: HttpDeleteDiagnosticId,
        attributeName: "HttpDeleteHypermediaAction");

    private static readonly DiagnosticDescriptor HttpGetHypermediaActionParameterInfoRule = CreateDescriptor(
        id: HttpGetHypermediaActionParameterInfoDiagnosticId,
        attributeName: "HttpGetHypermediaActionParameterInfo");
    
    private static DiagnosticDescriptor CreateDescriptor(string id, string attributeName) => new(
        id: id,
        title: $"Use of obsolete {attributeName} attribute",
        messageFormat: $"{attributeName} attribute on endpoint {{0}} is obsolete",
        category: "Issue",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        HttpGetRule,
        HttpPostRule,
        HttpPutRule,
        HttpPatchRule,
        HttpDeleteRule,
        HttpGetHypermediaActionParameterInfoRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
    }

    private void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attributeSyntax)
        {
            return;
        }

        var name = attributeSyntax.Name;
        if (name is not IdentifierNameSyntax ins)
        {
            return;
        }
        
        var rule = ins.Identifier.ValueText switch
        {
            "HttpGetHypermediaObject" => HttpGetRule,
            "HttpPostHypermediaAction" => HttpPostRule,
            "HttpPutHypermediaAction" => HttpPutRule,
            "HttpPatchHypermediaAction" => HttpPatchRule,
            "HttpDeleteHypermediaAction" => HttpDeleteRule,
            "HttpGetHypermediaActionParameterInfo" => HttpGetHypermediaActionParameterInfoRule,
            _ => null,
        };
        if (rule is null)
        {
            return;
        }
        var parent = (MethodDeclarationSyntax)attributeSyntax.Parent!.Parent!;
        var diagnostic = Diagnostic.Create(
            rule,
            attributeSyntax.GetLocation(),
            parent.Identifier.ValueText);
            
        context.ReportDiagnostic(diagnostic);
    }
}