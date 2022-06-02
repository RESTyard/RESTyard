using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia;
using CarShack.Hypermedia.Cars;
using CarShack.Util;
using Microsoft.AspNetCore.Http;
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
        [HttpPostHypermediaAction("MyFavoriteCustomers", typeof(MarkAsFavorite))]
        public async Task<ActionResult> MarkAsFavoriteAction([HypermediaActionParameterFromBody]MarkAsFavoriteParameters favoriteCustomer)
        {
            if (favoriteCustomer == null)
            {
                var problem = new ProblemJson
                {
                    Title = $"Can not use provided object of type '{typeof(MarkAsFavoriteParameters)}'",
                    Detail = "Json or contained links might be invalid",
                    ProblemType = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    StatusCode = 422 // Unprocessable Entity
                };
                return this.UnprocessableEntity(problem);
            }

            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(favoriteCustomer.CustomerId).ConfigureAwait(false);
                var hypermediaCustomer = customer.ToHto();
                hypermediaCustomer.MarkAsFavorite.Execute(favoriteCustomer);
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
                    Title = $"Can not use provided object of type '{typeof(MarkAsFavoriteParameters)}'",
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

        [HttpPostHypermediaAction("{key:int}/BuysCar", typeof(BuyCar))]
        public async Task<ActionResult> BuyCar(int key, BuyCarParameters parameter)
        {
            if (parameter == null)
            {
                var problem = new ProblemJson
                {
                    Title = $"Can not use provided object of type '{typeof(BuyCarParameters)}'",
                    Detail = "Json or contained links might be invalid",
                    ProblemType = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    StatusCode = 422 // Unprocessable Entity
                };
                return this.UnprocessableEntity(problem);
            }

            try
            {
                //shortcut for get car from repository
                var car = new HypermediaCarHto(parameter.Brand, parameter.CarId);
                var customer = await customerRepository.GetEnitityByKeyAsync(key).ConfigureAwait(false);
                //do what has to be done
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
        }

        [HttpPostHypermediaAction("{key:int}/Moves", typeof(CustomerMove), typeof(CustomerRouteKeyProducer))]
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
                hypermediaCustomer.CustomerMove.Execute(newAddress);
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
        
        [HttpDeleteHypermediaAction("{key:int}", 
            typeof(CustomerRemove), 
            typeof(CustomerRouteKeyProducer))]
        public async Task<ActionResult> RemoveCustomer(int key)
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
        // Provide type information for Action parameters. Does not depend on a specific customer. Optional when using
        // MvcOptionsExtension.AutoDeliverActionParameterSchemas
        [HttpGetHypermediaActionParameterInfo("NewAddressType", typeof(NewAddress))]
        public async Task<ActionResult> NewAddressType()
        {
            var schema = await JsonSchemaFactory.Generate(typeof(NewAddress)).ConfigureAwait(false);
            return Ok(schema);
        }
        #endregion
    }
}
