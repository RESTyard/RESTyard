﻿using System;
using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Link : Primary
    {
        public List<string> Relations { get; } = new List<string>();
        public Link(string relation)
        {
            Relations.Add(relation);
        }

        public Link( params string[] relations)
        {
            Relations.AddRange(relations);
        }
    }
}