using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HypermediaClient.Resolver;

namespace HypermediaClient.Hypermedia
{
    public class MandatoryHypermediaLink<T> : HypermediaLink<T> where T : HypermediaClientObject
    {
        public async Task<T> ResolveAsync()
        {
            var result = await Resolver.ResolveLinkAsync<T>(Uri);
            if (!result.Success)
            {
                throw new Exception("Could not resolve mandatory link.");
            }

            return result.ResultObject;
        }
    }

    public class HypermediaLink<T> : IHypermediaLink where T : HypermediaClientObject
    {
        public List<string> Relations { get; set; }

        public Uri Uri { get; set; } = null;

        public IHypermediaResolver Resolver { get; set; }

        public async Task<ResolverResult<T>> TryResolveAsync()
        {
            if (Uri == null)
            {
                return new ResolverResult<T> {Success = false};
            }

            var result = await Resolver.ResolveLinkAsync<T>(Uri);
            return result;
        }
    }

    public interface IHypermediaLink : ICanResolve
    {
        Uri Uri { get; set; }
        List<string> Relations { get; set; }
    }
}