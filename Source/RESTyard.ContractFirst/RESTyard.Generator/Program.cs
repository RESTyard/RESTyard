using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Xml.Serialization;
using Scriban;
using Scriban.Runtime;

namespace RESTyard.Generator;

internal static class Program
{
    public static Task<int> Main(string[] args) =>
        CreateCommandLine()
            .UseDefaults()
            .Build()
            .InvokeAsync(args);

    private static CommandLineBuilder CreateCommandLine()
    {
        var schemaFileOption = new Option<string>("--schema-file")
        {
            IsRequired = true,
        };
        var options = GetAllTemplatePaths();
        var templateOption = new Option<string>("--template", $"Valid options are: {string.Join(", ", options)}")
        {
            IsRequired = true,
        };
        var outputFileOption = new Option<string>("--output-file")
        {
            IsRequired = true,
        };
        var namespaceOption = new Option<string>("--namespace");
        var includeFileOption = new Option<string>("--include-file");
        var includeTypeOption = new Option<IEnumerable<string>>("--include-type");
        var excludeTypeOption = new Option<IEnumerable<string>>("--exclude-type");
        var useDefaultClassificationsOption = new Option<bool>("--use-default-classifications",
            "Use the document name as classification if no classification is provided.");

        var rootCommand = new RootCommand
        {
            schemaFileOption,
            templateOption,
            outputFileOption,
            namespaceOption,
            includeFileOption,
            includeTypeOption,
            excludeTypeOption,
            useDefaultClassificationsOption
        };
        rootCommand.Handler = CommandHandler.Create(Run);

        return new CommandLineBuilder(rootCommand);
    }

    private static ImmutableArray<string> GetAllTemplatePaths()
    {
        var templatesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        if (!Directory.Exists(templatesDirectory))
        {
            return [];
        }

        var clientServerDirectories = new[] { "client", "server" };
        return
        [
            ..clientServerDirectories
                .SelectMany(dir => Directory.Exists(Path.Combine(templatesDirectory, dir))
                    ? Directory.GetFiles(Path.Combine(templatesDirectory, dir), "*", SearchOption.AllDirectories)
                    : [])
                .Select(filePath => Path.GetRelativePath(templatesDirectory, filePath))
        ];
    }

    private static void FilterTypes(
        HypermediaType schema,
        IReadOnlyCollection<string> includedTypeNames,
        IReadOnlyCollection<string> excludedTypeNames)
    {
        if (includedTypeNames.Any() && excludedTypeNames.Any())
            Console.WriteLine("[WARNING] Type inclusion always overrides exclusion.");

        schema.TransferParameters.Parameters = Filter(schema.TransferParameters.Parameters, x => x.typeName);
        schema.Documents = Filter(schema.Documents, x => x.name);

        Func<T, bool> Condition<T>(Func<T, string> nameSelector) => includedTypeNames.Any()
            ? IsIncluded(nameSelector)
            : IsNotExcluded(nameSelector);

        Func<T, bool> IsIncluded<T>(Func<T, string> nameSelector) => x => includedTypeNames.Contains(nameSelector(x));

        Func<T, bool> IsNotExcluded<T>(Func<T, string> nameSelector) =>
            x => !excludedTypeNames.Contains(nameSelector(x));

        T[] Filter<T>(IEnumerable<T> sequence, Func<T, string> nameSelector) => sequence
            .Where(Condition(nameSelector))
            .ToArray();
    }

    private static async Task RenderTemplate(
        HypermediaType schema,
        FileInfo templateFile,
        string outputPath,
        string? @namespace,
        string? includeFile)
    {
        var templateContent = await File.ReadAllTextAsync(templateFile.FullName);
        var template = Template.Parse(templateContent, templateFile.FullName);
        var includeContent =
            string.IsNullOrEmpty(includeFile) ? string.Empty : await File.ReadAllTextAsync(includeFile);

        var scriptObject = new ScriptObject();
        scriptObject.Import(schema, renamer: GetMemberName);
        scriptObject.Import(new
        {
            Namespace = @namespace,
            IncludeContent = includeContent,
        }, renamer: GetMemberName);
        scriptObject.Import(new CustomFunctions());

        var templateContext = new TemplateContext
        {
            MemberRenamer = GetMemberName,
            TemplateLoader = new DiskLoader(),
        };
        templateContext.PushGlobal(scriptObject);

        var code = await template.RenderAsync(templateContext);
        await File.WriteAllTextAsync(outputPath, code);

        string GetMemberName(MemberInfo member) => member.Name;
    }

    private static async Task Run(
        string schemaFile,
        string template,
        string outputFile,
        IEnumerable<string> includeType,
        IEnumerable<string> excludeType,
        string? @namespace = default,
        string? includeFile = default,
        bool useDefaultClassifications = default)
    {
        await using var schemaFileStream = File.OpenRead(schemaFile);
        var schemaSerializer = new XmlSerializer(typeof(HypermediaType));
        if (schemaSerializer.Deserialize(schemaFileStream) is not HypermediaType schema)
            throw new InvalidOperationException("Failed to read schema.");

        var templateFile = TryGetTemplateFile(template);
        if (templateFile is null)
            throw new FileNotFoundException($"Template \"{template}\" could not be found.");

        FilterTypes(schema, includeType.ToList(), excludeType.ToList());
        if (useDefaultClassifications)
        {
            AddDefaultClassificationFromDocumentName(schema);
        }

        AssertAllDocumentsHaveClassifications(schema);

        await RenderTemplate(schema, templateFile, outputFile, @namespace, includeFile);

        Console.WriteLine("Done.");
    }

    private static void AssertAllDocumentsHaveClassifications(HypermediaType schema)
    {
        var documentsWithoutClassifications = schema.Documents
            .Where(d => d.Classifications == null || d.Classifications.Length == 0)
            .Select(d => d.name)
            .ToImmutableArray();

        if (documentsWithoutClassifications.Length != 0)
        {
            throw new InvalidOperationException(
                $"The following documents must have classifications: {string.Join(", ", documentsWithoutClassifications)}");
        }
    }

    private static void AddDefaultClassificationFromDocumentName(HypermediaType schema)
    {
        foreach (var document in schema.Documents)
        {
            if ((document.Classifications?.Length ?? 0) == 0)
            {
                document.Classifications = [new ClassificationType { @class = document.name }];
            }
        }
    }

    private static FileInfo? TryGetTemplateFile(string template)
    {
        var installedTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", $"{template}.sbn");
        if (File.Exists(installedTemplatePath))
            return new FileInfo(installedTemplatePath);

        if (File.Exists(template))
            return new FileInfo(template);

        return default;
    }
}