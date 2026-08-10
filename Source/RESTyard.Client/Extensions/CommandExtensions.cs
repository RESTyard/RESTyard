using System;
using System.Threading;
using System.Threading.Tasks;
using FunicularSwitch;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Extensions
{
    public static class CommandExtensions
    {
        public static async Task<HypermediaResult<Unit>> ExecuteAsync(
            this IHypermediaClientAction action,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            return await resolver.ResolveActionAsync(action.Uri, action.Method, cancellationToken);
        }

        public static async Task<HypermediaResult<Unit>> ExecuteAsync<TParameters>(
            this IHypermediaClientAction<TParameters> action,
            TParameters parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            return await resolver.ResolveActionAsync(
                action.Uri,
                action.Method,
                action.ParameterDescriptions,
                parameters,
                cancellationToken);
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType>(
            this IHypermediaClientFunction<TResultType> function,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver.ResolveFunctionAsync<TResultType>(function.Uri, function.Method, cancellationToken)
                .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType>(
            this IHypermediaClientFunction<TResultType> function,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver
                .ResolveFunctionAsync<TResultType>(function.Uri, function.Method, cancellationToken)
                .Bind(linkOrEntity => ResolveAsyncIfLink(linkOrEntity, cancellationToken));
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType, TParameters>(
            this IHypermediaClientFunction<TResultType, TParameters> function,
            TParameters parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver.ResolveFunctionAsync<TResultType>(
                function.Uri,
                function.Method,
                function.ParameterDescriptions,
                parameters,
                cancellationToken)
                .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType, TParameters>(
            this IHypermediaClientFunction<TResultType, TParameters> function,
            TParameters parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver
                .ResolveFunctionAsync<TResultType>(
                    function.Uri,
                    function.Method,
                    function.ParameterDescriptions,
                    parameters,
                    cancellationToken)
                .Bind(linkOrEntity => ResolveAsyncIfLink(linkOrEntity, cancellationToken));
        }

        public static async Task<HypermediaResult<Unit>> ExecuteAsync(
            this IHypermediaClientFileUploadAction action,
            HypermediaFileUploadActionParameter parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            return await resolver.ResolveActionAsync(
                action.Uri,
                action.Method,
                action.ParameterDescriptions,
                parameters,
                cancellationToken);
        }
        
        public static async Task<HypermediaResult<Unit>> ExecuteAsync<TParameters>(
            this IHypermediaClientFileUploadAction<TParameters> action,
            HypermediaFileUploadActionParameter<TParameters> parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            return await resolver.ResolveActionAsync(
                action.Uri,
                action.Method,
                action.ParameterDescriptions,
                parameters,
                cancellationToken);
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType>(
            this IHypermediaClientFileUploadFunction<TResultType> function,
            HypermediaFileUploadActionParameter parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver.ResolveFunctionAsync<TResultType>(
                function.Uri,
                function.Method,
                function.ParameterDescriptions,
                parameters,
                cancellationToken)
                .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType>(
            this IHypermediaClientFileUploadFunction<TResultType> function,
            HypermediaFileUploadActionParameter parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver
                .ResolveFunctionAsync<TResultType>(
                    function.Uri,
                    function.Method,
                    function.ParameterDescriptions,
                    parameters,
                    cancellationToken)
                .Bind(linkOrEntity => ResolveAsyncIfLink(linkOrEntity, cancellationToken));
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType, TParameters>(
            this IHypermediaClientFileUploadFunction<TResultType, TParameters> function,
            HypermediaFileUploadActionParameter<TParameters> parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver.ResolveFunctionAsync<TResultType>(
                function.Uri,
                function.Method,
                function.ParameterDescriptions,
                parameters,
                cancellationToken)
                .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType, TParameters>(
            this IHypermediaClientFileUploadFunction<TResultType, TParameters> function,
            HypermediaFileUploadActionParameter<TParameters> parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver
                .ResolveFunctionAsync<TResultType>(
                    function.Uri,
                    function.Method,
                    function.ParameterDescriptions,
                    parameters,
                    cancellationToken)
                .Bind(linkOrEntity => ResolveAsyncIfLink(linkOrEntity, cancellationToken));
        }

        private static Task<HypermediaResult<T>> ResolveAsyncIfLink<T>(
            LinkOrEntity<T> linkOrEntity,
            CancellationToken cancellationToken)
            where T : HypermediaClientObject
            => linkOrEntity.Match(
                link: link => link.Value.ResolveAsync(cancellationToken),
                entity: entity => Task.FromResult(HypermediaResult.Ok(entity.Value)));
        
        private static HypermediaResult<MandatoryHypermediaLink<T>> SafeCastToLink<T>(LinkOrEntity<T> linkOrEntity, IHypermediaResolver resolver)
            where T : HypermediaClientObject
            => linkOrEntity.Match(
                link: link => HypermediaResult.Ok(link.Value),
                entity: entity => HypermediaResult.Ok(new MandatoryHypermediaLink<T>()
                {
                    Uri = entity.Location,
                    Resolver = resolver,
                }));
    }
}