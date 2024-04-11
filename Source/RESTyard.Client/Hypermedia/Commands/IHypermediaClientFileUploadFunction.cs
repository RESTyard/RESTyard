namespace RESTyard.Client.Hypermedia.Commands;

public interface IHypermediaClientFileUploadFunction<TResultType>
    : IHypermediaClientFileUploadCommand
    where TResultType : HypermediaClientObject
{
}

public interface IHypermediaClientFileUploadFunction<TResultType, TParameters>
    : IHypermediaClientFileUploadCommand
    where TResultType : HypermediaClientObject
{
}