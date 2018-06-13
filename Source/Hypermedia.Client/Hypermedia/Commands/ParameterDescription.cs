namespace Hypermedia.Client.Hypermedia.Commands
{
    using System.Collections.Generic;

    public class ParameterDescription
    {
        public string Name { get; set; }
        public string Type { get; set; }
        // public string Value { get; set; } // todo but what type?
        public List<string> Classes { get; set; }
    }
}