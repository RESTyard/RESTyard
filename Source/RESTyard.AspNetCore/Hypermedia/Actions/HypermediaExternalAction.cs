﻿using System;
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
        public TParameter PrefilledValues { protected set;  get; }

        public HypermediaExternalAction(Func<bool> canExecute,
            Uri externalUri,
            HttpMethod httpMethod,
            string mediaType = DefaultMediaTypes.ApplicationJson,
            TParameter prefilledValues = null) 
            : base(canExecute, externalUri, httpMethod, mediaType)
        {
            PrefilledValues = prefilledValues;
        }

        public HypermediaExternalAction(Uri externalUri,
            HttpMethod httpMethod,
            string mediaType = DefaultMediaTypes.ApplicationJson,
            TParameter prefilledValues = null)
            : base(() => true, externalUri, httpMethod, mediaType)
        {
            PrefilledValues = prefilledValues;
        }

        public override object GetPrefilledParameter()
        {
            return PrefilledValues;
        }

        public override Type ParameterType()
        {
            return typeof(TParameter);
        }
    }

    /// <summary>
    /// A HypermediaAction which will point to an external endpoint.
    /// </summary>
    public abstract class HypermediaExternalAction : HypermediaExternalActionBase
    {
        protected HypermediaExternalAction(Func<bool> canExecute,
            Uri externalUri,
            HttpMethod httpMethod,
            string mediaType = DefaultMediaTypes.ApplicationJson) 
            : base(canExecute, externalUri, httpMethod, mediaType) { }

        protected HypermediaExternalAction(Uri externalUri,
            HttpMethod httpMethod,
            string mediaType = DefaultMediaTypes.ApplicationJson)
            : base(() => true, externalUri, httpMethod, mediaType) { }

        public override object GetPrefilledParameter()
        {
            return null;
        }

        public override Type ParameterType()
        {
            return null;
        }
    }
}
