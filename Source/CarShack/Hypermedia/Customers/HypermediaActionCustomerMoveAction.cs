using System;
using WebApi.HypermediaExtensions.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    // Action on a HypermediaCustomer which requires a parameter (NewAdress) which must be posted to the corresponding route. See CustomerController
    public class HypermediaActionCustomerMoveAction : HypermediaAction<NewAddress>
    {
        public HypermediaActionCustomerMoveAction(Func<bool> canExecute, Action<NewAddress> command = null) : base(canExecute, command)
        {
        }
    }
    // Action on a HypermediaCustomer which requires a delete-verb the corresponding route. See CustomerController
}