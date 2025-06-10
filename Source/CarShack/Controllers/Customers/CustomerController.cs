using System.Net;
using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.WebApi;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace CarShack.Controllers.Customers
{
    [Route("Customers")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository customerRepository;
        private readonly IKeyFromUriService keyFromUriService;

        public CustomerController(
            ICustomerRepository customerRepository,
            IKeyFromUriService keyFromUriService)
        {
            this.customerRepository = customerRepository;
            this.keyFromUriService = keyFromUriService;
        }

        #region HypermediaObjects
        // Route to the HypermediaCustomer. References to HypermediaCustomer type will be resolved to this route.
        // This RouteTemplate also contains a key, so a RouteKeyProducer can be provided. In this case the RouteKeyProducer
        // could be ommited and KeyAttribute could be used on HypermediaCustomer instead.
        [HttpGet("{key:int}"), HypermediaObjectEndpoint<HypermediaCustomerHto>(typeof(CustomerRouteKeyProducer))]
        public async Task<ActionResult> GetEntity(int key)
        {
            try
            {
                var customer = await customerRepository.GetEntityByKeyAsync(key).ConfigureAwait(false);
                var result = HypermediaCustomerHto.FromDomain(customer);
                return Ok(result);
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
        }
        #endregion

        #region Actions
        [HttpPost("MyFavoriteCustomers"), HypermediaActionEndpoint<HypermediaCustomerHto>(nameof(HypermediaCustomerHto.MarkAsFavorite))]
        public async Task<ActionResult> MarkAsFavoriteAction([HypermediaActionParameterFromBody] MarkAsFavoriteParameters favoriteCustomer)
        {
            if (favoriteCustomer == null)
            {
                var problem = new ProblemDetails
                {
                    Title = $"Can not use provided object of type '{typeof(MarkAsFavoriteParameters)}'",
                    Detail = "Json or contained links might be invalid",
                    Type = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                };
                return this.UnprocessableEntity(problem);
            }

            try
            {
                var keyFromUri = this.keyFromUriService.GetKeyFromUri<HypermediaCustomerHto, HypermediaCustomerHto.CustomKey>(favoriteCustomer.Customer);
                if (keyFromUri.IsError)
                {
                    return this.BadRequest();
                }
                var customer = await customerRepository.GetEntityByKeyAsync(keyFromUri.GetValueOrThrow().Key).ConfigureAwait(false);
                var hypermediaCustomer = customer.ToHto();

                // Check can execute here since we need to call business logic and not rely on previously checked value from HTO passed to caller
                if (customer.IsFavorite)
                {
                    return this.CanNotExecute();
                }

                DoMarkAsFavorite(hypermediaCustomer, customer);
                return Ok();
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
            catch (InvalidLinkException e)
            {
                var problem = new ProblemDetails()
                {
                    Title = $"Can not use provided object of type '{typeof(MarkAsFavoriteParameters)}'",
                    Detail = e.Message,
                    Type = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                };
                return this.UnprocessableEntity(problem);
            }
            catch (CanNotExecuteActionException)
            {
                return this.CanNotExecute();
            }
        }

        [HttpPost("{key:int}/BuysCar"), HypermediaActionEndpoint<HypermediaCustomerHto>(nameof(HypermediaCustomerHto.BuyCar))]
        public async Task<ActionResult> BuyCar(int key, BuyCarParameters parameter)
        {
            if (parameter == null)
            {
                var problem = new ProblemDetails
                {
                    Title = $"Can not use provided object of type '{typeof(BuyCarParameters)}'",
                    Detail = "Json or contained links might be invalid",
                    Type = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                };
                return this.UnprocessableEntity(problem);
            }

            try
            {
                //shortcut for get car from repository
                var car = new HypermediaCarHto(parameter.Brand, parameter.CarId);
                var customer = await customerRepository.GetEntityByKeyAsync(key).ConfigureAwait(false);
                //do what has to be done
                return this.Created(Link.ByKey(new HypermediaCarHto.Key(parameter.CarId, parameter.Brand)));
            }
            catch (EntityNotFoundException)
            {
                return this.Problem(ProblemJsonBuilder.CreateEntityNotFound());
            }
            catch (CanNotExecuteActionException)
            {
                return this.CanNotExecute();
            }
        }

        [HttpPost("{key:int}/Moves"), HypermediaActionEndpoint<HypermediaCustomerHto>(nameof(HypermediaCustomerHto.CustomerMove))]
        public async Task<ActionResult> CustomerMove(int key, NewAddress newAddress)
        {
            if (newAddress == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            try
            {
                var customer = await customerRepository.GetEntityByKeyAsync(key).ConfigureAwait(false);
                var hypermediaCustomer = customer.ToHto();
                // Can execute logic is NOT checked, but is always true
                DoMove(hypermediaCustomer, customer, newAddress);
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
                var problem = new ProblemDetails()
                {
                    Title = $"Can not use provided object of type '{typeof(NewAddress)}'",
                    Detail = e.Message,
                    Type = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                };
                return this.UnprocessableEntity(problem);
            }
        }
        
        [HttpDelete("{key:int}"), HypermediaActionEndpoint<HypermediaCustomerHto>(nameof(HypermediaCustomerHto.CustomerRemove))]
        public ActionResult RemoveCustomer(int key)
        {
            try
            {
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
                var problem = new ProblemDetails()
                {
                    Title = $"Can not use provided object of type '{typeof(NewAddress)}'",
                    Detail = e.Message,
                    Type = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                };
                return this.UnprocessableEntity(problem);
            }
        }
        
        private static void DoMarkAsFavorite(HypermediaCustomerHto hto, Customer customer)
        {
            customer.IsFavorite = true;
            hto.IsFavorite = true;
        }
        
        private static void DoMove(HypermediaCustomerHto hto, Customer customer, NewAddress newAddress)
        {
            // semantic validation is business logic
            if (string.IsNullOrEmpty(newAddress.Address.Street))
                throw new ActionParameterValidationException("New customer address may not be null or empty.");

            // call business logic here
            hto.Address = newAddress.Address;
            customer.Address = new Address(
                Street: newAddress.Address.Street,
                Number: newAddress.Address.Number,
                City: newAddress.Address.City,
                ZipCode: newAddress.Address.ZipCode);
        }

        #endregion

        #region TypeRoutes
        // Provide type information for Action parameters. Does not depend on a specific customer. Optional when using
        // MvcOptionsExtension.AutoDeliverActionParameterSchemas
        [HttpGet("NewAddressType"), HypermediaActionParameterInfoEndpoint<NewAddress>]
        public ActionResult NewAddressType()
        {
            var schema = JsonSchemaFactory.Generate(typeof(NewAddress));
            return Ok(schema);
        }
        #endregion
    }
}
