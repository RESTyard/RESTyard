using System;
using System.Threading.Tasks;

namespace RESTyard.Client.Hypermedia
{
    public class SelfHypermediaLink<T> : MandatoryHypermediaLink<T> where T : HypermediaClientObject
    {
        public async Task<T> RefreshAsync()
        {
            var result = await this.Resolver.ResolveLinkAsync<T>(this.Uri, forceResolve: true);
            if (!result.Success)
            {
                throw new Exception("Could not resolve mandatory link.");
            }

            return result.ResultObject;
        }
    }
}