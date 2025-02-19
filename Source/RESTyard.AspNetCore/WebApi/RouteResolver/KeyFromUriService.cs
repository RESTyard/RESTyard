using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.WebUtilities;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Util;

namespace RESTyard.AspNetCore.WebApi.RouteResolver;

public class KeyFromUriService : IKeyFromUriService
{
    private readonly ApplicationModel applicationModel;

    public KeyFromUriService(ApplicationModel applicationModel)
    {
        this.applicationModel = applicationModel;
    }

    public TKey GetKeyFromUri<THto, TKey>(Uri uri)
        where THto : HypermediaObject
    {
        var matchers = GetTemplateMatchers<THto>();

        if (matchers.Count == 0)
        {
            throw new ArgumentException("");
        }

        RouteValueDictionary? values = null;
        if (!matchers.Any(tm => tm.TryGetValuesFromRequest(
                uri.LocalPath,
                out values)))
        {
            // TODO need to do the extra trimming here also?
            throw new ArgumentException("");
        }

        if (!string.IsNullOrEmpty(uri.Query))
        {
            var query = QueryHelpers.ParseQuery(uri.Query);
            foreach (var q in query)
            {
                string? value = q.Value.First();
                values!.Add(q.Key, value);
            }
        }

        var ctor = typeof(TKey).GetConstructors().First();
        var parameterDescriptions = ctor.GetParameters();
        var parameters = new object?[parameterDescriptions.Length];
        for (int i = 0; i < parameterDescriptions.Length; i += 1)
        {
            var value = values![parameterDescriptions[i].Name!];
            parameters[i] = Convert.ChangeType(value, parameterDescriptions[i].ParameterType);
        }

        return (TKey)ctor.Invoke(parameters);
    }

    private IImmutableList<TemplateMatcher> GetTemplateMatchers<THto>()
    {
        if (!this.applicationModel.HmoTypes.TryGetValue(typeof(THto), out var hmoType))
        {
            throw new ArgumentException($"No route found for type {typeof(THto).BeautifulName()}");
        }

        var templates = hmoType.GetHmoMethods
            .Select(m => m.RouteTemplateFull)
            .ToList();

        var matchers = templates.Select(RouteMatcher.GetTemplateMatcher).ToImmutableList();

        return matchers;
    }
}