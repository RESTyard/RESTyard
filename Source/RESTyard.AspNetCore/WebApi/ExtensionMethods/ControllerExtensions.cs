﻿using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns a 201 Created and puts a Location in the header pointing to the HypermediaObject.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="hypermediaObject">The created HypermediaObject</param>
        /// <returns></returns>
        [Obsolete("use Created(ILink)")]
        public static ActionResult Created(this ControllerBase controller, IHypermediaObject hypermediaObject)
        {
            return controller.Ok(new HypermediaEntityLocation(new HypermediaObjectReference(hypermediaObject), HttpStatusCode.Created));
        }

        /// <summary>
        /// Returns a 201 Created and puts a Location in the header pointing to the HypermediaObject.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="hypermediaObjectReferenceBase">Reference to the created HypermediaObject</param>
        /// <returns></returns>
        [Obsolete("use Created(ILink)")]
        public static ActionResult Created(this ControllerBase controller, HypermediaObjectReferenceBase hypermediaObjectReferenceBase)
        {
            return controller.Ok(new HypermediaEntityLocation(hypermediaObjectReferenceBase, HttpStatusCode.Created));
        }

        public static ActionResult Created(this ControllerBase controller, ILink link)
        {
            return controller.Ok(new HypermediaLinkLocation(link, HttpStatusCode.Created));
        }

        /// <summary>
        /// Returns a 201 Created and puts a Location to the Query result in the header.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="queryType">The type of the QueryResult which will be returned when following the Location header.</param>
        /// <param name="queryParameter">The query which was requested. Used by the Formatter to produce the Location header.</param>
        /// <returns></returns>
        [Obsolete("use Created(ILink) with Link.ByQuery<THto>()")]
        public static ActionResult CreatedQuery(this ControllerBase controller, Type queryType, IHypermediaQuery? queryParameter = null)
        {
            return controller.Ok(new HypermediaQueryLocation(queryType, queryParameter));
        }

        /// <summary>
        /// Returns a 201 Created and puts a Location to the Query result in the header
        /// </summary>
        /// <typeparam name="TQueryResult">The type of the QueryResult which will be returned when following the Location header.</typeparam>
        /// <param name="controller"></param>
        /// <param name="queryParameter"></param>
        /// <returns></returns>
        [Obsolete("use Created(ILink) with Link.ByQuery<TQueryResult>()")]
        public static IActionResult CreatedQuery<TQueryResult>(
            this ControllerBase controller,
            IHypermediaQuery? queryParameter = null)
            where TQueryResult : HypermediaQueryResult
        {
            return controller.Ok(new HypermediaQueryLocation(typeof(TQueryResult), queryParameter));
        }

        /// <summary>
        /// Return a ProblemJson as defined in https://tools.ietf.org/html/rfc7807. Status code will be set according to the ProblemDetails.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="problemDetails">The Problem details.</param>
        public static ActionResult Problem(this ControllerBase controller, ProblemDetails problemDetails)
        {
            var objectResult = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            objectResult.ContentTypes.Add(DefaultMediaTypes.ProblemJson);
            return objectResult;
        }

        /// <summary>
        /// Indicates that the provided ActionParameters were technically correct but the internal validation (in the business logic) did not accept the parameters. 
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static ActionResult UnprocessableEntity(this ControllerBase controller)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Can not use provided object",
                Detail = "",
                Type = "WebApi.HypermediaExtensions.Hypermedia.BadActionParameter",
                Status = (int)HttpStatusCode.UnprocessableEntity,
            };

            return controller.Problem(problemDetails);
        }

        /// <summary>
        /// The action which was requested can not be executed. Might have changed state since received the Hypermedia.
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static ActionResult CanNotExecute(this ControllerBase controller)
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Can not execute Action",
                Detail = "",
                Type = "WebApi.HypermediaExtensions.Hypermedia.ActionNotAvailable",
                Status = (int)HttpStatusCode.BadRequest,
            };

            return controller.Problem(problemDetails);
        }

        public static ActionResult EntityAlreadyExists(
            this ControllerBase controller,
            IRouteResolverFactory routeResolverFactory,
            HypermediaObjectReferenceBase htoReferenceBase)
        {
            var routeResolver = routeResolverFactory.CreateRouteResolver(controller.HttpContext);
            var problemDetails = new ProblemDetails()
            {
                Title = "Entity already exists",
                Detail = "",
                Type = "WebApi.HypermediaExtensions.Hypermedia.EntityAlreadyExists",
                Status = (int)HttpStatusCode.BadRequest,
                Extensions =
                {
                    { "Location", routeResolver.ReferenceToRoute(htoReferenceBase).Url },
                },
            };
            return controller.Problem(problemDetails);
        }
    }
}
