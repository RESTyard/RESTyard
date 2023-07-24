using System;
using System.Collections.Generic;

namespace RESTyard.Client.Hypermedia.Commands
{
    public record ParameterDescription(string Name, string Type, IReadOnlyList<string> Classes);
}