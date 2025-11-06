using System;

namespace RESTyard.AspNetCore.HypermediaUI;

public record ConfiguredEntryPoint(
    string Alias,
    string Title,
    Uri EntryPointUri);