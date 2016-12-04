using System;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    // Action on a HypermediaCustoemr which must be posted to the corresponding route. See CustomerController
    public class HypermediaActionCustomerMarkAsFavorite : HypermediaAction
    {
        public HypermediaActionCustomerMarkAsFavorite(Func<bool> canExecute, Action command) : base(canExecute, command)
        {
        }
    }
}