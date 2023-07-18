using RESTyard.Client.Exceptions;
using System.Text.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTyard.Client.Extensions.SystemTextJson.Extensions;

public static class ProblemDetailsExtensions
{
    public static bool TryGetValue<TValue>(this ProblemDetails problemDetails, string key, [NotNullWhen(true)] out TValue? value)
    {
        value = default;
        if (problemDetails.Extensions.TryGetValue(key, out var obj)
            && obj is JsonElement jsonElement)
        {
            try
            {
                value = jsonElement.Deserialize<TValue>();
                return value is not null;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}