using WebApiHypermediaExtensionsCore.Hypermedia.Actions;

namespace CarShack.Hypermedia.Customers
{
    public class NewAddress : IHypermediaActionParameter
    {
        public string Address { get; set; }
    }
}