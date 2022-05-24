using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public static class ResolverResult
    {
        public static ResolverResult<TResult> Failed<TResult>()
            where TResult : HypermediaClientObject
        {
            return new ResolverResult<TResult>(false, default);
        }
    }

    public class ResolverResult<T>
        where T : HypermediaClientObject
    {
        public ResolverResult(
            bool success,
            T resultObject)
        {
            Success = success;
            ResultObject = resultObject;
        }

        public bool Success { get; }

        public T ResultObject { get; }
    }
}