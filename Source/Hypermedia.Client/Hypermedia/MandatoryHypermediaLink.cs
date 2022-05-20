using System;
using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    public class MandatoryHypermediaLink<T> : HypermediaLink<T> where T : HypermediaClientObject
    {
        public async Task<T> ResolveAsync()
        {
            var result = await this.Resolver.ResolveLinkAsync<T>(this.Uri);
            if (!result.Success)
            {
                throw new Exception("Could not resolve mandatory link.");
            }

            return result.ResultObject;
        }
    }
}