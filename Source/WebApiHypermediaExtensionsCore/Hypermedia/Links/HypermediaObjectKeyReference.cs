using System;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

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