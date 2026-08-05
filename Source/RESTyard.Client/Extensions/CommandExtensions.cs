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
                return HypermediaResult.Error<Unit>(HypermediaProblem.InvalidRequest("Can not execute Action."));
            }

            try
            {
                var result = await resolver.ResolveActionAsync(action.Uri, action.Method, cancellationToken);
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
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
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
                    parameters,
                    cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<Unit>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<MandatoryHypermediaLink<TResultType>>> ExecuteAsync<TResultType>(
            this IHypermediaClientFunction<TResultType> function,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                var result = await resolver.ResolveFunctionAsync<TResultType>(function.Uri, function.Method, cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
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
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                var result = await resolver.ResolveFunctionAsync<TResultType>(
                    function.Uri,
                    function.Method,
                    function.ParameterDescriptions,
                    parameters,
                    cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
        }

        public static async Task<HypermediaResult<Unit>> ExecuteAsync(
            this IHypermediaClientFileUploadAction action,
            HypermediaFileUploadActionParameter parameters,
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
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
                    parameters,
                    cancellationToken);
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
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
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
                    parameters,
                    cancellationToken);
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
            IHypermediaResolver resolver,
            CancellationToken cancellationToken = default)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                var result = await resolver.ResolveFunctionAsync<TResultType>(
                    function.Uri,
                    function.Method,
                    function.ParameterDescriptions,
                    parameters,
                    cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
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
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.InvalidRequest("Can not execute Function."));
            }

            try
            {
                var result = await resolver.ResolveFunctionAsync<TResultType>(
                    function.Uri,
                    function.Method,
                    function.ParameterDescriptions,
                    parameters,
                    cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                return HypermediaResult.Error<MandatoryHypermediaLink<TResultType>>(HypermediaProblem.Exception(e));
            }
        }
    }
}