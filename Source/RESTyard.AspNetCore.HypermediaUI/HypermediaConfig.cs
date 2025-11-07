using System.Collections.Generic;

namespace RESTyard.AspNetCore.HypermediaUI;

public class HypermediaConfig
{
    public required bool DisableDeveloperControls { get; init; }
    public required bool OnlyAllowConfiguredEntryPoints { get; init; }
    public required List<ConfiguredEntryPoint> ConfiguredEntryPoints { get; init; }
}