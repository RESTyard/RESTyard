using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    public class HypermediaLink<T> : IHypermediaLink where T : HypermediaClientObject
    {
        public List<string> Relations { get; set; }

        public Uri Uri { get; set; } = null;

        public IHypermediaResolver Resolver { get; set; }

        public async Task<ResolverResult<T>> TryResolveAsync(bool ignoreCache = false)
        {
            if (this.Uri == null)
            {
                return new ResolverResult<T> {Success = false};
            }

            var result = await this.Resolver.ResolveLinkAsync<T>(this.Uri, ignoreCache: ignoreCache);
            return result;
        }
    }
}