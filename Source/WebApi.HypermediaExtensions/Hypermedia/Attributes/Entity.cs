using System;
using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Entity : Primary
    {
        public List<string> Relations { get; } = new List<string>();
        public Entity(string relation)
        {
            Relations.Add(relation);
        }

        public Entity(params string[] relations)
        {
            Relations.AddRange(relations);
        }
    }
}