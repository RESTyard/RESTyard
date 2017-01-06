using System.Threading.Tasks;
using CarShack.Domain.Customer;
using CarShack.Hypermedia.Customers;
using CarShack.Util;
using Microsoft.AspNetCore.Mvc;
using WebApiHypermediaExtensionsCore.ErrorHandling;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.WebApi;
using WebApiHypermediaExtensionsCore.WebApi.AttributedRoutes;
using WebApiHypermediaExtensionsCore.WebApi.ExtensionMethods;

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
        // This RouteTemplate also contains a key, so a RouteKeyProducer is required.
        [HttpGetHypermediaObject("{key:int}", typeof(HypermediaCustomer), typeof(CustomerRouteKeyProducer))]
        public async Task<ActionResult> GetEntity(int key)
        {
            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(key);
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
        [HttpPostHypermediaAction("{key:int}/MarkAsFavorite", typeof(HypermediaActionCustomerMarkAsFavorite))]
        public async Task<ActionResult> MarkAsFovoriteAction(int key)
        {
            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(key);
                var hypermediaCustomer = new HypermediaCustomer(customer);
                hypermediaCustomer.MarkAsFavoriteAction.Execute();
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

        [HttpPostHypermediaAction("{key:int}/Move", typeof(HypermediaActionCustomerMoveAction))]
        public async Task<ActionResult> CustomerMove(int key, [SingleParameterBinder(typeof(NewAddress))] NewAddress newAddress)
        {
            if (newAddress == null)
            {
                return this.Problem(ProblemJsonBuilder.CreateBadParameters());
            }

            try
            {
                var customer = await customerRepository.GetEnitityByKeyAsync(key);
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
                var problem  = new ProblemJson()
                {
                    Title = $"Can not use provided object of type '{typeof(NewAddress)}'",
                    Detail = e.Message,
                    ProblemType = "WebApiHypermediaExtensionsCore.Hypermedia.BadActionParameter",
                    StatusCode = 422 // Unprocessable Entity
                };
                return this.UnprocessableEntity(problem);
            }
        }
#endregion

#region TypeRoutes
        // Provide type information for Action parameters. Does not depend on a specific customer.
        [HttpGetHypermediaActionParameterInfo("NewAddressType", typeof(NewAddress))]
        public ActionResult NewAddressType()
        {
            var schema = JsonSchemaFactory.Generate(typeof(NewAddress));

            return Ok(schema);
        }
#endregion
    }
}
