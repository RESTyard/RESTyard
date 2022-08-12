using System;
using System.Reflection;
using System.Xml.Serialization;
using Scriban;
using Scriban.Runtime;

namespace RESTyard.Generator;

public static class Program
{
    public static async Task Main(string schemaFile, string type, string language, string template, string outputFile, string? @namespace = default, string? includeFile = default)
    {
        await using var schemaFileStream = File.OpenRead(schemaFile);
        var schemaSerializer = new XmlSerializer(typeof(HypermediaType));
        if(schemaSerializer.Deserialize(schemaFileStream) is not HypermediaType schema)
            throw new InvalidOperationException("Failed to read schema.");

        var templatePath = string.IsNullOrEmpty(template) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", type, $"{language}.sbn") : template;
        await RenderTemplate(schema, templatePath, outputFile, @namespace, includeFile);
        Console.WriteLine("Done.");
    }

    private static async Task RenderTemplate(HypermediaType schema, string templatePath, string outputPath, string? @namespace, string? includeFile)
    {
        var template = Template.Parse(await File.ReadAllTextAsync(templatePath), templatePath);
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

        string GetMemberName (MemberInfo member) => member.Name;
    }
}