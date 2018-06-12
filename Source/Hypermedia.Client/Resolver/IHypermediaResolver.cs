namespace Hypermedia.Client.Resolver
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using global::Hypermedia.Client.Authentication;
    using global::Hypermedia.Client.Hypermedia;
    using global::Hypermedia.Client.Hypermedia.Commands;
    using global::Hypermedia.Client.Reader;

    public interface IHypermediaResolver
    {
        void InitializeHypermediaReader(IHypermediaReader reader);

        void SetCredentials(UsernamePasswordCredentials usernamePasswordCredentials);

        Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve) where T : HypermediaClientObject;

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method);

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject);

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method) where T : HypermediaClientObject;

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject) where T : HypermediaClientObject;
    }
}