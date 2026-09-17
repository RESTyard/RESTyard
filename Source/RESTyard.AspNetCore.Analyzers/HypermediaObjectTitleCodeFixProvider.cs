using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace RESTyard.AspNetCore.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(HypermediaObjectTitleCodeFixProvider)), Shared]
public class HypermediaObjectTitleCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [HypermediaObjectTitleAnalyzer.DiagnosticId];

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var titleArgument = root?.FindNode(diagnostic.Location.SourceSpan) as AttributeArgumentSyntax;

        if (titleArgument?.Expression is null || titleArgument.FirstAncestorOrSelf<ClassDeclarationSyntax>() is not { } classDeclaration)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Move Title to SirenTitle property",
                equivalenceKey: HypermediaObjectTitleAnalyzer.DiagnosticId,
                createChangedDocument: cancellationToken => MoveTitleToProperty(context.Document, classDeclaration, titleArgument, cancellationToken)),
            diagnostic);
    }

    private static async Task<Document> MoveTitleToProperty(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        AttributeArgumentSyntax titleArgument,
        CancellationToken cancellationToken)
    {
        var property = PropertyDeclaration(NullableType(PredefinedType(Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringKeyword))), "SirenTitle")
            .WithModifiers(TokenList(Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)))
            .WithExpressionBody(ArrowExpressionClause(titleArgument.Expression.WithoutTrivia()))
            .WithSemicolonToken(Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SemicolonToken));

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        editor.RemoveNode(titleArgument);
        editor.AddMember(classDeclaration, property);
        return editor.GetChangedDocument();
    }
}
