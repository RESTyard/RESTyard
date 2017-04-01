using System;
using System.Threading.Tasks;
using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Commands;
using HypermediaClient.ParameterSerializer;
using HypermediaClient.Resolver;

namespace HypermediaClient
{
    public class SirenHttpHypermediaClient<TEntryPoint> : IHypermediaClient<TEntryPoint> where TEntryPoint : HypermediaClientObject
    {
        private readonly SirenHypermediaReader sirenHypermediaReader;
        private readonly HypermediaHttpResolver resolver;

        // todo make mock able with json example text 
        public SirenHttpHypermediaClient(Uri uriApiEntryPoint, IHypermediaObjectRegister hypermediaObjectRegister) 
        {
            this.UriApiEntryPoint = uriApiEntryPoint;

            // register http implementations for actions
            var hypermediaCommandFactory = CreateHypermediaCommandFactory();
            resolver = new HypermediaHttpResolver(ProcessContent, new SingleJsonObjectParameterSerializer());
            sirenHypermediaReader = new SirenHypermediaReader(hypermediaObjectRegister, hypermediaCommandFactory, resolver);
        }

        private static RegisterHypermediaCommandFactory CreateHypermediaCommandFactory()
        {
            var hypermediaCommandFactory = new RegisterHypermediaCommandFactory();
            hypermediaCommandFactory.Register(typeof(IHypermediaClientAction), typeof(HypermediaClientAction));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientAction<>), typeof(HypermediaClientAction<>));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFunction<>), typeof(HypermediaClientFunction<>));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFunction<,>), typeof(HypermediaClientFunction<,>));
            return hypermediaCommandFactory;
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