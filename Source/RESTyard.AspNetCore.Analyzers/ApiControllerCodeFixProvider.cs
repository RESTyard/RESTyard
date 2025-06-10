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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ApiControllerCodeFixProvider)), Shared]
public class ApiControllerCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(ApiControllerAnalyzer.DiagnosticId);

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnosticNode = root?.FindNode(diagnosticSpan);

        if (diagnosticNode is not ClassDeclarationSyntax cds)
        {
            return;
        }
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Add [ApiController] attribute to Controller class",
                createChangedDocument: c => AddApiControllerAttribute(context.Document, cds, c)),
            diagnostic);
    }

    private async Task<Document> AddApiControllerAttribute(
        Document document,
        ClassDeclarationSyntax cds,
        CancellationToken cancellationToken)
    {
        var st = await document.GetSyntaxTreeAsync(cancellationToken);
        var usings = (await st!.GetRootAsync(cancellationToken)).DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Where(u => u.Name is QualifiedNameSyntax {
                Left: QualifiedNameSyntax
                    {
                        Left: IdentifierNameSyntax { Identifier.ValueText: "Microsoft" },
                        Right: IdentifierNameSyntax { Identifier.ValueText: "AspNetCore" },
                    },
                Right: IdentifierNameSyntax { Identifier.ValueText: "Mvc"},
                });
        var newAttribute = AttributeList(
            SingletonSeparatedList<AttributeSyntax>(
                Attribute(
                    IdentifierName(usings.Any() ? "ApiController" : "Microsoft.AspNetCore.Mvc.ApiController"))));
        var oldList = cds.AttributeLists;
        var newList = oldList.Add(newAttribute);
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        editor.ReplaceNode(cds, cds.WithAttributeLists(newList));
        return editor.GetChangedDocument();
    }
}