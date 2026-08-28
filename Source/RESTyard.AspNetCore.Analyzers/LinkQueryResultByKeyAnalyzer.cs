using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace RESTyard.AspNetCore.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class LinkQueryResultByKeyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RY0020";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Link to QueryResultHto with ByKey method",
        messageFormat: "Linking to {0} should be done using Link.ByQuery method",
        category: "Issue",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeLink, SyntaxKind.SimpleMemberAccessExpression);
    }

    private void AnalyzeLink(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            return;
        }

        var member = memberAccessExpressionSyntax.Name;
        if (member is not { Identifier.Text: "ByKey" })
        {
            return;
        }

        if (memberAccessExpressionSyntax.Parent is not InvocationExpressionSyntax invocationExpression)
        {
            return;
        }
        
        var operation = context.SemanticModel.GetOperation(invocationExpression);
        if (operation is not IInvocationOperation invocationOperation)
        {
            return;
        }

        if (invocationOperation.TargetMethod.ContainingType.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat) != "global::RESTyard.AspNetCore.Hypermedia.Link")
        {
            return;
        }

        var typeArguments = invocationOperation.TargetMethod.TypeArguments;
        if (typeArguments.Length != 1)
        {
            return;
        }

        var typeArgument = typeArguments[0];
        if (typeArgument.AllInterfaces.Any(i =>
                i.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ==
                "global::RESTyard.AspNetCore.Hypermedia.IHypermediaQueryResult"))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                invocationExpression.GetLocation(),
                typeArgument.Name);
            
            context.ReportDiagnostic(diagnostic);
        }
    }
}