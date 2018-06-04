using System;
using WebApi.HypermediaExtensions.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    // Action on a HypermediaCustoemr which must be posted to the corresponding route. See CustomerController
    public class HypermediaActionCustomerMarkAsFavorite : HypermediaAction<FavoriteCustomer>
    {
        public HypermediaActionCustomerMarkAsFavorite(Func<bool> canExecute, Action<FavoriteCustomer> command) : base(canExecute, command)
        {
        }
    }
}