using System;
using RESTyard.Client.Hypermedia.Commands;

namespace RESTyard.Client
{
    public interface IHypermediaCommandFactory
    {
        IHypermediaClientCommand Create(Type commandInterfaceType);
    }
}