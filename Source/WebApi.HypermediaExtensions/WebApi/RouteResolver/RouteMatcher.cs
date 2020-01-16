using System;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace WebApi.HypermediaExtensions.WebApi.RouteResolver
{
    static class RouteMatcher
    {
        public static bool TryMatch(string routeTemplate, Uri requestPath, out RouteValueDictionary values)
        {
            var matcher = GetTemplateMatcher(routeTemplate);

            var requestLocalPath = requestPath.LocalPath;
            return TryGetValuesFromRequest(matcher, requestLocalPath, out values);
        }

        public static TemplateMatcher GetTemplateMatcher(string routeTemplate)
        {
            var template = TemplateParser.Parse(routeTemplate);
            var matcher = new TemplateMatcher(template, GetDefaults(template));
            return matcher;
        }

        public static bool TryGetValuesFromRequest(this TemplateMatcher matcher, string requestLocalPath, out RouteValueDictionary values)
        {
            values = new RouteValueDictionary();
            return matcher.TryMatch(requestLocalPath, values);
        }

        // This method extracts the default argument values from the template.
        static RouteValueDictionary GetDefaults(RouteTemplate parsedTemplate)
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

        public static string GetKeyFromRequest(string routeTemplate, string key, Uri request)
        {
            return GetKeyFromRequest(routeTemplate, key, request, s => s);
        }

        public static Guid GetGuidKeyFromRequest(string routeTemplate, string key, Uri request)
        {
            return GetKeyFromRequest(routeTemplate, key, request, Guid.Parse);
        }

        public static T GetKeyFromRequest<T>(string routeTemplate, string key, Uri request, Func<string, T> keyFromString)
        {
            if (!TryMatch(routeTemplate, request, out var dict))
            {
                throw new ArgumentException($"Unexpected uri '{request}'. Expected uri for template: {routeTemplate}");
            }

            if (!dict.TryGetValue(key, out var value))
            {
                throw new ArgumentException($"Key {key} not found in {request}");
            }

            return keyFromString((string)value);
        }
    }
}