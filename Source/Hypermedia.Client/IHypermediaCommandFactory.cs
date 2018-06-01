using System;
using HypermediaClient.Hypermedia.Commands;

namespace HypermediaClient
{
    public interface IHypermediaCommandFactory
    {
        IHypermediaClientCommand Create(Type commandInterfaceType);
    }
}