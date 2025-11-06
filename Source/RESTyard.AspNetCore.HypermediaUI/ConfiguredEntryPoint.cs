using System;

namespace RESTyard.AspNetCore.HypermediaUI;

public record ConfiguredEntryPoint
{
    public ConfiguredEntryPoint()
    {
    }

    public ConfiguredEntryPoint(string Alias,
        string Title,
        Uri EntryPointUri)
    {
        this.Alias = Alias;
        this.Title = Title;
        this.EntryPointUri = EntryPointUri;
    }

    public required string Alias { get; init; }
    public required string Title { get; init; }
    public required Uri EntryPointUri { get; init; }
        
}