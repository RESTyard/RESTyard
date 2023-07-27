using RESTyard.Client.Exceptions;
using System.Text.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace RESTyard.Client.Extensions.SystemTextJson.Extensions;

/// <summary>
/// Extensions to work with ProblemDetails that have been deserialized using System.Text.Json
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Get a typed entity from the extensions by name
    /// </summary>
    /// <typeparam name="TValue">The desired result type</typeparam>
    /// <param name="problemDetails">The received ProblemDetails that were deserialized using System.Text.Json</param>
    /// <param name="key">The name of the Property to get from the extensions</param>
    /// <param name="value">The resulting object, if the key is contained in the extensions and it is of type <typeparamref name="TValue" /></param>
    /// <returns></returns>
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