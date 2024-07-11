namespace RESTyard.Client.Hypermedia.Commands;

public class HypermediaClientFileUploadFunction<TResultType>
    : HypermediaClientFileUploadCommandBase, IHypermediaClientFileUploadFunction<TResultType>
    where TResultType : HypermediaClientObject
{
    public HypermediaClientFileUploadFunction()
    {
        this.HasParameters = false;
        this.HasResultLink = true;
    }
}

public class HypermediaClientFileUploadFunction<TResultType, TParameters>
    : HypermediaClientFileUploadCommandBase, IHypermediaClientFileUploadFunction<TResultType, TParameters>
    where TResultType : HypermediaClientObject
{
    public HypermediaClientFileUploadFunction()
    {
        this.HasParameters = true;
        this.HasResultLink = true;
    }
}