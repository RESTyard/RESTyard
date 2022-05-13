using System;
using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    public class SelfHypermediaLink<T> : MandatoryHypermediaLink<T> where T : HypermediaClientObject
    {
        public new async Task<T> ResolveAsync(bool ignoreCache = true)
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