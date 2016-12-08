using System.Threading.Tasks;
using CarShack.Domain.Customer;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.Hypermedia.Actions;
using WebApiHypermediaExtensionsCore.Hypermedia.Attributes;
using WebApiHypermediaExtensionsCore.Hypermedia.Links;

namespace CarShack.Hypermedia.Customers
{
    [HypermediaObject(Title = "The Customers API", Classes = new[] { "CustomersRoot" })]
    public class HypermediaCustomersRoot : HypermediaObject
    {
        private readonly ICustomerRepository customerRepository;

        // Add actions:
        // Each ActionType must be unique and a corresponding route must exist so the formatter can look it up.
        // See the CustomersRootController.
        [HypermediaAction(Name = "CreateCustomer", Title = "Request creation of a new Customer.")]
        public HypermediaAction<CreateCustomerParameters, Task<Customer>> CreateCustomerAction { get; private set; }

        [HypermediaAction(Name = "CreateQuery", Title = "Query the Customers collection.")]
        public HypermediaAction<CustomerQuery> CreateQueryAction { get; private set; }

        public HypermediaCustomersRoot(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;

            CreateCustomerAction = new HypermediaAction<CreateCustomerParameters, Task<Customer>>(CanCreateCustomer, DoCreateCustomer);
            CreateQueryAction = new HypermediaAction<CustomerQuery>(CanNewQuery);

            // Add Links:
            var allQuery = new CustomerQuery();
            Links.Add(DefaultHypermediaRelations.Queries.All, new HypermediaObjectQueryReference(typeof(HypermediaCustomerQueryResult), allQuery));

            // This Link uses a reference to a HypermediaObject without actually building it. It Gives the type and the value which is used do identify the Entity.
            // The key will be used while resolving routes in the Formatter.
            // links to the HypermediaCustomer with Id = 1
            Links.Add("BestCustomer", new HypermediaObjectKeyReference(typeof(HypermediaCustomer), 1));
        }

        // Will be called to determine if tis action is available at the moment/current state.
        private bool CanNewQuery()
        {
            return true;
        }

        private async Task<Customer> DoCreateCustomer(CreateCustomerParameters arg)
        {
            var customer = CustomerService.CreateRandomCustomer();
            await customerRepository.AddEntityAsync(customer);

            return customer;
        }

        private bool CanCreateCustomer()
        {
            return true;
        }
    }
}
