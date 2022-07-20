using System;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace RESTyard.Generator;

internal class DiskLoader : ITemplateLoader
{
    string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        // NOTE: Does not work for absolute template paths at the moment
        var currentTemplatePath = Path.GetFullPath(context.CurrentSourceFile);
        var path = Path.Combine(Path.GetDirectoryName(currentTemplatePath) ?? string.Empty, templateName);
        return path;
    }

    string ITemplateLoader.Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        => Load(context, callerSpan, templatePath);

    public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        => await File.ReadAllTextAsync(templatePath);

    string ITemplateLoader.GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        => GetPath(context, callerSpan, templateName);

    string Load(TemplateContext context, SourceSpan callerSpan, string templatePath) =>
        LoadAsync(context, callerSpan, templatePath).GetAwaiter().GetResult();
}