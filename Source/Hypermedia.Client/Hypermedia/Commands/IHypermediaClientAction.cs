using System;

namespace RESTyard.Client.Hypermedia.Commands
{
    public interface IHypermediaClientAction
        : IHypermediaClientCommand
    {
    }

    public interface IHypermediaClientAction<TParameters>
        : IHypermediaClientCommand
    {
    }
}