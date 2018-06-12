namespace Hypermedia.Client
{
    using System;
    using System.Threading.Tasks;

    using global::Hypermedia.Client.Hypermedia;
    using global::Hypermedia.Client.Resolver;

    public class HypermediaClient<TEntryPoint> : IHypermediaClient<TEntryPoint> where TEntryPoint : HypermediaClientObject
    {
        private readonly IHypermediaReader sirenHypermediaReader;
        private readonly IHypermediaResolver resolver;

        public Uri UriApiEntryPoint { get; private set; }

        public HypermediaClient(
            Uri uriApiEntryPoint,
            IHypermediaResolver hypermediaResolver,
            IHypermediaReader hypermediaReader) 
        {
            this.UriApiEntryPoint = uriApiEntryPoint;
            this.resolver = hypermediaResolver;
            this.sirenHypermediaReader = hypermediaReader;
            this.resolver.InitializeHypermediaReader(this.sirenHypermediaReader);
        }

        public async Task<TEntryPoint> EnterAsync()
        {
            var result = await this.resolver.ResolveLinkAsync<TEntryPoint>(this.UriApiEntryPoint);

            if (!result.Success)
            {
                throw new Exception("Could not resolve Entry Point to API.");
            }

            var hypermediaClientObject =  result.ResultObject;
            return hypermediaClientObject;
        }
    }
}