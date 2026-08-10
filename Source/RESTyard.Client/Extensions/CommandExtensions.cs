using System;
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
            IHypermediaResolver resolver)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            try
            {
                var result = await resolver.ResolveActionAsync(action.Uri, action.Method);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<Unit>> ExecuteAsync<TParameters>(
            this IHypermediaClientAction<TParameters> action,
            TParameters parameters,
            IHypermediaResolver resolver)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            try
            {
                var result = await resolver.ResolveActionAsync(
                    action.Uri,
                    action.Method,
                    action.ParameterDescriptions,
                    parameters);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType>(
            this IHypermediaClientFunction<TResultType> function,
            IHypermediaResolver resolver)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                return await resolver
                    .ResolveFunctionAsync<TResultType>(function.Uri, function.Method)
                    .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType>(
            this IHypermediaClientFunction<TResultType> function,
            IHypermediaResolver resolver)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            return await resolver
                .ResolveFunctionAsync<TResultType>(function.Uri, function.Method)
                .Bind(ResolveAsyncIfLink);
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType, TParameters>(
            this IHypermediaClientFunction<TResultType, TParameters> function,
            TParameters parameters,
            IHypermediaResolver resolver)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                return await resolver.ResolveFunctionAsync<TResultType>(
                    function.Uri,
                    function.Method,
                    function.ParameterDescriptions,
                    parameters)
                    .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType, TParameters>(
            this IHypermediaClientFunction<TResultType, TParameters> function,
            TParameters parameters,
            IHypermediaResolver resolver)
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
                    parameters)
                .Bind(ResolveAsyncIfLink);
        }

        public static async Task<HypermediaResult<Unit>> ExecuteAsync(
            this IHypermediaClientFileUploadAction action,
            HypermediaFileUploadActionParameter parameters,
            IHypermediaResolver resolver)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            try
            {
                var result = await resolver.ResolveActionAsync(
                    action.Uri,
                    action.Method,
                    action.ParameterDescriptions,
                    parameters);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.Exception(e));
            }
        }
        
        public static async Task<HypermediaResult<Unit>> ExecuteAsync<TParameters>(
            this IHypermediaClientFileUploadAction<TParameters> action,
            HypermediaFileUploadActionParameter<TParameters> parameters,
            IHypermediaResolver resolver)
        {
            if (!action.CanExecute)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            try
            {
                var result = await resolver.ResolveActionAsync(
                    action.Uri,
                    action.Method,
                    action.ParameterDescriptions,
                    parameters);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType>(
            this IHypermediaClientFileUploadFunction<TResultType> function,
            HypermediaFileUploadActionParameter parameters,
            IHypermediaResolver resolver)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                return await resolver
                    .ResolveFunctionAsync<TResultType>(
                        function.Uri,
                        function.Method,
                        function.ParameterDescriptions,
                        parameters)
                    .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType>(
            this IHypermediaClientFileUploadFunction<TResultType> function,
            HypermediaFileUploadActionParameter parameters,
            IHypermediaResolver resolver)
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
                    parameters)
                .Bind(ResolveAsyncIfLink);
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType, TParameters>(
            this IHypermediaClientFileUploadFunction<TResultType, TParameters> function,
            HypermediaFileUploadActionParameter<TParameters> parameters,
            IHypermediaResolver resolver)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                return await resolver
                    .ResolveFunctionAsync<TResultType>(
                        function.Uri,
                        function.Method,
                        function.ParameterDescriptions,
                        parameters)
                    .Bind(linkOrEntity => SafeCastToLink(linkOrEntity, resolver));
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<TResultType>> ExecuteAndResolveAsync<TResultType, TParameters>(
            this IHypermediaClientFileUploadFunction<TResultType, TParameters> function,
            HypermediaFileUploadActionParameter<TParameters> parameters,
            IHypermediaResolver resolver)
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
                    parameters)
                .Bind(ResolveAsyncIfLink);
        }

        private static Task<HypermediaResult<T>> ResolveAsyncIfLink<T>(LinkOrEntity<T> linkOrEntity)
            where T : HypermediaClientObject
            => linkOrEntity.Match(
                link: link => link.Value.ResolveAsync(),
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