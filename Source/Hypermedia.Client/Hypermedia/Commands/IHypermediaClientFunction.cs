using System;

namespace RESTyard.Client.Hypermedia.Commands
{
    public interface IHypermediaClientFunction<TResultType>
        : IHypermediaClientCommand
        where TResultType : HypermediaClientObject
    {
    }

    public interface IHypermediaClientFunction<TResultType, TParameters>
        : IHypermediaClientCommand
        where TResultType : HypermediaClientObject
    {
    }
}