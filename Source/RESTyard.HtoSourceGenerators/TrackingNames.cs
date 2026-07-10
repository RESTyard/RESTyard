namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// Names passed to <c>WithTrackingName</c> on the incremental pipeline stages so
/// cacheability tests can assert <c>IncrementalStepRunReason</c> per stage
/// (via <c>GeneratorDriverOptions.TrackIncrementalGeneratorSteps</c>).
/// </summary>
internal static class TrackingNames
{
    public const string AssemblyConfig = "AssemblyConfig";
    public const string HtoTypes = "HtoTypes";
    public const string EndpointResultMappings = "EndpointResultMappings";
    public const string LegacyResultMappings = "LegacyResultMappings";
    public const string ActionResultMappings = "ActionResultMappings";
    public const string AssemblyName = "AssemblyName";
}
