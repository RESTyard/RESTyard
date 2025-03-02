using System.Text;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Immutable;

namespace RESTyard.Generator
{
    public class CodeFormatter
    {
        public static async Task<string> Format(string code, string outputPath)
        {
            var normalized = await Normalize(code);

            var styled = await ApplyLocalEditorconfig(normalized, outputPath);

            return styled;
        }

        private static async Task<string> Normalize(string code)
        {
            var trimmed = string.Join(Environment.NewLine,
                        code.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                            .Where(line => !string.IsNullOrWhiteSpace(line)));
            var tree = CSharpSyntaxTree.ParseText(trimmed);
            var root = await tree.GetRootAsync();
            var normalized = root.NormalizeWhitespace().ToFullString();
            return normalized;
        }

        private static async Task<string> ApplyLocalEditorconfig(string code, string outputPath)
        {
            var absolutePath = Path.GetFullPath(outputPath);
            var directory = Path.GetDirectoryName(absolutePath)!;
            var editorConfigPaths = GetEditorConfigPaths(directory!);
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);
            var analyzerConfigDocuments = editorConfigPaths.Select(
                path => DocumentInfo.Create(
                    DocumentId.CreateNewId(projectId),
                    name: path,
                    loader: new FileTextLoader(path, Encoding.UTF8),
                    filePath: path));
            var sourceDocument = DocumentInfo.Create(
                documentId,
                name: absolutePath,
                loader: TextLoader.From(TextAndVersion.Create(SourceText.From(code), VersionStamp.Create())),
                filePath: absolutePath);
            var projectInfo = ProjectInfo.Create(
                projectId,
                version: default,
                name: "inmemory",
                assemblyName: directory,
                language: LanguageNames.CSharp,
                filePath: directory,
                documents: [ sourceDocument ])
                .WithAnalyzerConfigDocuments(analyzerConfigDocuments);

            var workspace = new AdhocWorkspace();
            var project = workspace.AddProject(projectInfo);
            var document = project.Documents.First(d => d.Id == documentId);
            var options = await document.GetOptionsAsync();

            var formatted = await Formatter.FormatAsync(document, options);
            return (await formatted.GetTextAsync()).ToString();
        }

        // copied from https://github.com/dotnet/format/blob/ad8125a6fc036fe1eb4e57aa604a79f2d98d5aa0/src/Utilities/EditorConfigFinder.cs#L10
        private static ImmutableArray<string> GetEditorConfigPaths(string path)
        {
            // If the path is to a file then remove the file name and process the
            // folder path.
            var startPath = Directory.Exists(path)
                ? path
                : Path.GetDirectoryName(path);

            if (!Directory.Exists(startPath))
            {
                return ImmutableArray<string>.Empty;
            }

            var editorConfigPaths = ImmutableArray.CreateBuilder<string>(16);

            var directory = new DirectoryInfo(path);

            // Find .editorconfig files contained unders the folder path.
            var files = directory.GetFiles(".editorconfig", SearchOption.AllDirectories);
            for (var index = 0; index < files.Length; index++)
            {
                editorConfigPaths.Add(files[index].FullName);
            }

            // Walk from the folder path up to the drive root addings .editorconfig files.
            while (directory.Parent != null)
            {
                directory = directory.Parent;

                files = directory.GetFiles(".editorconfig", SearchOption.TopDirectoryOnly);
                if (files.Length == 1)
                {
                    editorConfigPaths.Add(files[0].FullName);
                }
            }

            return editorConfigPaths.ToImmutable();
        }
    }
}
