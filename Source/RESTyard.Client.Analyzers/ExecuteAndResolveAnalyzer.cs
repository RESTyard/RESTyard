using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace RESTyard.Client.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExecuteAndResolveAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "RYC001";

    private static readonly DiagnosticDescriptor Rule = new(
        id: DiagnosticId,
        title: "Use ExecuteAndResolveAsync",
        messageFormat: "Use ExecuteAndResolveAsync to Opt-In to QUERY requests returning the result in-place and saving on the additional request",
        category: "Performance",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Bind" },
                ArgumentList.Arguments.Count: 1,
            } bindInvocation)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(bindInvocation).Symbol is not IMethodSymbol { Name: "Bind" })
        {
            return;
        }

        if (bindInvocation.ArgumentList.Arguments[0].Expression is not LambdaExpressionSyntax lambda
            || !ResolvesLambdaParameter(lambda, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        if (bindInvocation.Expression is not MemberAccessExpressionSyntax
            {
                Expression: InvocationExpressionSyntax executeInvocation,
            })
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(executeInvocation).Symbol is not IMethodSymbol
            {
                Name: "ExecuteAsync",
                ContainingType: { } containingType,
            }
            || containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) != "global::RESTyard.Client.Extensions.CommandExtensions")
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, ((MemberAccessExpressionSyntax)executeInvocation.Expression).Name.GetLocation()));
    }

    private static bool ResolvesLambdaParameter(LambdaExpressionSyntax lambda, SemanticModel semanticModel, System.Threading.CancellationToken cancellationToken)
    {
        if (lambda.Body is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax { Name.Identifier.ValueText: "ResolveAsync" } resolveAccess,
                ArgumentList.Arguments.Count: 0,
            })
        {
            return false;
        }

        var lambdaParameter = lambda switch
        {
            SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Parameter,
            ParenthesizedLambdaExpressionSyntax { ParameterList.Parameters.Count: 1 } parenthesizedLambda => parenthesizedLambda.ParameterList.Parameters[0],
            _ => null,
        };

        return lambdaParameter is not null &&
               semanticModel.GetSymbolInfo(resolveAccess.Expression, cancellationToken).Symbol is IParameterSymbol parameter &&
               SymbolEqualityComparer.Default.Equals(parameter, semanticModel.GetDeclaredSymbol(lambdaParameter, cancellationToken));
    }
}
