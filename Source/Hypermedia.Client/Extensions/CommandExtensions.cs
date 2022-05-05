using System;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Extensions
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
    }
}