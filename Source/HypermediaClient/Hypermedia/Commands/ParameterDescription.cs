using System.Collections.Generic;

namespace HypermediaClient.Hypermedia.Commands
{
    public class ParameterDescription
    {
        public string Name { get; set; }
        public string Type { get; set; }
        // public string Value { get; set; } // todo but what type?
        public List<string> Classes { get; set; }
    }
}