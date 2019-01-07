using System;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;

namespace Bluehands.Hypermedia.Client
{
    public interface IHypermediaCommandFactory
    {
        IHypermediaClientCommand Create(Type commandInterfaceType);
    }
}