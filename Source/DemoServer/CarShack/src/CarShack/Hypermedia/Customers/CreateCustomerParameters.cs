using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    public class CreateCustomerParameters : IHypermediaActionParameter
    {
        public string Name { get; set; }
    }
}