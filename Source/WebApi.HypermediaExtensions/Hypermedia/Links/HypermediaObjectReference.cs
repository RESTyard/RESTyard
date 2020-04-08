using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.Query;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.Hypermedia.Links
{
    /// <summary>
    /// Reference to a HypermediaObject. Use this if you already have a HypermediaObject.
    /// </summary>
    public class HypermediaObjectReference<T> : HypermediaObjectReferenceBase where T : HypermediaObject
    {
        private readonly T reference;

        public HypermediaObjectReference(T hypermediaObject) : base (hypermediaObject.GetType())
        {
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

        public static implicit operator HypermediaObjectReference<T>(T hypermediaObject)
        {
            return new HypermediaObjectReference<T>(hypermediaObject);
        }
    }
}