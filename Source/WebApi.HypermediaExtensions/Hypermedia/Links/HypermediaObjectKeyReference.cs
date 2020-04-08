using System;
using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.Hypermedia.Links
{
    /// <summary>
    /// A reference to an <see cref="HypermediaObject"/> where the Type and (if required for the <see cref="HypermediaObject"/>) key are known.
    /// Allows to referenc an <see cref="HypermediaObject"/> without creating it for reference purpose only.
    /// </summary>
    public class HypermediaObjectKeyReference<T> : HypermediaObjectReferenceBase where T : HypermediaObject
    {
        private readonly object key;

        public HypermediaObjectKeyReference(object key = null) : base(typeof(T))
        {
            this.key = key;
        }

        public object GetKey()
        {
            return this.key;
        }

        public override object GetKey(IKeyProducer keyProducer)
        {
            return keyProducer.CreateFromKeyObject(this.key);
        }

        public override bool CanResolve()
        {
            return false;
        }

        public override HypermediaObject GetInstance()
        {
            return null;
        }

        public override bool IsResolved()
        {
            return false;
        }

        public override IHypermediaQuery GetQuery()
        {
            return null;
        }
    }

}