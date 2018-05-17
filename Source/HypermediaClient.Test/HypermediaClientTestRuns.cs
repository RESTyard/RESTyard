using System;
using System.Linq;
using System.Threading.Tasks;
using HypermediaClient.Extensions;
using HypermediaClient.Test.Hypermedia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HypermediaClient.Test
{
    using HypermediaClient.Authentication;
    using HypermediaClient.ParameterSerializer;
    using HypermediaClient.Resolver;

    /// <summary>
    /// Tests used during prototyping.
    /// </summary>
    [TestClass]
    public class HypermediaClientTestRuns
    {
        private HypermediaObjectRegister HypermediaObjectRegister { get; set; }
        private HypermediaClient<EntryPointHco> SirenClient { get; set; }
        private static readonly Uri ApiEntryPoint = new Uri("http://localhost:5000/entrypoint");

        // TODO Handle Erors (http, authorization, hypermediaparsing and api), problem json, status codes
        // use chache info from header so client may cache resolved documents

        [TestInitialize]
        public void Initialize()
        {
            this.HypermediaObjectRegister = CreateHypermediaObjectRegister();
            
            var resolver = new HttpHypermediaResolver(new SingleJsonObjectParameterSerializer());
            // be sure to use https
            resolver.SetCredentials(new UsernamePasswordCredentials("User", "Password"));

            var hypermediaReader = new SirenHypermediaReader(HypermediaObjectRegister, resolver);
            this.SirenClient = new HypermediaClient<EntryPointHco>(ApiEntryPoint, resolver, hypermediaReader);
        }

        [TestMethod]
        public async Task EnterEntryPoint()
        {
            var apiRoot = await SirenClient.EnterAsync();
            Assert.IsNotNull(apiRoot);
        }

        [TestMethod]
        public async Task CallAction_CustomerMove()
        {
            var apiRoot = await SirenClient.EnterAsync();
            var customersAll = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);

            var customer = customersAll.Customers.First();

            var newAddress = "New Address";
            var actionResult = await customer.CustomerMove.ExecuteAsync(new NewAddress {Address = newAddress});
            
            customer = await customer.Self.ResolveAsync();
            Assert.IsTrue(actionResult.Success);
            Assert.AreEqual(newAddress, customer.Address);
        }

        [TestMethod]
        public async Task CallAction_MarkAsFavorite()
        {
            var apiRoot = await SirenClient.EnterAsync();
            var customersAll = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);

            var customer = customersAll.Customers.First();
            if (!customer.MarkAsFavorite.CanExecute)
            {
                Assert.Inconclusive("Action can not be run on server, not offered.");
            }

            var actionResult = await customer.MarkAsFavorite.ExecuteAsync(new FavoriteCustomer{ CustomerLink = customer.Self.Uri.ToString() }); 

            customer = await customer.Self.ResolveAsync();
            Assert.IsTrue(actionResult.Success);
            Assert.IsTrue(customer.IsFavorite);
        }

        [TestMethod]
        public async Task CallAction_CreateQuery()
        {
            var apiRoot = await SirenClient.EnterAsync();
            var customersRoot = await apiRoot.NavigateAsync(l => l.Customers);

            var query = new CustomersQuery
            {
                Filter = new CustomerFilter {MinAge = 22},
                SortBy = new SortOptions { PropertyName = "Age", SortType  = "Ascending" },
                Pagination = new Pagination { PageOffset = 2, PageSize = 3}
            };

            var resultResource = await customersRoot.CreateQuery.ExecuteAsync(query);
            var queryResultPage = await resultResource.ResultLocation.ResolveAsync();
            Assert.IsNotNull(queryResultPage);
        }

        [TestMethod]
        public async Task EnterEntryPointAndNavigate()
        {
            var apiRoot = await SirenClient.EnterAsync();
            var customers = await apiRoot.Customers.ResolveAsync();
            var all = await customers.All.ResolveAsync();

            var allFluent = await SirenClient.EnterAsync().NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);
            var allFluent2 = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);
            var optionalFluent = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All).NavigateAsync(l => l.Next);
        }

        private static HypermediaObjectRegister CreateHypermediaObjectRegister()
        {
            var hypermediaObjectRegister = new HypermediaObjectRegister();
            hypermediaObjectRegister.Register(typeof(EntryPointHco));
            hypermediaObjectRegister.Register(typeof(CustomerHco));
            hypermediaObjectRegister.Register(typeof(CustomersRootHco));
            hypermediaObjectRegister.Register(typeof(CustomerQueryResultHco));
            return hypermediaObjectRegister;
        }
    }
}
