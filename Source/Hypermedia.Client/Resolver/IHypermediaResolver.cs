using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Authentication;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;
using Bluehands.Hypermedia.Client.Reader;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public interface IHypermediaResolver
        : IDisposable
    {
        Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve) where T : HypermediaClientObject;

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method);

        Task<HypermediaCommandResult> ResolveActionAsync(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject);

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method) where T : HypermediaClientObject;

        Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(Uri uri, string method, List<ParameterDescription> parameterDescriptions, object parameterObject) where T : HypermediaClientObject;
    }
}