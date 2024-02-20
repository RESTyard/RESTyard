using System.Collections.Generic;

namespace RESTyard.Client.Hypermedia.Commands;

public class HypermediaClientFileUploadAction
    : HypermediaClientCommandBase, IHypermediaClientFileUploadAction
{
    public HypermediaClientFileUploadAction()
    {
        this.HasParameters = false;
        this.HasResultLink = false;
    }
}

public class HypermediaClientFileUploadAction<TParameter>
    : HypermediaClientCommandBase, IHypermediaClientFileUploadAction<TParameter>
{
    public HypermediaClientFileUploadAction()
    {
        this.HasParameters = true;
        this.HasResultLink = false;
    }
}

public record HypermediaFileUploadActionParameter
    (IReadOnlyList<FileDefinition> FileDefinitions) : IHypermediaFileUploadParameter
{
    object? IHypermediaFileUploadParameter.ParameterObject => null;
}

public record HypermediaFileUploadActionParameter<TParameters>(
    IReadOnlyList<FileDefinition> FileDefinitions,
    TParameters ParameterObject) : IHypermediaFileUploadParameter
{
    object? IHypermediaFileUploadParameter.ParameterObject => this.ParameterObject;
}