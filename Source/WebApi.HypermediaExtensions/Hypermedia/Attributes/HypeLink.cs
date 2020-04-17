using System;
using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypeLink : Primary
    {
        public List<string> Relations { get; } = new List<string>();
        public HypeLink(string relation)
        {
            Relations.Add(relation);
        }

        public HypeLink( params string[] relations)
        {
            Relations.AddRange(relations);
        }
        
    }
}