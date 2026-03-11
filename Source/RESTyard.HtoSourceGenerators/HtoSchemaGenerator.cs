using Microsoft.CodeAnalysis;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Incremental source generator that analyzes HTO (HypermediaTypedObject) classes
/// and emits <c>GetSchema()</c> methods producing <c>EntityTypeSchema</c> instances,
/// as well as per-assembly schema registries.
/// </summary>
[Generator]
public class HtoSchemaGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Skeleton — to be filled in Steps 2.2+
    }
}
