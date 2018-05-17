using System;
using System.Threading.Tasks;
using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Commands;
using HypermediaClient.Resolver;

namespace HypermediaClient
{
    public class SirenHttpHypermediaClient<TEntryPoint> : IHypermediaClient<TEntryPoint> where TEntryPoint : HypermediaClientObject
    {
        private readonly IHypermediaReader sirenHypermediaReader;
        private readonly IHypermediaResolver resolver;

        // todo make mock able with json example text 
        public SirenHttpHypermediaClient(
            Uri uriApiEntryPoint,
            IHypermediaResolver hypermediaResolver,
            IHypermediaReader hypermediaReader) 
        {
            UriApiEntryPoint = uriApiEntryPoint;
            resolver = hypermediaResolver;
            sirenHypermediaReader = hypermediaReader;
            resolver.InitializeHypermediaReader(sirenHypermediaReader);
        }

        

        public Uri UriApiEntryPoint { get; private set; }

        public async Task<TEntryPoint> EnterAsync()
        {
            var result = await resolver.ResolveLinkAsync<TEntryPoint>(UriApiEntryPoint);

            if (!result.Success)
            {
                throw new Exception("Could not resolve Entry Point to API.");
            }

            var hypermediaClientObject =  result.ResultObject;
            return hypermediaClientObject;
        }

        private HypermediaClientObject ProcessContent(string siren) // TODO use stream
        {
            return sirenHypermediaReader.Read(siren);
        }
    }
}