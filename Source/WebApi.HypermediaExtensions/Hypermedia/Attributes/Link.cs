using System;
using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class Link : Attribute
    {
        public List<string> Relations { get; } = new List<string>();
        public Link(string relation)
        {
            Relations.Add(relation);
        }

        public Link(List<string> relations)
        {
            Relations.AddRange(relations);
        }
        
    }
}