using System;
using System.Net;
using Bluehands.Hypermedia.MediaTypes;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.ErrorHandling;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Links;
using WebApi.HypermediaExtensions.Query;

namespace WebApi.HypermediaExtensions.WebApi.ExtensionMethods
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns a 201 Created and puts a Location in the header pointing to the HypermediaObject.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="hypermediaObject">The created HypermediaObject</param>
        /// <returns></returns>
        public static ActionResult Created(this ControllerBase controller, HypermediaObject hypermediaObject)
        {
            return controller.Ok(new HypermediaEntityLocation(new HypermediaObjectReference(hypermediaObject), HttpStatusCode.Created));
        }

        /// <summary>
        /// Returns a 201 Created and puts a Location in the header pointing to the HypermediaObject.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="hypermediaObjectReferenceBase">Reference to the created HypermediaObject</param>
        /// <returns></returns>
        public static ActionResult Created(this ControllerBase controller, HypermediaObjectReferenceBase hypermediaObjectReferenceBase)
        {
            return controller.Ok(new HypermediaEntityLocation(hypermediaObjectReferenceBase, HttpStatusCode.Created));
        }

        /// <summary>
        /// Returns a 201 Created and puts a Location to the Query result in the header.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="queryType">The type of the QueryResult which will be returned when following the Location header.</param>
        /// <param name="queryParameter">The query which was requested. Used by the Formatter to produce the Location header.</param>
        /// <returns></returns>
        public static ActionResult CreatedQuery(this ControllerBase controller, Type queryType, IHypermediaQuery queryParameter = null)
        {
            return controller.Ok(new HypermediaQueryLocation(queryType, queryParameter));
        }

        /// <summary>
        /// Return a ProblemJson. Status code wil be set according to the ProbkemJson.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="problemJson">The Problem description.</param>
        /// <returns></returns>
        public static ActionResult Problem(this ControllerBase controller, ProblemJson problemJson)
        {
            var objectResult = new ObjectResult(problemJson) {StatusCode = problemJson.StatusCode};
            objectResult.ContentTypes.Add(DefaultMediaTypes.ProblemJson);
            return objectResult;
        }

        /// <summary>
        /// Indicates that the provided ActionParameters were tecnicaly correct but the internal validation (in the bussiness logic) did not accept the parameters. 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="problemJson">Optional Problem Json.</param>
        /// <returns></returns>
        public static ActionResult UnprocessableEntity(this ControllerBase controller, ProblemJson problemJson = null)
        {
            if (problemJson == null)
            {
                problemJson = new ProblemJson
                {
                    Title = "Can not use provided object",
                    Detail = "",
                    ProblemType = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                    StatusCode = 422 // Unprocessable Entity
                };
            }
            return new ObjectResult(problemJson) { StatusCode = problemJson.StatusCode };
        }

        /// <summary>
        /// The action which was requested can not be executed. Migth have changed state since received the Hypermedia.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="problemJson"></param>
        /// <returns></returns>
        public static ActionResult CanNotExecute(this ControllerBase controller, ProblemJson problemJson = null)
        {
            if (problemJson == null)
            {
                problemJson = new ProblemJson
                {
                    Title = "Can not execute Action",
                    Detail = "",
                    ProblemType = "WebApi.HypermediaExtensions.Hypermedia.ActionNotAvailable",
                    StatusCode = 400
                };
            }

            return new ObjectResult(problemJson) { StatusCode = problemJson.StatusCode };
        }
    }
}
