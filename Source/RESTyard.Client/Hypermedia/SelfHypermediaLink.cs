using System;
using System.Threading.Tasks;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Hypermedia
{
    public class SelfHypermediaLink<T> : MandatoryHypermediaLink<T> where T : HypermediaClientObject
    {
        public async Task<HypermediaResult<T>> RefreshAsync()
        {
            var result = await this.Resolver.ResolveLinkAsync<T>(this.Uri!, forceResolve: true);

            return result;
        }
    }
}