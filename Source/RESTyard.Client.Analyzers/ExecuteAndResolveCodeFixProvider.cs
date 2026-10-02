using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace RESTyard.Client.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ExecuteAndResolveCodeFixProvider)), Shared]
public class ExecuteAndResolveCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [ExecuteAndResolveAnalyzer.DiagnosticId];

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var executeInvocation = root?
            .FindToken(diagnostic.Location.SourceSpan.Start)
            .Parent?
            .FirstAncestorOrSelf<MemberAccessExpressionSyntax>()?
            .Parent as InvocationExpressionSyntax;

        if (executeInvocation is null || executeInvocation.Parent is not MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax bindInvocation })
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use ExecuteAndResolveAsync",
                equivalenceKey: ExecuteAndResolveAnalyzer.DiagnosticId,
                createChangedDocument: cancellationToken => UseExecuteAndResolveAsync(context.Document, executeInvocation, bindInvocation, cancellationToken)),
            diagnostic);
    }

    private static async Task<Document> UseExecuteAndResolveAsync(
        Document document,
        InvocationExpressionSyntax executeInvocation,
        InvocationExpressionSyntax bindInvocation,
        CancellationToken cancellationToken)
    {
        var methodName = executeInvocation.Expression.DescendantTokens().First(token => token.ValueText == "ExecuteAsync");
        var replacement = executeInvocation
            .ReplaceToken(methodName, SyntaxFactory.Identifier(methodName.LeadingTrivia, "ExecuteAndResolveAsync", methodName.TrailingTrivia))
            .WithTriviaFrom(bindInvocation);

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.ReplaceNode(bindInvocation, replacement);
        return editor.GetChangedDocument();
    }
}
