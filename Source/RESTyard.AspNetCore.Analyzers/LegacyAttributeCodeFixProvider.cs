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
            LegacyAttributeAnalyzer.HttpDeleteDiagnosticId,
            LegacyAttributeAnalyzer.HttpGetHypermediaActionParameterInfoDiagnosticId);
    
    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

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

        var document = context.Document;
        var (attributeName, httpName, newAttributeName) = diagnostic.Id switch
        {
            LegacyAttributeAnalyzer.HttpGetDiagnosticId => ("HttpGetHypermediaObject", "HttpGet",
                "HypermediaObjectEndpoint"),
            LegacyAttributeAnalyzer.HttpPostDiagnosticId => ("HttpPostHypermediaAction", "HttpPost",
                "HypermediaActionEndpoint"),
            LegacyAttributeAnalyzer.HttpPutDiagnosticId => ("HttpPutHypermediaAction", "HttpPut",
                "HypermediaActionEndpoint"),
            LegacyAttributeAnalyzer.HttpPatchDiagnosticId => ("HttpPatchHypermediaAction", "HttpPatch",
                "HypermediaActionEndpoint"),
            LegacyAttributeAnalyzer.HttpDeleteDiagnosticId => ("HttpDeleteHypermediaAction", "HttpDelete",
                "HypermediaActionEndpoint"),
            LegacyAttributeAnalyzer.HttpGetHypermediaActionParameterInfoDiagnosticId => (
                "HttpGetHypermediaActionParameterInfo", "HttpGet", "HypermediaActionParameterInfoEndpoint"),
            _ => throw new ArgumentOutOfRangeException(nameof(diagnostic.Id), diagnostic.Id, diagnostic.Id),
        };
        var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel is null)
        {
            return;
        }

        var oldAttributeList = (attribute.Parent as AttributeListSyntax)!;
        var method = (oldAttributeList.Parent as MethodDeclarationSyntax)!;
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);
        var attributeData = methodSymbol?
            .GetAttributes()
            .FirstOrDefault(a => a.ApplicationSyntaxReference!.Span == attribute.Span);
        if (attributeData is null)
        {
            return;
        }
        TypedConstant? template;
        TypedConstant routeType;
        TypedConstant? routeKeyProducer;
        ExpressionSyntax? acceptedMediaTypeExpression = attribute.ArgumentList?.Arguments
            .Where(a => a.NameEquals?.Name.Identifier.ValueText == "AcceptedMediaType")
            .Select(a => a.Expression)
            .FirstOrDefault();
        var ctor = attributeData.ConstructorArguments;
        if (diagnostic.Id == LegacyAttributeAnalyzer.HttpGetHypermediaActionParameterInfoDiagnosticId)
        {
            if (ctor.Length == 1)
            {
                template = null;
                routeType = ctor[0];
                routeKeyProducer = null;
            }
            else if (ctor.Length == 2)
            {
                template = ctor[0];
                routeType = ctor[1];
                routeKeyProducer = null;
            }
            else
            {
                return;
            }
        }
        else
        {
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
                return;
            }
        }

        AttributeSyntax? hoeAttribute =
            GetHypermediaEndpointAttribute(diagnostic.Id, semanticModel, attribute.SpanStart, routeType, routeKeyProducer, acceptedMediaTypeExpression);

        if (hoeAttribute is null)
        {
            return;
        }
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"Migrate [{attributeName}] to [{httpName}, {newAttributeName}] attributes",
                equivalenceKey: diagnostic.Id,
                createChangedDocument: c => MigrateAttribute(
                    context.Document,
                    diagnostic.Id,
                    attribute,
                    oldAttributeList,
                    template,
                    hoeAttribute,
                    method,
                    c)),
            diagnostic);
    }

    private async Task<Document> MigrateAttribute(
        Document document,
        string diagnosticId,
        AttributeSyntax attribute,
        AttributeListSyntax oldAttributeList,
        TypedConstant? template,
        AttributeSyntax hoeAttribute,
        MethodDeclarationSyntax method,
        CancellationToken cancellationToken)
    {
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
        SemanticModel semanticModel,
        int position,
        TypedConstant routeType,
        TypedConstant? routeKeyProducer,
        ExpressionSyntax? acceptedMediaTypeExpression)
    {
        var routeTypeValue = (routeType.Value as INamedTypeSymbol)!.ToMinimalDisplayString(semanticModel, position);
        var routeKeyProducerValue = (routeKeyProducer?.Value as INamedTypeSymbol)?.ToMinimalDisplayString(semanticModel, position);
        
        switch (diagnosticId)
        {
            case LegacyAttributeAnalyzer.HttpGetDiagnosticId:
                var hoeAttribute = Attribute(
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

                return hoeAttribute;
            case LegacyAttributeAnalyzer.HttpGetHypermediaActionParameterInfoDiagnosticId:
                return Attribute(
                    GenericName(
                            Identifier("HypermediaActionParameterInfoEndpoint"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(
                                    IdentifierName(routeTypeValue)))));
            default:
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

                var attributeArguments = SingletonSeparatedList<AttributeArgumentSyntax>(
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
                                                IdentifierName(property.Name))))))));
                if (acceptedMediaTypeExpression is not null)
                {
                    attributeArguments = attributeArguments.Add(
                        AttributeArgument(
                            acceptedMediaTypeExpression));
                }
                return Attribute(
                        GenericName(
                                Identifier("HypermediaActionEndpoint"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(routeTypeInfo.ContainingType.Name)))))
                    .WithArgumentList(
                        AttributeArgumentList(
                            attributeArguments));
        }
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
                LegacyAttributeAnalyzer.HttpGetHypermediaActionParameterInfoDiagnosticId => "HttpGet",
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