using System;

namespace RESTyard.AspNetCore.HypermediaUI;

public class ConfiguredEntryPoint
{
    public required string Alias { get; init; }
    public required string Title { get; init; }
    public required Uri EntryPointUri { get; init; }
}