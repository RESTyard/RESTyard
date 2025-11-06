using System.Collections.Generic;

namespace RESTyard.AspNetCore.HypermediaUI;

public record HypermediaConfig(
    bool DisableDeveloperControls,
    List<ConfiguredEntryPoint> ConfiguredEntryPoints,
    bool OnlyAllowConfiguredEntryPoints);