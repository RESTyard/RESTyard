using System;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Extensions
{
    public static class NavigateExtension
    {
        public static async Task<HypermediaResult<TResult>> NavigateAsync<TIn, TResult>(
            this Task<HypermediaResult<TIn>> result,
            Func<TIn, HypermediaLink<TResult>> linkSelector)
            where TResult : HypermediaClientObject
            where TIn : HypermediaClientObject
        {
            return await result.Bind(hco => linkSelector(hco).ResolveAsync());
        }

        public static async Task<HypermediaResult<TResult>> NavigateAsync<TIn, TResult>(
            this HypermediaResult<TIn> result,
            Func<TIn, HypermediaLink<TResult>> linkSelector)
            where TResult : HypermediaClientObject
            where TIn : HypermediaClientObject
        {
            return await result.Bind(hco => linkSelector(hco).ResolveAsync());
        }
    }
}