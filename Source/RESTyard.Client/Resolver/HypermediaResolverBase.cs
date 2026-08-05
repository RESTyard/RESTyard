using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FunicularSwitch;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.ParameterSerializer;
using RESTyard.Client.Reader;
using RESTyard.Client.Resolver.Caching;
using RESTyard.MediaTypes;

namespace RESTyard.Client.Resolver
{
    public abstract class HypermediaResolverBase<
            TNetworkResponseMessage,
            TUploadPayload,
            TLinkHcoCacheEntry,
            TLinkHcoCacheEntryConfiguration>
        : IHypermediaResolver
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
        where TLinkHcoCacheEntryConfiguration : LinkHcoCacheEntryConfiguration
    {
        private bool alreadyDisposed;

        protected HypermediaResolverBase(
            IHypermediaReader hypermediaReader,
            IParameterSerializer parameterSerializer,
            IProblemStringReader problemReader,
            ILinkHcoCache<TLinkHcoCacheEntry> linkHcoCache)
        {
            HypermediaReader = hypermediaReader;
            ParameterSerializer = parameterSerializer;
            ProblemReader = problemReader;
            LinkHcoCache = linkHcoCache;
        }

        protected IHypermediaReader HypermediaReader { get; }

        protected IParameterSerializer ParameterSerializer { get; }

        protected IProblemStringReader ProblemReader { get; }

        protected ILinkHcoCache<TLinkHcoCacheEntry> LinkHcoCache { get; }

        public async Task<HypermediaResult<T>> ResolveLinkAsync<T>(
            Uri uriToResolve,
            bool forceResolve = false,
            CancellationToken cancellationToken = default)
            where T : HypermediaClientObject
        {
            HypermediaResult<TNetworkResponseMessage> networkResult;
            if (this.LinkHcoCache.TryGetValue(uriToResolve, out var cacheEntry))
            {
                var verificationResult = await this.VerifyIfCacheEntryCanBeUsedAsync(uriToResolve, cacheEntry, DateTimeOffset.Now, forceResolve, cancellationToken);
                if (verificationResult.IsError)
                {
                    return HypermediaResult.Error(verificationResult.GetErrorOrDefault()!);
                }

                var verification = verificationResult.GetValueOrThrow();
                var cacheEntryCanBeUsed = verification.Match(
                    canBeUsed => true,
                    canNotBeUsed => false,
                    useThisResponseInstead => false);
                if (cacheEntryCanBeUsed)
                {
                    return this.HypermediaReader.Read(cacheEntry.LinkResponseContent, this)
                        .Match(
                            hco => HypermediaResult.Ok((T)hco),
                            error => HypermediaResult.Error(error.Match(
                                requiredPropertyMissing => HypermediaProblem.InvalidResponse(requiredPropertyMissing.Message),
                                invalidFormat => HypermediaProblem.InvalidResponse(invalidFormat.Message),
                                invalidClientClass => HypermediaProblem.BadHcoDefinition(invalidClientClass.Message),
                                exception => HypermediaProblem.Exception(exception.Exc))));
                }
                else
                {
                    this.LinkHcoCache.Remove(uriToResolve);
                }

                networkResult = await verification.Match(
                    canBeUsed => this.ResolveAsync(uriToResolve, cancellationToken),
                    canNotBeUsed => this.ResolveAsync(uriToResolve, cancellationToken),
                    useResponse => Task.FromResult(HypermediaResult.Ok(useResponse.Response)));
            }
            else
            {
                networkResult = await this.ResolveAsync(uriToResolve, cancellationToken);
            }

            return await networkResult
                .Bind(async response =>
                {
                    var cacheConfiguration = this.GetCacheConfigurationFromResponse(response, DateTimeOffset.Now);
                    bool serializeToString = cacheConfiguration.ShouldBeAddedToCache();
                    var linkResult = await this.HandleLinkResponseAsync<T>(response, serializeToString, cancellationToken);

                    linkResult.Match(ok =>
                    {
                        var hcoAsString = ok.HcoAsString;
                        if (cacheConfiguration.ShouldBeAddedToCache()
                            && !string.IsNullOrEmpty(hcoAsString))
                        {
                            var entry = GetCacheEntryFromConfiguration(hcoAsString, cacheConfiguration);
                            this.LinkHcoCache.Set(uriToResolve, entry);
                        }
                    });

                    return linkResult.Bind<T>(ok => ok.ResultHco);
                });
        }

        protected abstract Task<HypermediaResult<CacheEntryVerificationResult<TNetworkResponseMessage>>> VerifyIfCacheEntryCanBeUsedAsync(
            Uri uriToResolve,
            TLinkHcoCacheEntry cacheEntry,
            DateTimeOffset assumedNow,
            bool forceResolve,
            CancellationToken cancellationToken = default);

        protected abstract TLinkHcoCacheEntryConfiguration GetCacheConfigurationFromResponse(
            TNetworkResponseMessage response,
            DateTimeOffset assumedNow);

        protected abstract TLinkHcoCacheEntry GetCacheEntryFromConfiguration(
            string linkResponseContent,
            TLinkHcoCacheEntryConfiguration cacheConfiguration);

        public async Task<HypermediaResult<Unit>> ResolveActionAsync(
            Uri uri,
            string method,
            CancellationToken cancellationToken = default)
        {
            return await this.SendCommandAsync(uri, method, cancellationToken: cancellationToken)
                .Bind(responseMessage => this.HandleActionResponseAsync(responseMessage, cancellationToken));
        }

        public async Task<HypermediaResult<Unit>> ResolveActionAsync(
            Uri uri,
            string method,
            IReadOnlyList<ParameterDescription> parameterDescriptions,
            object? parameterObject,
            CancellationToken cancellationToken = default)
        {
            return parameterObject switch
            {
                IHypermediaFileUploadParameter fileUploadParameter => await this.ProcessUploadParameters(parameterDescriptions, fileUploadParameter, cancellationToken)
                    .Bind(uploadPayload => this.SendUploadCommandAsync(uri, method, uploadPayload, cancellationToken))
                    .Bind(responseMessage => this.HandleActionResponseAsync(responseMessage, cancellationToken)),
                _ => await this.ProcessParameters(parameterDescriptions, parameterObject)
                    .Bind(serializedParameters => this.SendCommandAsync(uri, method, serializedParameters, cancellationToken))
                    .Bind(responseMessage => this.HandleActionResponseAsync(responseMessage, cancellationToken))
            };
        }

        public async Task<HypermediaResult<MandatoryHypermediaLink<T>>> ResolveFunctionAsync<T>(
            Uri uri,
            string method,
            CancellationToken cancellationToken = default) where T : HypermediaClientObject
        {
            return await SendCommandAsync(uri, method, cancellationToken: cancellationToken)
                .Bind(responseMessage => this.HandleFunctionResponseAsync<T>(responseMessage, cancellationToken));
        }

        public async Task<HypermediaResult<MandatoryHypermediaLink<T>>> ResolveFunctionAsync<T>(
            Uri uri,
            string method,
            IReadOnlyList<ParameterDescription> parameterDescriptions,
            object? parameterObject,
            CancellationToken cancellationToken = default) where T : HypermediaClientObject
        {
            return parameterObject switch
            {
                IHypermediaFileUploadParameter fileUploadParameter => await this.ProcessUploadParameters(parameterDescriptions, fileUploadParameter, cancellationToken)
                    .Bind(uploadPayload => this.SendUploadCommandAsync(uri, method, uploadPayload, cancellationToken))
                    .Bind(responseMessage => this.HandleFunctionResponseAsync<T>(responseMessage, cancellationToken)),
                _ => await this.ProcessParameters(parameterDescriptions, parameterObject)
                    .Bind(serializedParameters => this.SendCommandAsync(uri, method, serializedParameters, cancellationToken))
                    .Bind(responseMessage => this.HandleFunctionResponseAsync<T>(responseMessage, cancellationToken))
            };
        }

        protected async Task<HypermediaResult<(T ResultHco, string HcoAsString)>> HandleLinkResponseAsync<T>(
            TNetworkResponseMessage responseMessage,
            bool serializeToString,
            CancellationToken cancellationToken = default)
            where T : HypermediaClientObject
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage, cancellationToken)
                .Bind(_ => this.ResponseAsStreamAsync(responseMessage, cancellationToken))
                .Bind(async hypermediaObjectSirenStream =>
                {
                    HypermediaReaderResult<(HypermediaClientObject ResultHco, string HcoAsString)> readResult;
                    if (serializeToString)
                    {
                        readResult = await this.HypermediaReader
                            .ReadAndSerializeAsync(hypermediaObjectSirenStream, this, cancellationToken);
                    }
                    else
                    {
                        readResult = await this.HypermediaReader
                            .ReadAsync(hypermediaObjectSirenStream, this, cancellationToken)
                            .Map(hco => (hco, string.Empty));
                    }

                    return readResult.Match(
                        ok: tuple => tuple.ResultHco is T hco
                            ? HypermediaResult.Ok((hco, tuple.HcoAsString))
                            : HypermediaResult.Error(HypermediaProblem.InvalidResponse(($"Could not retrieve result as {typeof(T).Name}."))),
                        error => HypermediaResult.Error(
                            error.Match(
                                requiredPropertyMissing: rpm => HypermediaProblem.InvalidResponse(rpm.Message),
                                invalidFormat: invalidFormat => HypermediaProblem.InvalidResponse(invalidFormat.Message),
                                invalidClientClass: icc => HypermediaProblem.BadHcoDefinition(icc.Message),
                                exception => HypermediaProblem.Exception(exception.Exc))));
                });
        }

        protected async Task<HypermediaResult<Unit>> HandleActionResponseAsync(
            TNetworkResponseMessage responseMessage,
            CancellationToken cancellationToken = default)
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage, cancellationToken);
        }

        protected async Task<HypermediaResult<MandatoryHypermediaLink<T>>> HandleFunctionResponseAsync<T>(
            TNetworkResponseMessage responseMessage,
            CancellationToken cancellationToken = default)
            where T : HypermediaClientObject
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage, cancellationToken)
                .Bind(_ => this.GetLocation(responseMessage))
                .Bind(location =>
                {
                    var actionResult = HypermediaResult.Ok(new MandatoryHypermediaLink<T>()
                    {
                        Uri = location,
                        Resolver = this,
                    });

                    return actionResult;
                });
        }

        protected HypermediaResult<string> ProcessParameters(IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject)
        {
            if (parameterObject is null)
            {
                return HypermediaResult.Error(
                    HypermediaProblem.InvalidRequest("Parameter is described but not passed by action."));
            }

            return GetParameterDescription(parameterDescriptions)
                .Map(parameterDescription => this.ParameterSerializer.SerializeParameterObject(parameterDescription.Name, parameterObject));
        }

        protected Task<HypermediaResult<TUploadPayload>> ProcessUploadParameters(
            IReadOnlyList<ParameterDescription> parameterDescriptions,
            IHypermediaFileUploadParameter parameterObject,
            CancellationToken cancellationToken = default)
        {
            return GetParameterDescription(parameterDescriptions)
                .Match(
                    parameterDescription => CreateUploadPayload(parameterObject, parameterDescription, cancellationToken),
                    error: _ => CreateUploadPayload(parameterObject, cancellationToken: cancellationToken));
        }

        protected abstract Task<HypermediaResult<TUploadPayload>> CreateUploadPayload(IHypermediaFileUploadParameter parameterObject, ParameterDescription? parameterDescription = null, CancellationToken cancellationToken = default);

        protected static HypermediaResult<ParameterDescription> GetParameterDescription(IReadOnlyList<ParameterDescription> parameterDescriptions)
        {
            if (parameterDescriptions.Count == 0)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Parameter not described."));
            }

            // todo allow more fields
            if (parameterDescriptions.Count > 1)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Only one action parameter is supported."));
            }

            // todo allow more types
            var parameterDescription = parameterDescriptions.First();
            if (!parameterDescription.Type.Equals(DefaultMediaTypes.ApplicationJson) && !parameterDescription.Type.Equals(DefaultMediaTypes.MultipartFormData))
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Only one action type 'application/json' is supported."));
            }
            return HypermediaResult.Ok(parameterDescription);
        }

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> ResolveAsync(
            Uri uriToResolve,
            CancellationToken cancellationToken = default);

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> SendCommandAsync(
            Uri uri,
            string method,
            string? payload = null,
            CancellationToken cancellationToken = default);

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> SendUploadCommandAsync(
            Uri uri,
            string method,
            TUploadPayload payload,
            CancellationToken cancellationToken = default);

        protected abstract Task<HypermediaResult<Unit>> EnsureRequestIsSuccessfulAsync(
            TNetworkResponseMessage responseMessage,
            CancellationToken cancellationToken = default);

        protected abstract Task<HypermediaResult<Stream>> ResponseAsStreamAsync(
            TNetworkResponseMessage responseMessage,
            CancellationToken cancellationToken = default);

        protected abstract HypermediaResult<Uri> GetLocation(TNetworkResponseMessage responseMessage);

        ~HypermediaResolverBase()
        {
            this.Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.alreadyDisposed)
            {
                return;
            }

            this.alreadyDisposed = true;
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}