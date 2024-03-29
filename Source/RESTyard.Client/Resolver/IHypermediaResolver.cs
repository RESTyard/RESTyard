using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;

namespace RESTyard.Client.Resolver
{
    public interface IHypermediaResolver
        : IDisposable
    {
        /// <summary>
        /// Resolves a link whose data is located at the given URI
        /// </summary>
        /// <typeparam name="T">The HCO type that is the target of the resolve operation</typeparam>
        /// <param name="uriToResolve">The Uri that indicates the location of the desired HCO</param>
        /// <param name="forceResolve">If set to true, indicates to the Resolver to ignore any measures to circumvent making a request with the server, like serving the result from a local cache.</param>
        /// <returns></returns>
        Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve, bool forceResolve = false) where T : HypermediaClientObject;

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method);

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject);

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method) where T : HypermediaClientObject;

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject) where T : HypermediaClientObject;
    }
}