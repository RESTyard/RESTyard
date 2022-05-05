using System;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions
{
    public static class LinkExtensions
    {
        public static async Task<ResolverResult<THco>> TryResolveAsync<THco>(
            this HypermediaLink<THco> link,
            IHypermediaResolver resolver)
            where THco : HypermediaClientObject
        {
            if (link.Uri == null)
            {
                return ResolverResult.Failed<THco>(resolver);
            }

            var result = await resolver.ResolveLinkAsync<THco>(link.Uri);
            return result;
        }

        public static async Task<THco> ResolveAsync<THco>(
            this MandatoryHypermediaLink<THco> link,
            IHypermediaResolver resolver)
            where THco : HypermediaClientObject
        {
            var result = await resolver.ResolveLinkAsync<THco>(link.Uri);
            if (!result.Success)
            {
                throw new Exception("Could not resolve mandatory link.");
            }

            return result.ResultObject;
        }
    }
}