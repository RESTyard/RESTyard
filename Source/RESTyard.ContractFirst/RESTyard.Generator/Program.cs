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
        var templateOption = new Option<string>("--template")
        {
            IsRequired = true,
        };
        var outputFileOption = new Option<string>("--output-file")
        {
            IsRequired = true,
        };
        var namespaceOption = new Option<string>("--namespace");
        var includeFileOption = new Option<string>("--include-file");

        var rootCommand = new RootCommand
        {
            schemaFileOption,
            templateOption,
            outputFileOption,
            namespaceOption,
            includeFileOption,
        };
        rootCommand.Handler = CommandHandler.Create(Run);

        return new CommandLineBuilder(rootCommand);
    }

    private static async Task RenderTemplate(HypermediaType schema, FileInfo templateFile, string outputPath, string? @namespace, string? includeFile)
    {
        var templateContent = await File.ReadAllTextAsync(templateFile.FullName);
        var template = Template.Parse(templateContent, templateFile.FullName);
        var includeContent = string.IsNullOrEmpty(includeFile) ? string.Empty : await File.ReadAllTextAsync(includeFile);

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

    private static async Task Run(string schemaFile, string template, string outputFile, string? @namespace = default, string? includeFile = default)
    {
        await using var schemaFileStream = File.OpenRead(schemaFile);
        var schemaSerializer = new XmlSerializer(typeof(HypermediaType));
        if (schemaSerializer.Deserialize(schemaFileStream) is not HypermediaType schema)
            throw new InvalidOperationException("Failed to read schema.");

        var templateFile = TryGetTemplateFile(template);
        if (templateFile is null)
            throw new FileNotFoundException($"Template \"{template}\" could not be found.");

        await RenderTemplate(schema, templateFile, outputFile, @namespace, includeFile);
        Console.WriteLine("Done.");
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
