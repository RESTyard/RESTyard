using System;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Extensions
{
    public static class CommandExtensions
    {
        public static async Task<HypermediaCommandResult> ExecuteAsync(
            this IHypermediaClientAction action,
            IHypermediaResolver resolver)
        {
            if (action.CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            var result = await resolver.ResolveActionAsync(action.Uri, action.Method);
            return result;
        }

        public static async Task<HypermediaCommandResult> ExecuteAsync<TParameters>(
            this IHypermediaClientAction<TParameters> action,
            TParameters parameters,
            IHypermediaResolver resolver)
        {
            if (!action.CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            var result = await resolver.ResolveActionAsync(
                action.Uri,
                action.Method,
                action.ParameterDescriptions,
                parameters);
            return result;
        }

        public static async Task<HypermediaFunctionResult<TResultType>> ExecuteAsync<TResultType>(
            this IHypermediaClientFunction<TResultType> function,
            IHypermediaResolver resolver)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                throw new Exception("Can not execute Function.");
            }

            var result = await resolver.ResolveFunctionAsync<TResultType>(function.Uri, function.Method);
            return result;
        }

        public static async Task<HypermediaFunctionResult<TResultType>> ExecuteAsync<TResultType, TParameters>(
            this IHypermediaClientFunction<TResultType, TParameters> function,
            TParameters parameters,
            IHypermediaResolver resolver)
            where TResultType : HypermediaClientObject
        {
            if (!function.CanExecute)
            {
                throw new Exception("Can not execute Function.");
            }

            var result = await resolver.ResolveFunctionAsync<TResultType>(
                function.Uri,
                function.Method,
                function.ParameterDescriptions,
                parameters);
            return result;
        }
        
        public static async Task<HypermediaCommandResult> ExecuteAsync(
            this IHypermediaClientFileUploadAction action,
            string fileContent, // todo make stream
            IHypermediaResolver resolver)
        {
            if (!action.CanExecute)
            {
                throw new Exception("Can not execute Action.");
            }

            var result = await resolver.ResolveActionAsync(
                action.Uri,
                action.Method,
                action.ParameterDescriptions,
                parameters);
            return result;
        }
    }
}