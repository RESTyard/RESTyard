using System;
using System.Collections.Generic;
using System.Threading;
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

    public Task<HypermediaResult<T>> ResolveLinkAsync<T>(Uri uriToResolve, bool forceResolve = false, CancellationToken cancellationToken = default) where T : HypermediaClientObject
    {
        return Task.FromResult(HypermediaResult.Error<T>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<Unit>> ResolveActionAsync(Uri uri, string method, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HypermediaResult.Error<Unit>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<Unit>> ResolveActionAsync(Uri uri, string method, IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HypermediaResult.Error<Unit>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<MandatoryHypermediaLink<T>>> ResolveFunctionAsync<T>(Uri uri, string method, CancellationToken cancellationToken = default) where T : HypermediaClientObject
    {
        return Task.FromResult(HypermediaResult.Error<MandatoryHypermediaLink<T>>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }

    public Task<HypermediaResult<MandatoryHypermediaLink<T>>> ResolveFunctionAsync<T>(Uri uri, string method, IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject, CancellationToken cancellationToken = default) where T : HypermediaClientObject
    {
        return Task.FromResult(HypermediaResult.Error<MandatoryHypermediaLink<T>>(
            HypermediaProblem.Exception(
                new Exception($"Library failed to set {nameof(IHypermediaResolver)} on result object"))));
    }
}