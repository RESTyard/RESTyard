using System;
using System.Threading;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Extensions
{
    public static class LinkExtensions
    {
        public static async Task<HypermediaResult<THco>> ResolveAsync<THco>(
            this HypermediaLink<THco> link,
            CancellationToken cancellationToken = default)
            where THco : HypermediaClientObject
        {
            if (link.Uri == null)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Link Uri is null"));
            }

            return await link.Resolver.ResolveLinkAsync<THco>(link.Uri, cancellationToken: cancellationToken);
        }
    }
}