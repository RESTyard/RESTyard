using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RESTyard.Client.Hypermedia.Commands;

public interface IHypermediaFileUploadParameter
{
    public IReadOnlyList<FileDefinition> FileDefinitions { get; }

    public object? ParameterObject { get; }
}

public record FileDefinition(Func<CancellationToken, Task<Stream>> OpenReadStreamAsync, string Name, string FileName);