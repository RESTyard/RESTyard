using System;
using System.Text.Json;

namespace RESTyard.AspNetCore.JsonSchema;

/// <summary>
/// Defines what is needed for automatic schema generation.
/// </summary>
public interface IJsonSchemaFactory
{
    /// <summary>
    /// Called by teh endpoint for automatic schema generation to retrieve a schema for given type. 
    /// </summary>
    JsonDocument Generate(Type type);
}