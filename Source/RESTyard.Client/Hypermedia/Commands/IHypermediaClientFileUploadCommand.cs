namespace RESTyard.Client.Hypermedia.Commands;

public interface IHypermediaClientFileUploadCommand
    : IHypermediaClientCommand
{
    HypermediaClientFileUploadConfiguration Configuration { get; }
}