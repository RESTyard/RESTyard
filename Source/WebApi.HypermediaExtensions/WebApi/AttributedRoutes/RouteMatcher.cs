using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace RESTyard.WebApi.Extensions.WebApi.AttributedRoutes
{
    public class RouteMatcher
    {
        public bool TryMatch(string routeTemplate, string requestPath, out RouteValueDictionary values)
        {
            var template = TemplateParser.Parse(routeTemplate);

            var matcher = new TemplateMatcher(template, this.GetDefaults(template));

            values = new RouteValueDictionary();
            return matcher.TryMatch(requestPath, values);
        }

        // This method extracts the default argument values from the template.
        private RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
        {
            var result = new RouteValueDictionary();

            foreach (var parameter in parsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    result.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return result;
        }
    }
}