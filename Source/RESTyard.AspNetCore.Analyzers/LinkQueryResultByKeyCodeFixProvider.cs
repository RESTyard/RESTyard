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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RESTyard.AspNetCore.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LinkQueryResultByKeyCodeFixProvider)), Shared]
public class LinkQueryResultByKeyCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        [LinkQueryResultByKeyAnalyzer.DiagnosticId];

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnosticNode = root?.FindNode(diagnosticSpan);

        if (diagnosticNode is not InvocationExpressionSyntax invocationExpressionSyntax)
        {
            return;
        }

        if (invocationExpressionSyntax.Expression is not MemberAccessExpressionSyntax)
        {
            return;
        }
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use Link.ByQuery",
                equivalenceKey: LinkQueryResultByKeyAnalyzer.DiagnosticId,
                createChangedDocument: c => UseLinkByQuery(context.Document, invocationExpressionSyntax, c)),
            diagnostic);
    }

    private async Task<Document> UseLinkByQuery(
        Document document,
        InvocationExpressionSyntax invocationExpressionSyntax,
        CancellationToken cancellationToken)
    {
        var st = await document.GetSyntaxTreeAsync(cancellationToken);

        var accessExpressionSyntax = (MemberAccessExpressionSyntax)invocationExpressionSyntax.Expression;
        var newMemberAccess = accessExpressionSyntax.WithName(accessExpressionSyntax.Name.WithIdentifier(Identifier("ByQuery")));
        var argumentList = invocationExpressionSyntax.ArgumentList;
        var newArgument = Argument(LiteralExpression(SyntaxKind.NullLiteralExpression))
            .WithNameColon(NameColon(IdentifierName("query"))
                .WithColonToken(Token(TriviaList(), SyntaxKind.ColonToken,
                    TriviaList(Comment(" /* TODO: provide the query */ ")))));
        var newArgumentList =
            argumentList.WithArguments(argumentList.Arguments.Insert(0, newArgument));
        var newExpression = invocationExpressionSyntax
            .WithExpression(newMemberAccess)
            .WithArgumentList(newArgumentList);
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        editor.ReplaceNode(invocationExpressionSyntax, newExpression);
        return editor.GetChangedDocument();
    }
}