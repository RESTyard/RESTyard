using System;
using WebApi.HypermediaExtensions.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    public class HypermediaActionRemoveCustomerAction : HypermediaAction
    {
        public HypermediaActionRemoveCustomerAction(Func<bool> canExecute, Action command = null) : base(canExecute, command)
        {
        }
    }
}