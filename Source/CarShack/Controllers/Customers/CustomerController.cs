using System.Net;
using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.WebApi;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

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
        [HttpGetHypermediaObject("{key:int}", typeof(HypermediaCustomerHto), typeof(CustomerRouteKeyProducer))]
        public async Task<ActionResult> GetEntity(int key)
        {
            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(key).ConfigureAwait(false);
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
        [HttpPostHypermediaAction("MyFavoriteCustomers", typeof(HypermediaCustomerHto.MarkAsFavoriteOp))]
        public async Task<ActionResult> MarkAsFavoriteAction([HypermediaActionParameterFromBody]MarkAsFavoriteParameters favoriteCustomer)
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
                var customer = await customerRepository.GetEnitityByKeyAsync(favoriteCustomer.CustomerId).ConfigureAwait(false);
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

        [HttpPostHypermediaAction("{key:int}/BuysCar", typeof(HypermediaCustomerHto.BuyCarOp))]
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
                var customer = await customerRepository.GetEnitityByKeyAsync(key).ConfigureAwait(false);
                //do what has to be done
                return this.Created(new HypermediaObjectKeyReference(typeof(HypermediaCarHto), new HypermediaCarHto.Key(parameter.CarId, parameter.Brand)));
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

        [HttpPostHypermediaAction("{key:int}/Moves", typeof(HypermediaCustomerHto.CustomerMoveOp), typeof(CustomerRouteKeyProducer))]
        public async Task<ActionResult> CustomerMove(int key, NewAddress newAddress)
        {
            if (newAddress == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(key).ConfigureAwait(false);
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
        
        [HttpDeleteHypermediaAction("{key:int}", 
            typeof(HypermediaCustomerHto.CustomerRemoveOp), 
            typeof(CustomerRouteKeyProducer))]
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
            // semantic validation is busyness logic
            if (string.IsNullOrEmpty(newAddress.Address))
                throw new ActionParameterValidationException("New customer address may not be null or empty.");

            // call busyness logic here
            customer.Address = newAddress.Address;
            hto.Address = customer.Address;
        }

        #endregion

        #region TypeRoutes
        // Provide type information for Action parameters. Does not depend on a specific customer. Optional when using
        // MvcOptionsExtension.AutoDeliverActionParameterSchemas
        [HttpGetHypermediaActionParameterInfo("NewAddressType", typeof(NewAddress))]
        public ActionResult NewAddressType()
        {
            var schema = JsonSchemaFactory.Generate(typeof(NewAddress));
            return Ok(schema);
        }
        #endregion
    }
}
