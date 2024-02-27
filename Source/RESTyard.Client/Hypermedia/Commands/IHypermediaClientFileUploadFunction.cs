namespace RESTyard.Client.Hypermedia.Commands;

public interface IHypermediaClientFileUploadFunction<TResultType>
    : IHypermediaClientCommand
    where TResultType : HypermediaClientObject
{
}

public interface IHypermediaClientFileUploadFunction<TResultType, TParameters>
    : IHypermediaClientCommand
    where TResultType : HypermediaClientObject
{
}