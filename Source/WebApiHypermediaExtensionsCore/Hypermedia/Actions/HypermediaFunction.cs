using System;
using WebApiHypermediaExtensionsCore.Exceptions;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Actions
{
    /// <summary>
    /// A HypermediaFunction. For each concrete type a corresponding attributed route must exist.
    /// </summary>
    /// <typeparam name="TParameter">Parameter object passed to the Action.</typeparam>
    /// <typeparam name="TReturn">Return object.</typeparam>
    public class HypermediaFunction<TParameter, TReturn> : HypermediaActionBase where TParameter : IHypermediaActionParameter
    {
        private readonly Func<TParameter, TReturn>  command;

        // todo in future make both calls async, CanExecute and DoExecute can take long. Should be awaitable
        public HypermediaFunction(Func<bool> canExecute, Func<TParameter, TReturn> command = null) : base (canExecute)
        {
            this.command = command;
        }

        public TReturn Execute(TParameter parameter)
        {
            if (!CanExecute())
            {
                throw new CanNotExecuteActionException("Can not execute.");
            }

            if (command == null)
            {
                throw new NoActionSetException($"No Action set: '{GetType()}'");
            }

            return command(parameter);
        }

        public override bool HasParameter()
        {
            return true;
        }

        public override Type ParameterType()
        {
            return typeof(TParameter);
        }
    }

    /// <summary>
    /// A hypemediaFunction which has no parameters.
    /// </summary>
    /// <typeparam name="TReturn">The return type</typeparam>
    public class HypermediaFunction<TReturn> : HypermediaActionBase
    {
        private readonly Func<TReturn> command;

        public HypermediaFunction(Func<bool> canExecute, Func<TReturn> command = null) : base(canExecute)
        {
            this.command = command;
        }

        public TReturn Execute()
        {
            if (!CanExecute())
            {
                throw new CanNotExecuteActionException("Can not execute.");
            }

            if (command == null)
            {
                throw new NoActionSetException($"No Action set: '{GetType()}'");
            }

            return command();
        }

        public override bool HasParameter()
        {
            return false;
        }

        public override Type ParameterType()
        {
            return null;
        }
    }
}