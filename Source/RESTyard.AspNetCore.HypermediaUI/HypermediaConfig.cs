using System.Collections.Generic;

namespace RESTyard.AspNetCore.HypermediaUI;

public record HypermediaConfig
{
    public HypermediaConfig()
    {
    }

    public HypermediaConfig(
        bool DisableDeveloperControls,
        bool OnlyAllowConfiguredEntryPoints,
        List<ConfiguredEntryPoint> ConfiguredEntryPoints)
    {
        this.DisableDeveloperControls = DisableDeveloperControls;
        this.OnlyAllowConfiguredEntryPoints = OnlyAllowConfiguredEntryPoints;
        this.ConfiguredEntryPoints = ConfiguredEntryPoints;
    }

    public required bool DisableDeveloperControls { get; init; }
    public required bool OnlyAllowConfiguredEntryPoints { get; init; }
    public required List<ConfiguredEntryPoint> ConfiguredEntryPoints { get; init; }
}