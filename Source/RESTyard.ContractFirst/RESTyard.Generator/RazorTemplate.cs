using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RESTyard.Generator.Templates.csharp_base;

namespace RESTyard.Generator
{
    public class RazorTemplate
    {
        public static async Task<string> Render(HypermediaType schema, Type componentType, string? @namespace, string includeContent)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<HtmlEncoder>(NullHtmlEncoder.Default);
            var sp = services.BuildServiceProvider();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

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
            return csharp;
        }
    }
}
