namespace RESTyard.Client.Hypermedia.Commands;

public class HypermediaClientFileUploadAction
    : HypermediaClientCommandBase, IHypermediaClientAction
{
    public HypermediaClientFileUploadAction()
    {
        this.HasParameters = true;
        this.HasResultLink = false;
    }
}