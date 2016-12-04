using System;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Links
{
    /// <summary>
    /// A reference to an <see cref="HypermediaObject"/> where the Type and (if required for the <see cref="HypermediaObject"/>) key are known.
    /// Allows to referenc an <see cref="HypermediaObject"/> without creating it for reference purpose only.
    /// </summary>
    public class HypermediaObjectKeyReference : HypermediaObjectReferenceBase
    {
        private readonly object key;

        public HypermediaObjectKeyReference(Type hypermediaObjectType, object key = null) : base(hypermediaObjectType)
        {
            this.key = key;
        }

        public object GetKey()
        {
            return this.key;
        }
    }

}