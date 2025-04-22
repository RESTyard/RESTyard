using System;

namespace RESTyard.HtoSourceGenerators.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaObjectAttribute : Attribute, IHypermediaAttribute
    {
        public string? Title { get; set; }
        public string[]? Classes { get; set; } // todo must be dynamic or replacable by a URI
    }
}
