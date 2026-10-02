using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using FunicularSwitch;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using RESTyard.Client.Extensions;
using VerifyTests;
using VerifyXunit;

namespace RESTyard.Client.Analyzers.Tests;

public class VerifyAnalyzer : VerifyBase
{
    private static readonly string assemblyDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
    private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
    private static readonly MetadataReference RuntimeReference = MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "System.Runtime.dll"));
    private static readonly MetadataReference CollectionsReference = MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "System.Collections.dll"));
    private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
    private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
    private static readonly MetadataReference RestyardClientReference = MetadataReference.CreateFromFile(typeof(CommandExtensions).Assembly.Location);
    private static readonly MetadataReference FunicularSwitchReference = MetadataReference.CreateFromFile(typeof(Unit).Assembly.Location);
    private static readonly MetadataReference NetStandardReference = MetadataReference.CreateFromFile(Path.Combine(assemblyDirectory, "netstandard.dll"));

    private readonly string sourceFile;
    
    // ReSharper disable once ExplicitCallerInfoArgument
    protected VerifyAnalyzer([CallerFilePath] string sourceFile = "") : base(sourceFile: sourceFile)
    {
        this.sourceFile = sourceFile;
    }

    protected async Task Verify(
        string source,
        DiagnosticAnalyzer analyzer,
        CodeFixProvider codeFixProvider,
        Action<ImmutableArray<Diagnostic>> verifyDiagnostics,
        bool sourceMustBuild = true,
        bool fixedDocumentMustBuild = true,
        [CallerMemberName] string callingMethod = "")
    {
        const string TestProjectName = "Test";
        var projectId = ProjectId.CreateNewId(debugName: TestProjectName);
        var documentId = DocumentId.CreateNewId(projectId, debugName: "File.cs");
        var solution = new AdhocWorkspace()
            .CurrentSolution
            .AddProject(projectId, TestProjectName, TestProjectName, LanguageNames.CSharp)
            .AddMetadataReference(projectId, CorlibReference)
            .AddMetadataReference(projectId, SystemCoreReference)
            .AddMetadataReference(projectId, RuntimeReference)
            .AddMetadataReference(projectId, CollectionsReference)
            .AddMetadataReference(projectId, CSharpSymbolsReference)
            .AddMetadataReference(projectId, CodeAnalysisReference)
            .AddMetadataReference(projectId, RestyardClientReference)
            .AddMetadataReference(projectId, FunicularSwitchReference)
            .AddMetadataReference(projectId, NetStandardReference)
            .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddDocument(documentId, "File.cs", SourceText.From(source));

        var project = solution.GetProject(projectId)!;
        var compilationWithAnalyzers = (await project.GetCompilationAsync())!.WithAnalyzers([analyzer]);
        if (sourceMustBuild)
        {
            compilationWithAnalyzers.Compilation.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Should().BeEmpty();
        }

        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
        verifyDiagnostics(diagnostics);
        var document = project.Documents.First();
        using var scope = new AssertionScope();
        foreach (var tuple in diagnostics
                     .OrderBy(d => d.Location.GetLineSpan().StartLinePosition.Line)
                     .ThenBy(d => d.Location.GetLineSpan().StartLinePosition.Character)
                     .Index())
        {
            var (index, diagnostic) = tuple;
            var actions = new List<CodeAction>();
            var codeFixContext = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None);
            await codeFixProvider.RegisterCodeFixesAsync(codeFixContext);
            actions.Should().NotBeEmpty();
            var updatedDocument = await ApplyFix(document, actions[0]);
            if (fixedDocumentMustBuild)
            {
                var updatedCompilation = await updatedDocument.Project.GetCompilationAsync();
                updatedCompilation!.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            }

            var syntaxTree = await updatedDocument.GetSyntaxRootAsync();
            var updatedCode = syntaxTree!.ToFullString();
            var settings = new VerifySettings();
            settings.UseFileName($"{Path.GetFileNameWithoutExtension(this.sourceFile)}_{callingMethod}_{diagnostic.Id}_{index}.cs");
            await Verify(updatedCode, settings)
                .UseDirectory("Snapshots");
        }
    }

    private static async Task<Document> ApplyFix(Document document, CodeAction codeAction)
    {
        var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
        return operations.OfType<ApplyChangesOperation>().Single().ChangedSolution.GetDocument(document.Id)!;
    }
}
