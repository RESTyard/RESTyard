using System;
using System.Collections.Generic;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Hypermedia.Commands
{
    public interface IHypermediaClientCommand
    {
        string Name { get; set; }

        string Title { get; set; }

        string Method { get; set; }
        
        string Type { get; set; }

        Uri Uri { get; set; }

        bool CanExecute { get; set; }
        
        bool HasResultLink { get; set; }

        bool HasParameters { get; set; }

        IReadOnlyList<ParameterDescription> ParameterDescriptions { get; set; }

        IHypermediaResolver Resolver { get; set; }
    }
}