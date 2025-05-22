using System;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.Hypermedia.Actions
{
    /// <summary>
    /// A HypermediaAction which will point to an external endpoint.
    /// </summary>
    /// <typeparam name="TParameter">Parameter object passed to the Action.</typeparam>
    public class HypermediaExternalAction<TParameter> : HypermediaExternalActionBase where TParameter : class, IHypermediaActionParameter
    {
        /// <summary>
        /// The action may provide pre filled values which are passed to the client so action parameters can be filled with provided values.
        /// </summary>
        public TParameter? PrefilledValues { protected set;  get; }

        [Obsolete($"Please use overload without {nameof(HttpMethod)} enum")]
        public HypermediaExternalAction(
            Func<bool> canExecute,
            Uri externalUri,
            HttpMethod httpMethod,
            string acceptedMediaType = DefaultMediaTypes.ApplicationJson,
            TParameter? prefilledValues = null) 
            : this(canExecute, externalUri, httpMethod.ToString(), acceptedMediaType, prefilledValues)
        {
        }
        
        public HypermediaExternalAction(
            Func<bool> canExecute,
            Uri externalUri,
            string httpMethod,
            string acceptedMediaType = DefaultMediaTypes.ApplicationJson,
            TParameter? prefilledValues = null) 
            : base(canExecute, externalUri, httpMethod, acceptedMediaType)
        {
            PrefilledValues = prefilledValues;
        }

        [Obsolete($"Please use overload without {nameof(HttpMethod)} enum")]
        public HypermediaExternalAction(
            Uri externalUri,
            HttpMethod httpMethod,
            string acceptedMediaType = DefaultMediaTypes.ApplicationJson,
            TParameter? prefilledValues = null)
            : this(() => true, externalUri, httpMethod.ToString(), acceptedMediaType, prefilledValues)
        {
        }
        
        public HypermediaExternalAction(
            Uri externalUri,
            string httpMethod,
            string acceptedMediaType = DefaultMediaTypes.ApplicationJson,
            TParameter? prefilledValues = null)
            : base(() => true, externalUri, httpMethod, acceptedMediaType)
        {
            PrefilledValues = prefilledValues;
        }

        public override object? GetPrefilledParameter()
        {
            return PrefilledValues;
        }

        protected override Type? ParameterType => typeof(TParameter);
    }

    /// <summary>
    /// A HypermediaAction which will point to an external endpoint.
    /// </summary>
    public abstract class HypermediaExternalAction : HypermediaExternalActionBase
    {
        [Obsolete($"Please use overload without {nameof(HttpMethod)} enum")]
        protected HypermediaExternalAction(
            Func<bool> canExecute,
            Uri externalUri,
            HttpMethod httpMethod) 
            : this(canExecute, externalUri, httpMethod.ToString()) { }
        
        protected HypermediaExternalAction(
            Func<bool> canExecute,
            Uri externalUri,
            string httpMethod) 
            : base(canExecute, externalUri, httpMethod) { }

        [Obsolete($"Please use overload without {nameof(HttpMethod)} enum")]
        protected HypermediaExternalAction(
            Uri externalUri,
            HttpMethod httpMethod)
            : this(() => true, externalUri, httpMethod.ToString()) { }
        
        protected HypermediaExternalAction(
            Uri externalUri,
            string httpMethod)
            : base(() => true, externalUri, httpMethod) { }

        public override object? GetPrefilledParameter()
        {
            return null;
        }

        protected sealed override Type? ParameterType => null;
    }
}
