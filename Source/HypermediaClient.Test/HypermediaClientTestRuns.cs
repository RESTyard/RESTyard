using System;
using System.Linq;
using System.Threading.Tasks;
using HypermediaClient.Extensions;
using HypermediaClient.Test.Hypermedia;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HypermediaClient.Test
{
    /// <summary>
    /// Tests used during prototyping.
    /// </summary>
    [TestClass]
    public class HypermediaClientTestRuns
    {
        private static Uri ApiEntryPoint = new Uri("http://localhost:5000/entrypoint");

        [TestMethod]
        public async Task EnterEntryPoint()
        {
            
            var hypermediaObjectRegister = CreateHypermediaObjectRegister();

            var sirenClient = new SirenHttpHypermediaClient<EntryPointHco>(ApiEntryPoint, hypermediaObjectRegister); 
            var apiRoot = await sirenClient.EnterAsync();

            // TODO Handle Erors (http, authorization, hypermediaparsing and api), problem json, status codes
            // use chache info from header so client may cache resolved documents
            
        }

        [TestMethod]
        public async Task CallAction_CustomerMove()
        {
            var hypermediaObjectRegister = CreateHypermediaObjectRegister();

            var sirenClient = new SirenHttpHypermediaClient<EntryPointHco>(ApiEntryPoint, hypermediaObjectRegister);
            var apiRoot = await sirenClient.EnterAsync();
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
            var hypermediaObjectRegister = CreateHypermediaObjectRegister();

            var sirenClient = new SirenHttpHypermediaClient<EntryPointHco>(ApiEntryPoint, hypermediaObjectRegister);
            var apiRoot = await sirenClient.EnterAsync();
            var customersAll = await apiRoot.NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);

            var customer = customersAll.Customers.First();
            if (!customer.MarkAsFavorite.CanExecute)
            {
                Assert.Inconclusive("Action can not be run on server, not offered.");
            }

            var actionResult = await customer.MarkAsFavorite.ExecuteAsync();

            customer = await customer.Self.ResolveAsync();
            Assert.IsTrue(actionResult.Success);
            Assert.IsTrue(customer.IsFavorite);
        }

        [TestMethod]
        public async Task CallAction_CreateQuery()
        {
            var hypermediaObjectRegister = CreateHypermediaObjectRegister();

            var sirenClient = new SirenHttpHypermediaClient<EntryPointHco>(ApiEntryPoint, hypermediaObjectRegister);
            var apiRoot = await sirenClient.EnterAsync();
            var customersRoot = await apiRoot.NavigateAsync(l => l.Customers);

            var query = new CustomersQuery
            {
                Filter = new CustomerFilter {MinAge = 22},
                SortBy = new SortOptions { PropertyName = "Age", SortType  = "Ascending" },
                Pagination = new Pagination { PageOffset = 2, PageSize = 3}
            };

            var resultResource = await customersRoot.CreateQuery.ExecuteAsync(query);
            var queryResultPage = await resultResource.ResultLocation.ResolveAsync();
        }

        [TestMethod]
        public async Task EnterEntryPointAndNavigate()
        {
            var hypermediaObjectRegister = CreateHypermediaObjectRegister();

            var sirenClient = new SirenHttpHypermediaClient<EntryPointHco>(ApiEntryPoint, hypermediaObjectRegister); 
            var apiRoot = await sirenClient.EnterAsync();
            var customers = await apiRoot.Customers.ResolveAsync();
            var all = await customers.All.ResolveAsync();

            var allFluent = await sirenClient.EnterAsync().NavigateAsync(l => l.Customers).NavigateAsync(l => l.All);
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
