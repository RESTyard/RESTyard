using System;
using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    public class MandatoryHypermediaLink<T> : HypermediaLink<T> where T : HypermediaClientObject
    {
        public async Task<T> ResolveAsync(bool ignoreCache = false)
        {
            var result = await this.Resolver.ResolveLinkAsync<T>(this.Uri, ignoreCache: ignoreCache);
            if (!result.Success)
            {
                throw new Exception("Could not resolve mandatory link.");
            }

            return result.ResultObject;
        }
    }
}