using System;
using System.Linq;
using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia.Customers;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.ErrorHandling;
using WebApi.HypermediaExtensions.Exceptions;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.WebApi;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;

namespace CarShack.Controllers.Customers
{
    [Route("Customers")]
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            this.customerRepository = customerRepository;
        }

        #region HypermediaObjects
        // Route to the HypermediaCustomer. References to HypermediaCustomer type will be resolved to this route.
        // This RouteTemplate also contains a key, so a RouteKeyProducer can be provided. In this case the RouteKeyProducer
        // could be ommited and KeyAttribute could be used on HypermediaCustomer instead.
        [HttpGetHypermediaObject("{key:int}", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]
        public async Task<ActionResult> GetEntity(int key)
        {
            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(key).ConfigureAwait(false);
                var result = new HypermediaCustomer(customer);
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
        }
        #endregion

        #region Actions
        [HttpPostHypermediaAction("MyFavoriteCustomers", typeof(HypermediaActionCustomerMarkAsFavorite))]
        public async Task<ActionResult> MarkAsFavoriteAction(FavoriteCustomer favoriteCustomer)
        {
            try
            {
                //var id =  ExtractIdFromCustomerUri(favoriteCustomer.CustomerLink);
                var customer = await customerRepository.GetEnitityByKeyAsync(favoriteCustomer.CustomerId).ConfigureAwait(false);
                var hypermediaCustomer = new HypermediaCustomer(customer);
                hypermediaCustomer.MarkAsFavoriteAction.Execute(favoriteCustomer);
                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
            catch (InvalidLinkException e)
            {
                var problem = new ProblemJson()
                {
                    Title = $"Can not use provided object of type '{typeof(FavoriteCustomer)}'",
                    Detail = e.Message,
                    ProblemType = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    StatusCode = 422 // Unprocessable Entity
                };
                return this.UnprocessableEntity(problem);
            }
            catch (CanNotExecuteActionException)
            {
                return this.CanNotExecute();
            }
        }

        private int ExtractIdFromCustomerUri(string favoriteCustomerCustomerLink)
        {
            if (string.IsNullOrWhiteSpace(favoriteCustomerCustomerLink))
            {
                throw new InvalidLinkException($"Provided Link is empty '{favoriteCustomerCustomerLink}'");
            }
            var lastSegment = favoriteCustomerCustomerLink.Split('/').Last();

            try
            {
                return Convert.ToInt16(lastSegment);
            }
            catch (Exception)
            {
                throw new InvalidLinkException($"Provided Link is invalid '{favoriteCustomerCustomerLink}', provide propper self link.");
            }
        }

        [HttpPostHypermediaAction("{key:int}/Moves", typeof(HypermediaActionCustomerMoveAction), typeof(CustomerRouteKeyProducer))]
        public async Task<ActionResult> CustomerMove(int key, [SingleParameterBinder(typeof(NewAddress))] NewAddress newAddress)
        {
            if (newAddress == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(key).ConfigureAwait(false);
                var hypermediaCustomer = new HypermediaCustomer(customer);
                hypermediaCustomer.MoveAction.Execute(newAddress);
                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
            catch (CanNotExecuteActionException)
            {
                return this.CanNotExecute();
            }
            catch (ActionParameterValidationException e)
            {
                var problem = new ProblemJson()
                {
                    Title = $"Can not use provided object of type '{typeof(NewAddress)}'",
                    Detail = e.Message,
                    ProblemType = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    StatusCode = 422 // Unprocessable Entity
                };
                return this.UnprocessableEntity(problem);
            }
        }
        #endregion

        #region TypeRoutes
        // Provide type information for Action parameters. Does not depend on a specific customer.
        [HttpGetHypermediaActionParameterInfo("NewAddressType", typeof(NewAddress))]
        public async Task<ActionResult> NewAddressType()
        {
            var schema = await JsonSchemaFactory.Generate(typeof(NewAddress)).ConfigureAwait(false);
            return Ok(schema);
        }
        #endregion
    }
}
