using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using FunicularSwitch;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.WebUtilities;
using RESTyard.AspNetCore.Extensions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Util;

namespace RESTyard.AspNetCore.WebApi.RouteResolver;

public class KeyFromUriService : IKeyFromUriService
{
    private readonly IHypermediaApiExplorer apiExplorer;

    public KeyFromUriService(IHypermediaApiExplorer apiExplorer)
    {
        this.apiExplorer = apiExplorer;
    }

    public Result<TKey> GetKeyFromUri<THto, TKey>(Uri uri)
        where THto : IHypermediaObject
    {
        var result =
            from matchers in GetTemplateMatchers<THto>()
            from values in GetValuesFromRequest(matchers, uri)
            from ctor in GetConstructor<TKey>()
            from parameters in GetParameters(ctor, values)
            from key in Invoke<TKey>(ctor, parameters)
            select key;

        return result;
    }

    private Result<IImmutableList<TemplateMatcher>> GetTemplateMatchers<THto>()
        where THto : IHypermediaObject
    {
        var templates = this.apiExplorer
            .GetFullRouteTemplateFor<THto>()
            .ToList();

        if (!templates.Any())
        {
            return Result.Error<IImmutableList<TemplateMatcher>>(
                $"No route found for type {typeof(THto).BeautifulName()}");
        }

        var matchers = templates.Select(RouteMatcher.GetTemplateMatcher).ToImmutableList();

        return Result.Ok<IImmutableList<TemplateMatcher>>(matchers);
    }

    private static Result<RouteValueDictionary> GetValuesFromRequest(IImmutableList<TemplateMatcher> matchers, Uri uri)
    {
        RouteValueDictionary? values = null;
        if (!matchers.Any(tm => tm.TryGetValuesFromRequest(uri.LocalPath, out values)))
        {
            return Result.Error<RouteValueDictionary>($"Given URI '{uri.LocalPath}' does not match any route for the requested type");
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
        
        return Result.Ok(values!);
    }

    private static Result<ConstructorInfo> GetConstructor<TKey>()
    {
        var constructor = typeof(TKey).GetConstructors().FirstOrDefault(c => c.GetParameters().Length != 0);
        if (constructor is null)
        {
            return Result.Error<ConstructorInfo>($"No suitable constructor found for Key {typeof(TKey).BeautifulName()}");
        }

        return Result.Ok(constructor);
    }

    private static Result<object?[]> GetParameters(ConstructorInfo ctor, RouteValueDictionary values)
    {
        var parameterDescriptions = ctor.GetParameters();
        var parameters = new object?[parameterDescriptions.Length];
        for (int i = 0; i < parameterDescriptions.Length; i += 1)
        {
            var parameterDescription = parameterDescriptions[i];
            if (!values.TryGetValue(parameterDescription.Name!, out var value) || value is null)
            {
                return Result.Error<object?[]>($"Parameter {parameterDescription.Name!} not present in route values");
            }

            try
            {
                parameters[i] = TypeDescriptor.GetConverter(parameterDescription.ParameterType).ConvertFrom(value);
            }
            catch (Exception e)
            {
                return Result.Error<object?[]>(
                    $"Exception thrown while attempting to convert {value} to {parameterDescription.ParameterType.BeautifulName()}: {e}");
            }
        }

        return Result.Ok(parameters);
    }

    private static Result<TKey> Invoke<TKey>(ConstructorInfo ctor, object?[] parameters)
        => Result.Try(
            () =>
            {
                var result = ctor.Invoke(parameters);
                return Result.Ok((TKey)result);
            },
            e => $"Exception while attempting to invoke constructor for type {typeof(TKey).BeautifulName()}: {e}");
}