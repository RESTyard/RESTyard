using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FunicularSwitch;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;

namespace RESTyard.Client.Resolver;

public class ResolverDummyObject : IHypermediaResolver
{
    public static IHypermediaResolver Instance { get; } = new ResolverDummyObject();

    public void Dispose()
    {
    }

    public Task<HypermediaResult<T>> ResolveLinkAsync<T>(Uri uriToResolve, bool forceResolve = false) where T : HypermediaClientObject
    {
        return Task.FromResult(HypermediaResult.Error<T>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<Unit>> ResolveActionAsync(Uri uri, string method)
    {
        return Task.FromResult(HypermediaResult.Error<Unit>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<Unit>> ResolveActionAsync(Uri uri, string method, IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject)
    {
        return Task.FromResult(HypermediaResult.Error<Unit>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<LinkOrEntity<T>>> ResolveFunctionAsync<T>(Uri uri, string method,
        bool supportInlineFunctionResult) where T : HypermediaClientObject
    {
        return Task.FromResult(HypermediaResult.Error<LinkOrEntity<T>>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<LinkOrEntity<T>>> ResolveFunctionAsync<T>(Uri uri, string method, bool supportInlineFileResult, IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject) where T : HypermediaClientObject
    {
        return Task.FromResult(HypermediaResult.Error<LinkOrEntity<T>>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }
}