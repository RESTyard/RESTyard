using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaObjectAttribute : Primary
    {
        /// <summary>
        /// A title, describing the object for a Human
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Classes of this object, specifies the layout of this object
        /// </summary>
        public string[] Classes { get; set; } = new string[0];

        /// <summary>
        /// If true no default self link should be generated for this object
        /// </summary>
        public bool NoDefaultSelfLink { get; set; }
    }
}
