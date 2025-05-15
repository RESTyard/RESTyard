using System.Reflection;
using Scriban.Runtime;
using Scriban;

namespace RESTyard.Generator
{
    public static class ScribanTemplate
    {
        public static async Task<string> Render(HypermediaType schema, FileInfo templateFile, string? @namespace,
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
            static string GetMemberName(MemberInfo member) => member.Name;
        }
    }
}
