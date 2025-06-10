using System;
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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LegacyAttributeCodeFixProvider)), Shared]
public class LegacyAttributeCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(
            LegacyAttributeAnalyzer.HttpGetDiagnosticId,
            LegacyAttributeAnalyzer.HttpPostDiagnosticId,
            LegacyAttributeAnalyzer.HttpPutDiagnosticId,
            LegacyAttributeAnalyzer.HttpPatchDiagnosticId,
            LegacyAttributeAnalyzer.HttpDeleteDiagnosticId);
    
    public override FixAllProvider? GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.Single();

        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);

        var diagnosticNode = root?.FindNode(diagnosticSpan);

        if (diagnosticNode is not AttributeSyntax attribute)
        {
            return;
        }
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Migrate [HttpGetHypermediaObject] to [HttpGet, HypermediaObjectEndpoint] attributes",
                createChangedDocument: c => MigrateAttribute(context.Document, diagnostic.Id, attribute, c)),
            diagnostic);
    }

    private async Task<Document> MigrateAttribute(
        Document document,
        string diagnosticId,
        AttributeSyntax attribute,
        CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
        if (semanticModel is null)
        {
            return document;
        }

        var oldAttributeList = (attribute.Parent as AttributeListSyntax)!;
        var method = (oldAttributeList.Parent as MethodDeclarationSyntax)!;
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);
        var attributeData = methodSymbol?
            .GetAttributes()
            .FirstOrDefault(a => a.ApplicationSyntaxReference!.SyntaxTree == attribute.SyntaxTree);
        if (attributeData is null)
        {
            return document;
        }
        TypedConstant? template;
        TypedConstant routeType;
        TypedConstant routeKeyProducer;
        var ctor = attributeData.ConstructorArguments;
        if (ctor.Length == 2)
        {
            template = null;
            routeType = ctor[0];
            routeKeyProducer = ctor[1];
        }
        else if (ctor.Length == 3)
        {
            template = ctor[0];
            routeType = ctor[1];
            routeKeyProducer = ctor[2];
        }
        else
        {
            return document;
        }

        AttributeSyntax? hoeAttribute =
            GetHypermediaEndpointAttribute(diagnosticId, routeType, routeKeyProducer);

        if (hoeAttribute is null)
        {
            return document;
        }

        var newAttributes = oldAttributeList.Attributes
            .Remove(attribute)
            .Add(GetHttpAttribute(diagnosticId, template))
            .Add(hoeAttribute);
        
        var newAttributeList = oldAttributeList.WithAttributes(newAttributes);

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
        editor.ReplaceNode(method, method.WithAttributeLists(method.AttributeLists.Remove(oldAttributeList).Add(newAttributeList)));

        return editor.GetChangedDocument();
    }

    private static AttributeSyntax? GetHypermediaEndpointAttribute(
        string diagnosticId,
        TypedConstant routeType,
        TypedConstant routeKeyProducer)
    {
        var routeTypeValue = (routeType.Value as INamedTypeSymbol)!.Name;
        var routeKeyProducerValue = (routeKeyProducer.Value as INamedTypeSymbol)?.Name;
        
        AttributeSyntax hoeAttribute;
        if (diagnosticId == LegacyAttributeAnalyzer.HttpGetDiagnosticId)
        {
            hoeAttribute = Attribute(
                GenericName(
                        Identifier("HypermediaObjectEndpoint"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                IdentifierName(routeTypeValue)))));
            if (routeKeyProducerValue is not null)
            {
                hoeAttribute = hoeAttribute.WithArgumentList(
                    AttributeArgumentList(
                        SingletonSeparatedList<AttributeArgumentSyntax>(
                            AttributeArgument(
                                TypeOfExpression(
                                    IdentifierName(routeKeyProducerValue))))));
            }
        }
        else
        {
            var routeTypeInfo = (routeType.Value as INamedTypeSymbol)!;
            if (routeTypeInfo.ContainingType is null)
            {
                return null;
            }

            var property = routeTypeInfo.ContainingType.GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Type.Equals(routeTypeInfo, SymbolEqualityComparer.Default));
            if (property is null)
            {
                return null;
            }
            
            hoeAttribute = Attribute(
                    GenericName(
                            Identifier("HypermediaActionEndpoint"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(routeTypeInfo.ContainingType.Name)))))
                .WithArgumentList(
                    AttributeArgumentList(
                        SingletonSeparatedList<AttributeArgumentSyntax>(
                            AttributeArgument(
                                InvocationExpression(
                                        IdentifierName(
                                            Identifier(
                                                TriviaList(),
                                                SyntaxKind.NameOfKeyword,
                                                "nameof",
                                                "nameof",
                                                TriviaList())))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    MemberAccessExpression(
                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                        IdentifierName(routeTypeInfo.ContainingType.Name),
                                                        IdentifierName(property.Name))))))))));
        }

        return hoeAttribute;
    }

    private static AttributeSyntax GetHttpAttribute(string diagnosticId, TypedConstant? template)
    {
        var templateValue = template?.Value as string;
        var httpAttribute = Attribute(
            IdentifierName(diagnosticId switch
            {
                LegacyAttributeAnalyzer.HttpGetDiagnosticId => "HttpGet",
                LegacyAttributeAnalyzer.HttpPostDiagnosticId => "HttpPost",
                LegacyAttributeAnalyzer.HttpPutDiagnosticId => "HttpPut",
                LegacyAttributeAnalyzer.HttpPatchDiagnosticId => "HttpPatch",
                LegacyAttributeAnalyzer.HttpDeleteDiagnosticId => "HttpDelete",
                _ => throw new ArgumentOutOfRangeException(nameof(diagnosticId), diagnosticId, diagnosticId),
            }));
        if (templateValue is not null)
        {
            httpAttribute = httpAttribute
                .WithArgumentList(
                    AttributeArgumentList(
                        SingletonSeparatedList<AttributeArgumentSyntax>(
                            AttributeArgument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(templateValue))))));
        }

        return httpAttribute;
    }
}