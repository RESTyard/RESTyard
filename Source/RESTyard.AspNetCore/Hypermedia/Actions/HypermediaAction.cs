using System;
using RESTyard.AspNetCore.Exceptions;

namespace RESTyard.AspNetCore.Hypermedia.Actions
{
    /// <summary>
    /// A HypermediaAction. For each concrete type a corresponding attributed route must exist.
    /// </summary>
    /// <typeparam name="TParameter">Parameter object passed to the Action.</typeparam>
    public class HypermediaAction<TParameter> : HypermediaActionBase where TParameter : class, IHypermediaActionParameter
    {
        /// <summary>
        /// The action may provide pre filled values which are passed to the client so action parameters can be filled with provided values.
        /// </summary>
        public TParameter PrefilledValues { protected set;  get; }

        public HypermediaAction(Func<bool> canExecute, TParameter prefilledValues = null) : base(canExecute)
        {
            PrefilledValues = prefilledValues;
        }

        public HypermediaAction(TParameter prefilledValues = null) : base(()=>true)
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
    /// A HypermediaAction. For each concrete type a corresponding attributed route must exist.
    /// </summary>
    public abstract class HypermediaAction : HypermediaActionBase
    {
        protected HypermediaAction(Func<bool> canExecute) : base(canExecute)
        {
        }

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
