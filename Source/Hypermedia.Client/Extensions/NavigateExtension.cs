using System;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions
{
    public static class NavigateExtension
    {
        public static async Task<ResolverResult<TResult>> NavigateAsync<TIn, TResult>(
            this Task<ResolverResult<TIn>> resultInTask,
            Func<TIn, HypermediaLink<TResult>> linkSelector)
            where TIn : HypermediaClientObject
            where TResult : HypermediaClientObject
        {
            return await (await resultInTask).NavigateAsync(linkSelector);
        }

        public static async Task<ResolverResult<TResult>> NavigateAsync<TIn, TResult>(
            this ResolverResult<TIn> resultIn,
            Func<TIn, HypermediaLink<TResult>> linkSelector)
            where TIn : HypermediaClientObject
            where TResult : HypermediaClientObject
        {
            if (!resultIn.Success)
            {
                return ResolverResult.Failed<TResult>(resultIn.Resolver);
            }

            return await linkSelector(resultIn.ResultObject).TryResolveAsync(resultIn.Resolver);
        }
    }
}