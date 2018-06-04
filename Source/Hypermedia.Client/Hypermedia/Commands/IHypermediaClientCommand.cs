using System;
using System.Collections.Generic;
using HypermediaClient.Resolver;

namespace HypermediaClient.Hypermedia.Commands
{
    public interface IHypermediaClientCommand
    {
        string Name { get; set; }

        string Title { get; set; }

        string Method { get; set; }

        Uri Uri { get; set; }

        bool CanExecute { get; set; }
        
        bool HasResultLink { get; set; }

        bool HasParameters { get; set; }

        List<ParameterDescription> ParameterDescriptions { get; }

        IHypermediaResolver Resolver { get; set; }
    }
}