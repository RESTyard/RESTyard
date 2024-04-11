using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace RESTyard.AspNetCore.JsonSchema;

public static  class JObjectExtensions
{
    public static bool TryGetNestedValue(this JObject jObject, string propertyName,  ImmutableArray<string> nestingPath,out JToken uriToken)
    {
        uriToken = jObject.SelectToken(string.Join(".", nestingPath) + "." + propertyName);
        
        return uriToken != null;
    }

    public static void RemoveNested(this JObject jObject, string propertyName, ImmutableArray<string> nestingPath)
    {
        var currentPositon = jObject;
        foreach (var nesting in nestingPath)
        {
            if (jObject[nesting] == null)
            {
                // property does not exist, do nothing
                return;
            }

            if (jObject[nesting] is JObject obj)
            {
                currentPositon = obj;
            }
            else
            {
                // wrong type, do nothing
                return;
            }
        }

        currentPositon.Remove(propertyName);
    }

    public static void SetNestedValue(
        this JObject jObject,
        string propertyName,
        ImmutableArray<string> nestingPath,
        List<string?> values,
        bool isSingleUriDeconstruction)
    {
        var currentPositon = jObject;
        foreach (var nesting in nestingPath)
        {
            if (jObject[nesting] == null)
            {
                currentPositon = new JObject();
                jObject[nesting] = currentPositon;
            }
            else
            {
                if (jObject[nesting] is JObject obj)
                {
                    currentPositon = obj;
                }
                else
                {
                    throw new Exception($"Can not set key from uri decomposition since existing property {nesting} is not a object as expected.");
                }
            }
        }

        if (isSingleUriDeconstruction)
        {
            currentPositon[propertyName] = values.Single();
        }
        else
        {
            currentPositon[propertyName] ??= new JArray(values);
        }
    }
}