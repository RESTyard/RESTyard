using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Commands;

namespace HypermediaClient.Resolver
{
    public interface IHypermediaResolver
    {
        Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve) where T : HypermediaClientObject;

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method);

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject);

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method) where T : HypermediaClientObject;

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject) where T : HypermediaClientObject;
    }
}