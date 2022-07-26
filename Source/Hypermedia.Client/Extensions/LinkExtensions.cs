using System;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Extensions
{
    public static class LinkExtensions
    {
        public static async Task<ResolverResult<THco>> TryResolveAsync<THco>(
            this HypermediaLink<THco> link)
            where THco : HypermediaClientObject
        {
            if (link.Uri == null)
            {
                return ResolverResult.Failed<THco>();
            }

            var result = await link.Resolver.ResolveLinkAsync<THco>(link.Uri);
            return result;
        }

        public static async Task<THco> ResolveAsync<THco>(
            this MandatoryHypermediaLink<THco> link)
            where THco : HypermediaClientObject
        {
            var result = await link.Resolver.ResolveLinkAsync<THco>(link.Uri);
            if (!result.Success)
            {
                throw new Exception("Could not resolve mandatory link.");
            }

            return result.ResultObject;
        }

        public static async Task<THco> RefreshAsync<THco>(
            this SelfHypermediaLink<THco> link)
            where THco : HypermediaClientObject
        {
            var result = await link.Resolver.ResolveLinkAsync<THco>(link.Uri, true);
            if (!result.Success)
            {
                throw new Exception("Could not resolve mandatory link.");
            }

            return result.ResultObject;
        }
    }
}