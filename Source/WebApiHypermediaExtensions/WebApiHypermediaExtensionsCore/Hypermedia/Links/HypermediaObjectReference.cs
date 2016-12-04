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
            this.reference = hypermediaObject;
        }

        /// <summary>
        /// Resolves the referenced HypermediaObject
        /// </summary>
        /// <returns>The HypermediaObject.</returns>
        public override HypermediaObject Resolve()
        {
            return this.reference;
        }
    }
}