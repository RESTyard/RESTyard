using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;
using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;
using Bluehands.Hypermedia.Client.Resolver.Caching;
using Bluehands.Hypermedia.MediaTypes;

namespace Bluehands.Hypermedia.Client.Resolver
{
    public abstract class HypermediaResolverBase<TNetworkResponseMessage, TLinkHcoCacheEntry>
        : IHypermediaResolver
        where TLinkHcoCacheEntry : LinkHcoCacheEntry
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

        //public async Task<ResolverResult<T>> ResolveLinkAsync<T>(Uri uriToResolve, bool forceResolve = false)
        //    where T : HypermediaClientObject
        //{
        //    TNetworkResponseMessage response;
        //    if (this.LinkHcoCache.TryGetValue(uriToResolve, out var cacheEntry))
        //    {
        //        bool wasModified;
        //        (response, wasModified) =
        //            await this.ResolveWithCheckForModificationAsync(uriToResolve, cacheEntry.Identifier);
        //        if (!wasModified)
        //        {
        //            return new ResolverResult<T>(
        //                success: true,
        //                (T)cacheEntry.HypermediaClientObject,
        //                this);
        //        }
        //        else
        //        {
        //            this.LinkHcoCache.Remove(uriToResolve);
        //        }
        //    }
        //    else
        //    {
        //        response = await this.ResolveAsync(uriToResolve);
        //    }

        //    var resolverResult = await HandleLinkResponseAsync<T>(response);

        //    if (HasCacheIdentifier(response, out var identifier))
        //    {
        //        this.LinkHcoCache.Set(uriToResolve, new CacheEntry<TCacheEntryIdentifier>(resolverResult.ResultObject, identifier));
        //    }

        //    return resolverResult;
        //}
        public abstract Task<ResolverResult<T>> ResolveLinkAsync<T>(
            Uri uriToResolve,
            bool forceResolve = false)
            where T : HypermediaClientObject;

        public async Task<HypermediaCommandResult> ResolveActionAsync(
            Uri uri,
            string method)
        {
            var responseMessage = await this.SendCommandAsync(uri, method);
            var actionResult = await this.HandleActionResponseAsync(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaCommandResult> ResolveActionAsync(
            Uri uri,
            string method,
            List<ParameterDescription> parameterDescriptions,
            object parameterObject)
        {
            var serializedParameters = this.ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await this.SendCommandAsync(uri, method, serializedParameters);
            var actionResult = await HandleActionResponseAsync(responseMessage);
            return actionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(
            Uri uri,
            string method) where T : HypermediaClientObject
        {
            var responseMessage = await SendCommandAsync(uri, method);
            var functionResult = await this.HandleFunctionResponseAsync<T>(responseMessage);
            return functionResult;
        }

        public async Task<HypermediaFunctionResult<T>> ResolveFunctionAsync<T>(
            Uri uri,
            string method,
            List<ParameterDescription> parameterDescriptions,
            object parameterObject) where T : HypermediaClientObject
        {
            var serializedParameters = this.ProcessParameters(parameterDescriptions, parameterObject);

            var responseMessage = await this.SendCommandAsync(uri, method, serializedParameters);
            var actionResult = await this.HandleFunctionResponseAsync<T>(responseMessage);
            return actionResult;
        }

        protected async Task<(ResolverResult<T>, string)> HandleLinkResponseAsync<T>(
            TNetworkResponseMessage responseMessage,
            bool exportToString)
            where T : HypermediaClientObject
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var hypermediaObjectSirenStream = await this.ResponseAsStreamAsync(responseMessage);


            HypermediaClientObject hypermediaClientObject;
            string export = string.Empty;
            if (exportToString)
            {
                (hypermediaClientObject, export) =
                    await this.HypermediaReader.ReadAndExportAsync(hypermediaObjectSirenStream);
            }
            else
            {
                hypermediaClientObject = await this.HypermediaReader.ReadAsync(hypermediaObjectSirenStream);
            }
            if (!(hypermediaClientObject is T desiredResultObject))
            {
                throw new Exception($"Could not retrieve result as {typeof(T).Name}.");
            }

            var resolverResult = new ResolverResult<T>(
                success: true,
                desiredResultObject,
                this);
            return (resolverResult, export);
        }

        protected async Task<HypermediaCommandResult> HandleActionResponseAsync(TNetworkResponseMessage responseMessage)
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var actionResult = new HypermediaCommandResult()
            {
                Success = true,
            };
            return actionResult;
        }

        protected async Task<HypermediaFunctionResult<T>> HandleFunctionResponseAsync<T>(
            TNetworkResponseMessage responseMessage)
            where T : HypermediaClientObject
        {
            await this.EnsureRequestIsSuccessfulAsync(responseMessage);

            var location = this.GetLocation(responseMessage);
            if (location == null)
            {
                throw new Exception("hypermedia function did not return a result resource location.");
            }

            var actionResult = new HypermediaFunctionResult<T>
            {
                Success = true,
                ResultLocation =
                {
                    Uri = location,
                },
            };

            return actionResult;
        }

        protected string ProcessParameters(IList<ParameterDescription> parameterDescriptions, object parameterObject)
        {
            if (parameterObject is null)
            {
                throw new Exception("Parameter is described but not passed by action.");
            }

            var parameterDescription = GetParameterDescription(parameterDescriptions);

            var serializedParameters =
                this.ParameterSerializer.SerializeParameterObject(parameterDescription.Name, parameterObject);
            return serializedParameters;
        }

        protected static ParameterDescription GetParameterDescription(IList<ParameterDescription> parameterDescriptions)
        {
            if (parameterDescriptions.Count == 0)
            {
                throw new Exception("Parameter not described.");
            }

            // todo allow more fields
            if (parameterDescriptions.Count > 1)
            {
                throw new Exception("Only one action parameter is supported.");
            }

            // todo allow more types
            var parameterDescription = parameterDescriptions.First();
            if (!parameterDescription.Type.Equals(DefaultMediaTypes.ApplicationJson))
            {
                throw new Exception("Only one action type 'application/json' is supported.");
            }
            return parameterDescription;
        }

        //protected abstract Task<TNetworkResponseMessage> ResolveAsync(Uri uriToResolve);

        //protected abstract Task<(TNetworkResponseMessage response, bool wasModified)> ResolveWithCheckForModificationAsync(
        //    Uri uriToResolve,
        //    TCacheEntryIdentifier identifier);

        //protected abstract bool HasCacheIdentifier(
        //    TNetworkResponseMessage responseMessage,
        //    out TCacheEntryIdentifier identifier);

        protected abstract Task<TNetworkResponseMessage> SendCommandAsync(
            Uri uri,
            string method,
            string payload = null);

        protected abstract Task EnsureRequestIsSuccessfulAsync(TNetworkResponseMessage responseMessage);

        protected abstract Task<Stream> ResponseAsStreamAsync(TNetworkResponseMessage responseMessage);

        protected abstract Uri GetLocation(TNetworkResponseMessage responseMessage);

        protected void ClearCache()
        {
            this.LinkHcoCache?.Clear();
        }

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