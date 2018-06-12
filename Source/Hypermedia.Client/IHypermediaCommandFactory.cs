namespace Hypermedia.Client
{
    using System;

    using global::Hypermedia.Client.Hypermedia.Commands;

    public interface IHypermediaCommandFactory
    {
        IHypermediaClientCommand Create(Type commandInterfaceType);
    }
}