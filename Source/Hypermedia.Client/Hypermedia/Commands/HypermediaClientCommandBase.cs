namespace Hypermedia.Client.Hypermedia.Commands
{
    using System;
    using System.Collections.Generic;

    using global::Hypermedia.Client.Resolver;

    public abstract class HypermediaClientCommandBase : IHypermediaClientCommand
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public string Method { get; set; }

        public Uri Uri { get; set; }

        public bool CanExecute { get; set; }

        public bool HasResultLink { get; set; }

        public bool HasParameters { get; set; }

        public List<ParameterDescription> ParameterDescriptions { get; } = new List<ParameterDescription>();

        public IHypermediaResolver Resolver { get; set; }
    }
}