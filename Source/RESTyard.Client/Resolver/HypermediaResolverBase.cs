using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
            bool forceResolve = false)
            where T : HypermediaClientObject
        {
            HypermediaResult<TNetworkResponseMessage> networkResult;
            if (this.LinkHcoCache.TryGetValue(uriToResolve, out var cacheEntry))
            {
                var verificationResult = await this.VerifyIfCacheEntryCanBeUsedAsync(uriToResolve, cacheEntry, DateTimeOffset.Now, forceResolve);
                var cacheEntryCanBeUsed = verificationResult.Match(
                    canBeUsed => true,
                    canNotBeUsed => false,
                    useThisResponseInstead => false);
                if (cacheEntryCanBeUsed)
                {
                    return this.HypermediaReader.Read(cacheEntry.LinkResponseContent, this)
                        .Match(
                            hco => HypermediaResult.Ok((T)hco),
                            error => HypermediaResult.Error<T>(error.Match(
                                requiredPropertyMissing => HypermediaProblem.InvalidResponse(requiredPropertyMissing.Message),
                                invalidFormat => HypermediaProblem.InvalidResponse(invalidFormat.Message),
                                invalidClientClass => HypermediaProblem.BadHcoDefinition(invalidClientClass.Message),
                                exception => HypermediaProblem.Exception(exception.Exc))));
                }
                else
                {
                    this.LinkHcoCache.Remove(uriToResolve);
                }

                networkResult = await verificationResult.Match(
                    canBeUsed => this.ResolveAsync(uriToResolve),
                    canNotBeUsed => this.ResolveAsync(uriToResolve),
                    useResponse => Task.FromResult(HypermediaResult.Ok(useResponse.Response)));
            }
            else
            {
                networkResult = await this.ResolveAsync(uriToResolve);
            }

            return await networkResult
                .Bind(response => HandleLinkResponseAndAddToCacheIfCacheable<T>(uriToResolve, response));
        }

        private async Task<HypermediaResult<T>> HandleLinkResponseAndAddToCacheIfCacheable<T>(Uri uriToResolve, TNetworkResponseMessage response)
            where T : HypermediaClientObject
        {
            var cacheConfiguration = this.GetCacheConfigurationFromResponse(response, DateTimeOffset.Now);
            bool serializeToString = cacheConfiguration.ShouldBeAddedToCache();
            var linkResult = await this.HandleLinkResponseAsync<T>(response, serializeToString);

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

            return linkResult.Map(ok => ok.ResultHco);
        }

        protected abstract Task<CacheEntryVerificationResult<TNetworkResponseMessage>> VerifyIfCacheEntryCanBeUsedAsync(
            Uri uriToResolve,
            TLinkHcoCacheEntry cacheEntry,
            DateTimeOffset assumedNow,
            bool forceResolve);

        protected abstract TLinkHcoCacheEntryConfiguration GetCacheConfigurationFromResponse(
            TNetworkResponseMessage response,
            DateTimeOffset assumedNow);

        protected abstract TLinkHcoCacheEntry GetCacheEntryFromConfiguration(
            string linkResponseContent,
            TLinkHcoCacheEntryConfiguration cacheConfiguration);

        public async Task<HypermediaResult<Unit>> ResolveActionAsync(
            Uri uri,
            string method)
        {
            return await this.SendCommandAsync(uri, method, supportInlineFunctionResult: false)
                .Bind(this.HandleActionResponseAsync);
        }

        public async Task<HypermediaResult<Unit>> ResolveActionAsync(
            Uri uri,
            string method,
            IReadOnlyList<ParameterDescription> parameterDescriptions,
            object? parameterObject)
        {
            if (parameterObject is IHypermediaFileUploadParameter fileUploadParameter)
            {
                return await this.ProcessUploadParameters(parameterDescriptions, fileUploadParameter)
                    .Bind(uploadPayload => this.SendUploadCommandAsync(uri, method, supportInlineFunctionResult: false, uploadPayload))
                    .Bind(this.HandleActionResponseAsync);
            }
            return await this.ProcessParameters(parameterDescriptions, parameterObject)
                .Bind(serializedParameters => this.SendCommandAsync(uri, method, supportInlineFunctionResult: false, serializedParameters))
                .Bind(this.HandleActionResponseAsync);
        }

        public async Task<HypermediaResult<LinkOrEntity<T>>> ResolveFunctionAsync<T>(
            Uri uri,
            string method,
            bool supportInlineFunctionResult) where T : HypermediaClientObject
        {
            return await SendCommandAsync(uri, method, supportInlineFunctionResult)
                .Bind(this.HandleFunctionResponseAsync<T>);
        }

        public async Task<HypermediaResult<LinkOrEntity<T>>> ResolveFunctionAsync<T>(
            Uri uri,
            string method,
            bool supportInlineFunctionResult,
            IReadOnlyList<ParameterDescription> parameterDescriptions,
            object? parameterObject) where T : HypermediaClientObject
        {
            if (parameterObject is IHypermediaFileUploadParameter fileUploadParameter)
            {
                return await this.ProcessUploadParameters(parameterDescriptions, fileUploadParameter)
                    .Bind(uploadPayload => this.SendUploadCommandAsync(uri, method, supportInlineFunctionResult, uploadPayload))
                    .Bind(this.HandleFunctionResponseAsync<T>);
            }
            return await this.ProcessParameters(parameterDescriptions, parameterObject)
                .Bind(serializedParameters => this.SendCommandAsync(uri, method, supportInlineFunctionResult, serializedParameters))
                .Bind(this.HandleFunctionResponseAsync<T>);
        }

        protected async Task<HypermediaResult<(T ResultHco, string HcoAsString)>> HandleLinkResponseAsync<T>(
            TNetworkResponseMessage responseMessage,
            bool serializeToString)
            where T : HypermediaClientObject
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage)
                .Bind(_ => this.ResponseAsStreamAsync(responseMessage))
                .Bind(async hypermediaObjectSirenStream =>
                {
                    HypermediaReaderResult<(HypermediaClientObject ResultHco, string HcoAsString)> readResult;
                    if (serializeToString)
                    {
                        readResult =
                            await this.HypermediaReader.ReadAndSerializeAsync(hypermediaObjectSirenStream, this);
                    }
                    else
                    {
                        readResult =
                            await this.HypermediaReader.ReadAsync(hypermediaObjectSirenStream, this).Map(hco => (hco, string.Empty));
                    }

                    return readResult.Match(
                        ok: tuple => tuple.ResultHco is T hco
                            ? HypermediaResult.Ok((hco, tuple.HcoAsString))
                            : HypermediaResult.Error<(T, string)>(HypermediaProblem.InvalidResponse(($"Could not retrieve result as {typeof(T).Name}."))),
                        error => HypermediaResult.Error<(T, string)>(
                            error.Match(
                                requiredPropertyMissing: rpm => HypermediaProblem.InvalidResponse(rpm.Message),
                                invalidFormat: invalidFormat => HypermediaProblem.InvalidResponse(invalidFormat.Message),
                                invalidClientClass: icc => HypermediaProblem.BadHcoDefinition(icc.Message),
                                exception => HypermediaProblem.Exception(exception.Exc))));
                });
        }

        protected async Task<HypermediaResult<Unit>> HandleActionResponseAsync(TNetworkResponseMessage responseMessage)
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage);
        }

        protected async Task<HypermediaResult<LinkOrEntity<T>>> HandleFunctionResponseAsync<T>(
            TNetworkResponseMessage responseMessage)
            where T : HypermediaClientObject
        {
            return await this.EnsureRequestIsSuccessfulAsync(responseMessage)
                .Bind(async _ => this.WasFunctionResultInlined(responseMessage)
                    ? await this.GetLocation(responseMessage)
                        .Bind(locationOfInlinedResult => this.HandleLinkResponseAndAddToCacheIfCacheable<T>(locationOfInlinedResult, responseMessage))
                        .Map(LinkOrEntity<T>.Entity)
                    : this.GetLocation(responseMessage)
                        .Map(location => new MandatoryHypermediaLink<T>()
                        {
                            Uri = location,
                            Resolver = this,
                        })
                        .Map(LinkOrEntity<T>.Link));
        }

        protected HypermediaResult<string> ProcessParameters(IReadOnlyList<ParameterDescription> parameterDescriptions, object? parameterObject)
        {
            if (parameterObject is null)
            {
                return HypermediaResult.Error<string>(
                    HypermediaProblem.InvalidRequest("Parameter is described but not passed by action."));
            }

            return GetParameterDescription(parameterDescriptions)
                .Map(parameterDescription => {
                    var serializedParameters =
                        this.ParameterSerializer.SerializeParameterObject(parameterDescription.Name, parameterObject);
                    return serializedParameters;
                });
        }

        protected Task<HypermediaResult<TUploadPayload>> ProcessUploadParameters(
            IReadOnlyList<ParameterDescription> parameterDescriptions, IHypermediaFileUploadParameter parameterObject)
        {
            return GetParameterDescription(parameterDescriptions)
                .Match(
                    parameterDescription => CreateUploadPayload(parameterObject, parameterDescription),
                    error: _ => CreateUploadPayload(parameterObject));
        }

        protected abstract Task<HypermediaResult<TUploadPayload>> CreateUploadPayload(IHypermediaFileUploadParameter parameterObject, ParameterDescription? parameterDescription = null);

        protected static HypermediaResult<ParameterDescription> GetParameterDescription(IReadOnlyList<ParameterDescription> parameterDescriptions)
        {
            if (parameterDescriptions.Count == 0)
            {
                return HypermediaResult<ParameterDescription>.Error(HypermediaProblem.InvalidRequest("Parameter not described."));
            }

            // todo allow more fields
            if (parameterDescriptions.Count > 1)
            {
                return HypermediaResult<ParameterDescription>.Error(HypermediaProblem.InvalidRequest("Only one action parameter is supported."));
            }

            // todo allow more types
            var parameterDescription = parameterDescriptions.First();
            if (!parameterDescription.Type.Equals(DefaultMediaTypes.ApplicationJson) && !parameterDescription.Type.Equals(DefaultMediaTypes.MultipartFormData))
            {
                return HypermediaResult<ParameterDescription>.Error(HypermediaProblem.InvalidRequest("Only one action type 'application/json' is supported."));
            }
            return HypermediaResult.Ok(parameterDescription);
        }

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> ResolveAsync(Uri uriToResolve);

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> SendCommandAsync(
            Uri uri,
            string method,
            bool supportInlineFunctionResult,
            string? payload = null);

        protected abstract Task<HypermediaResult<TNetworkResponseMessage>> SendUploadCommandAsync(
            Uri uri,
            string method,
            bool supportInlineFunctionResult,
            TUploadPayload payload);

        protected abstract Task<HypermediaResult<Unit>> EnsureRequestIsSuccessfulAsync(TNetworkResponseMessage responseMessage);

        protected abstract Task<HypermediaResult<Stream>> ResponseAsStreamAsync(TNetworkResponseMessage responseMessage);

        protected abstract HypermediaResult<Uri> GetLocation(TNetworkResponseMessage responseMessage);
        
        protected abstract bool WasFunctionResultInlined(TNetworkResponseMessage responseMessage);

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