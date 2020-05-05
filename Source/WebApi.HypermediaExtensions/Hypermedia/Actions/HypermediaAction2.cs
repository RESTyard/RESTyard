namespace WebApi.HypermediaExtensions.Hypermedia.Actions
{
    /// <summary>
    ///  Action that does not return anything and has no parameter
    /// </summary>
    public class HypermediaAction2 : HypermediaOperationBase
    {
        public static HypermediaAction2 Available()
        {
            return new HypermediaAction2 {IsAvailable = true};
        }

        public static HypermediaAction2 NotAvailable()
        {
            return new HypermediaAction2();
        }
    }

    /// <summary>
    ///  Action that does not return anything and has a Parameter object
    /// </summary>
    public class HypermediaAction2<Paramerter> : HypermediaOperationBase
    {
        public static HypermediaAction2<Paramerter> Available()
        {
            return new HypermediaAction2<Paramerter> { IsAvailable = true };
        }

        public static HypermediaAction2<Paramerter> NotAvailable()
        {
            return new HypermediaAction2<Paramerter>();
        }
    }

    /// <summary>
    ///  Action that does have a Return and Parameter object
    /// </summary>
    public class HypermediaFunction2<Paramerter, Return> : HypermediaOperationBase
    {
        public static HypermediaFunction2<Paramerter, Return> Available()
        {
            return new HypermediaFunction2<Paramerter, Return> { IsAvailable = true };
        }

        public static HypermediaFunction2<Paramerter, Return> NotAvailable()
        {
            return new HypermediaFunction2<Paramerter, Return>();
        }
    }

    /// <summary>
    ///  Action that does have a Return
    /// </summary>
    public class HypermediaFunction2<Return> : HypermediaOperationBase
    {
        public static HypermediaFunction2<Return> Available()
        {
            return new HypermediaFunction2<Return> { IsAvailable = true };
        }

        public static HypermediaFunction2<Return> NotAvailable()
        {
            return new HypermediaFunction2<Return>();
        }
    }

    public abstract class HypermediaOperationBase
    {
        public bool IsAvailable { get; protected set; } = false;
    }
}