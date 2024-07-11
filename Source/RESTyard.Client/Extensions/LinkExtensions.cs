using System;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Extensions
{
    public static class LinkExtensions
    {
        public static async Task<HypermediaResult<THco>> ResolveAsync<THco>(
            this HypermediaLink<THco> link)
            where THco : HypermediaClientObject
        {
            if (link.Uri == null)
            {
                return HypermediaResult.Error<THco>(HypermediaProblem.InvalidRequest("Link Uri is null"));
            }

            try
            {
                var result = await link.Resolver.ResolveLinkAsync<THco>(link.Uri);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<THco>(HypermediaProblem.Exception(e));
            }
        }
    }
}