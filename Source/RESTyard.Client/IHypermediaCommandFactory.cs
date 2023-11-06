using System;
using FunicularSwitch;
using RESTyard.Client.Hypermedia.Commands;

namespace RESTyard.Client
{
    public interface IHypermediaCommandFactory
    {
        Result<IHypermediaClientCommand> Create(Type commandInterfaceType);
    }
}