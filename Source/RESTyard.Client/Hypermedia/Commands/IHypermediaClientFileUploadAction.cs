namespace RESTyard.Client.Hypermedia.Commands;

public interface IHypermediaClientFileUploadAction<TParameters>
    : IHypermediaClientCommand
    where TParameters : IHypermediaFileUploadParameter
{
}