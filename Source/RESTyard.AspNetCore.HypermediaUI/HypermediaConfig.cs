using System.Collections.Generic;

namespace RESTyard.AspNetCore.HypermediaUI;

public record HypermediaConfig(
    bool disableDeveloperControls,
    List<ConfiguredEntryPoint> configuredEntryPoints,
    bool onlyAllowConfiguredEntryPoints);