using System;
using System.Collections.Generic;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Hypermedia.Commands
{
    public abstract class HypermediaClientCommandBase
        : IHypermediaClientCommand
    {
        public string Name { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public Uri Uri { get; set; } = new Uri("not://set");

        public bool CanExecute { get; set; }

        public bool HasResultLink { get; set; }

        public bool HasParameters { get; set; }

        public IReadOnlyList<ParameterDescription> ParameterDescriptions { get; set; } = Array.Empty<ParameterDescription>();

        public IHypermediaResolver Resolver { get; set; } = ResolverDummyObject.Instance;
    }
}