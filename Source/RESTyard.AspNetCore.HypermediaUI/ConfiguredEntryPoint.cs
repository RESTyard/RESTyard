using System;

namespace RESTyard.AspNetCore.HypermediaUI;

public record ConfiguredEntryPoint(string alias, string title, Uri entryPointUri);