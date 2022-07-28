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
        private readonly Action<TParameter> command;

        /// <summary>
        /// The action may provide pre filled values which are passed to the client so action parameters can be filled with provided values.
        /// </summary>
        public TParameter PrefilledValues { protected set;  get; }

        public HypermediaAction(Func<bool> canExecute, Action<TParameter> command = null, TParameter prefilledValues = null) : base(canExecute)
        {
            this.command = command;
            this.PrefilledValues = prefilledValues;
        }

        public HypermediaAction(TParameter prefilledValues = null) : base(()=>true)
        {
            this.PrefilledValues = prefilledValues;
        }

        public void Execute(TParameter parameter)
        {
            if (!CanExecute())
            {
                throw new CanNotExecuteActionException("Can not execute.");
            }

            if (command == null)
            {
                throw new NoActionSetException($"No Action set: '{GetType()}'");
            }

            command(parameter);
        }

        public override bool HasParameter()
        {
            return true;
        }

        public override object GetPrefilledParameter()
        {
            return this.PrefilledValues;
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
        private readonly Action command;

        protected HypermediaAction(Func<bool> canExecute, Action command) : base(canExecute)
        {
            this.command = command;
        }

        public void Execute()
        {
            if (!CanExecute())
            {
                throw new CanNotExecuteActionException("Can not execute.");
            }

            if (command == null)
            {
                throw new NoActionSetException($"No Action set: '{GetType()}'");
            }

            command();
        }

        public override bool HasParameter()
        {
            return false;
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
