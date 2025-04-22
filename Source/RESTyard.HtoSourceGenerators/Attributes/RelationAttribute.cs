using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTyard.HtoSourceGenerators.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RelationAttribute : Attribute, IHypermediaAttribute
{
    public RelationAttribute(params string[] relations)
    {
        if (relations.Length == 0)
        {
            throw new ArgumentException("Relations must have at least one relation", nameof(relations));
        }

        if (relations.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Relation cannot be null or empty.", nameof(relations));
        }

        this.Relations = relations.ToList();
    }
    
    public List<string> Relations { get; set; }
}