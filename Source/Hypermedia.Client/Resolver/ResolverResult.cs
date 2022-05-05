using Bluehands.Hypermedia.Client.Hypermedia;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public static class ResolverResult
    {
        public static ResolverResult<TResult> Failed<TResult>(IHypermediaResolver resolver)
            where TResult : HypermediaClientObject
        {
            return new ResolverResult<TResult>(false, default, resolver);
        }
    }

    public class ResolverResult<T>
        where T : HypermediaClientObject
    {
        public ResolverResult(
            bool success,
            T resultObject,
            IHypermediaResolver resolver)
        {
            Success = success;
            ResultObject = resultObject;
            Resolver = resolver;
        }

        public bool Success { get; }

        public T ResultObject { get; }

        public IHypermediaResolver Resolver { get; }
    }
}