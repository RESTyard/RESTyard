using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Query;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Links
{
    /// <summary>
    /// Reference to a HypermediaObject. Use this if you already have a HypermediaObject.
    /// </summary>
    public class HypermediaObjectReference : HypermediaObjectReferenceBase
    {
        private readonly HypermediaObject reference;

        public HypermediaObjectReference(HypermediaObject hypermediaObject) : base (hypermediaObject.GetType())
        {
            if (hypermediaObject == null)
            {
                throw new HypermediaException("HypermediaObject is null.");
            }

            this.reference = hypermediaObject;
        }

        /// <summary>
        /// Resolves the referenced HypermediaObject
        /// </summary>
        /// <returns>The HypermediaObject.</returns>
        public override HypermediaObject GetInstance()
        {
            return this.reference;
        }

        public override bool CanResolve()
        {
            return true;
        }

        public override bool IsResolved()
        {
            return true;
        }

        public override object GetKey(IKeyProducer keyProducer)
        {
            return keyProducer.CreateFromHypermediaObject(this.reference);
        }

        public override IHypermediaQuery GetQuery()
        {
            return null;
        }
    }
}