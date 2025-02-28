using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Xml.Serialization;
using FunicularSwitch.Generators;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTyard.Generator.Templates.csharp_base;
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
            Description = """
                          select the template to render the schema with. Available options:
                          server
                            /csharp
                              /v4
                              /v5
                            /csharp-controller
                              /v4
                            /csharp-policies
                              /v4
                          client
                            /csharp
                              /v3
                            /typescript
                              /v0
                              
                          Example: --template server/csharp/v4
                          """
        };
        var outputFileOption = new Option<string>("--output-file")
        {
            IsRequired = true,
        };
        var namespaceOption = new Option<string>("--namespace");
        var includeFileOption = new Option<string>("--include-file");
        var includeTypeOption = new Option<IEnumerable<string>>("--include-type");
        var excludeTypeOption = new Option<IEnumerable<string>>("--exclude-type");

        var rootCommand = new RootCommand
        {
            schemaFileOption,
            templateOption,
            outputFileOption,
            namespaceOption,
            includeFileOption,
            includeTypeOption,
            excludeTypeOption,
        };
        rootCommand.Handler = CommandHandler.Create(Run);

        return new CommandLineBuilder(rootCommand);
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
        return;

        Func<T, bool> Condition<T>(Func<T, string> nameSelector) => includedTypeNames.Any()
            ? IsIncluded(nameSelector)
            : IsNotExcluded(nameSelector);

        Func<T, bool> IsIncluded<T>(Func<T, string> nameSelector) => x => includedTypeNames.Contains(nameSelector(x));

        Func<T, bool> IsNotExcluded<T>(Func<T, string> nameSelector) => x => !excludedTypeNames.Contains(nameSelector(x));

        T[] Filter<T>(IEnumerable<T> sequence, Func<T, string> nameSelector) => sequence
            .Where(Condition(nameSelector))
            .ToArray();
    }

    private static async Task RenderTemplate(
        HypermediaType schema,
        TemplateInfo templateFile,
        string outputPath,
        string? @namespace,
        string? includeFile)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<HtmlEncoder>(NullHtmlEncoder.Default);
        var sp = services.BuildServiceProvider();
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var includeContent = string.IsNullOrEmpty(includeFile) ? string.Empty : await File.ReadAllTextAsync(includeFile);

        var code = await templateFile.Match(
            scribanTemplate: sbn => RenderScribanTemplate(schema, sbn.FileInfo, @namespace, includeContent),
            razorTemplate: razor =>
                RenderRazorTemplate(schema, razor.RazorType, @namespace, sp, loggerFactory, includeContent));

        await File.WriteAllTextAsync(outputPath, code);
    }

    private static async Task<string> RenderScribanTemplate(HypermediaType schema, FileInfo templateFile, string? @namespace,
        string includeContent)
    {
        var templateContent = await File.ReadAllTextAsync(templateFile.FullName);
        var template = Template.Parse(templateContent, templateFile.FullName);

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
        return code;
        string GetMemberName(MemberInfo member) => member.Name;
    }

    private static async Task<string> RenderRazorTemplate(HypermediaType schema, Type componentType, string? @namespace,
        ServiceProvider sp, ILoggerFactory loggerFactory, string includeContent)
    {
        await using var renderer = new HtmlRenderer(sp, loggerFactory);

        var csharp = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var dictionary = new Dictionary<string, object?>()
            {
                [nameof(ITemplateBase.Schema)] = schema,
                [nameof(ITemplateBase.Namespace)] = @namespace,
                [nameof(ITemplateBase.Includes)] = includeContent,
            };
            var parameters = ParameterView.FromDictionary(dictionary);
            var output = await renderer.RenderComponentAsync(componentType, parameters);

            return output.ToHtmlString();
        });
        csharp = string.Join(Environment.NewLine,
            csharp.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line)));
        var tree = CSharpSyntaxTree.ParseText(csharp);
        var root = await tree.GetRootAsync();
        return root.NormalizeWhitespace().ToFullString();
    }

    private static async Task Run(
        string schemaFile,
        string template,
        string outputFile,
        IEnumerable<string> includeType,
        IEnumerable<string> excludeType,
        string? @namespace = default,
        string? includeFile = default)
    {
        await using var schemaFileStream = File.OpenRead(schemaFile);
        var schemaSerializer = new XmlSerializer(typeof(HypermediaType));
        if (schemaSerializer.Deserialize(schemaFileStream) is not HypermediaType schema)
            throw new InvalidOperationException("Failed to read schema.");
        NormalizeSchema(schema);

        var templateFile = TryGetTemplateInfo(template);
        if (templateFile is null)
        {
            throw new FileNotFoundException($"Template \"{template}\" could not be found.");
        }

        FilterTypes(schema, includeType.ToList(), excludeType.ToList());
        await RenderTemplate(schema, templateFile, outputFile, @namespace, includeFile);
        
        Console.WriteLine("Done.");
    }

    private static TemplateInfo? TryGetTemplateInfo(string template)
    {
        var installedScribanTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", $"{template}.sbn");
        if (File.Exists(installedScribanTemplatePath))
        {
            return TemplateInfo.ScribanTemplate(new FileInfo(installedScribanTemplatePath));
        }

        if (File.Exists(template))
        {
            return TemplateInfo.ScribanTemplate(new FileInfo(template));
        }

        return template.Split('/', '\\') switch
        {
            ["server", "csharp", "v5"] => TemplateInfo.RazorTemplate(typeof(Templates.server.csharp.V5)),
            _ => null,
        };
    }

    private static void NormalizeSchema(HypermediaType schema)
    {
        schema.Documents ??= [];
        foreach (var schemaDocument in schema.Documents)
        {
            schemaDocument.Classifications ??= [];
            schemaDocument.Entities ??= [];
            schemaDocument.Links ??= [];
            foreach (var link in schemaDocument.Links)
            {
                link.QueryParameters ??= [];
                link.ResultDocuments ??= [];
            }
            schemaDocument.Operations ??= [];
            schemaDocument.Properties ??= [];
        }

        schema.TransferParameters.Parameters ??= [];
        foreach (var parameter in schema.TransferParameters.Parameters)
        {
            parameter.Property ??= [];
        }

        schema.TransferParameters.ExternalParameters ??= [];
    }
}

[UnionType(CaseOrder = CaseOrder.AsDeclared)]
internal abstract partial record TemplateInfo
{
    public sealed record ScribanTemplate_(FileInfo FileInfo) : TemplateInfo;

    public sealed record RazorTemplate_(Type RazorType) : TemplateInfo;
}