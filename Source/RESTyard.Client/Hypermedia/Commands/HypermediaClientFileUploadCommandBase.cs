using System.Collections.Generic;

namespace RESTyard.Client.Hypermedia.Commands;

public abstract class HypermediaClientFileUploadCommandBase : HypermediaClientCommandBase
{
    public HypermediaClientFileUploadConfiguration Configuration { get; set; }
}

public record HypermediaClientFileUploadConfiguration(long MaxFileSizeBytes, bool AllowMultiple, IReadOnlyList<string> Accept);