using System;
using System.Threading.Tasks;
using WebApi.HypermediaExtensions.Exceptions;

namespace WebApi.HypermediaExtensions.Hypermedia.Actions
{
    /// <summary>
    /// A HypermediaFunction. For each concrete type a corresponding attributed route must exist.
    /// </summary>
    /// <typeparam name="TParameter">Parameter object passed to the Action.</typeparam>
    /// <typeparam name="TReturn">Return object.</typeparam>
    public class HypermediaFunction<TParameter, TReturn> : HypermediaActionBase where TParameter : class, IHypermediaActionParameter
    {
        private readonly Func<TParameter, Task<TReturn>>  command;

        /// <summary>
        /// The action may provide pre filled values which are passed to the client so action parameters can be filled with provided values.
        /// </summary>
        public TParameter PrefilledValues { protected set; get; }

        // todo in future make both calls async, CanExecute and DoExecute can take long. Should be awaitable
        public HypermediaFunction(Func<bool> canExecute, Func<TParameter, TReturn> command = null,
            TParameter prefilledValues = null)
            : this(canExecute, p => Task.FromResult(command(p)), prefilledValues){}

        public HypermediaFunction(Func<bool> canExecute, Func<TParameter, Task<TReturn>> command = null, TParameter prefilledValues = null) : base (canExecute)
        {
            this.command = command;
            this.PrefilledValues = prefilledValues;
        }

        public HypermediaFunction(TParameter prefilledValues) : base(()=> true)
        {
            this.PrefilledValues = prefilledValues;
        }

        public TReturn Execute(TParameter parameter) => ExecuteAsync(parameter).GetAwaiter().GetResult();

        public Task<TReturn> ExecuteAsync(TParameter parameter)
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
    /// A HypemediaFunction which has no parameters.
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