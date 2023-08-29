using System;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.AspNetCore.Hypermedia
{
    /// <summary>
    /// This class is intended for situations where a reference to a route
    /// must be provided which can not be resolved by the framework.
    /// It uses the IUrlHelper so a rout name and route parameters can be used.
    /// It can only be used
    /// in combination with <see cref="HypermediaObjectReference"/>
    /// Serializer will assume a GET HTTP method
    /// </summary>
    [HypermediaObject(Classes = new[] { "Internal" })]
    public class InternalReference : DirectReferenceBase<InternalReference>
    {
        /// <summary>
        /// Creates a internal link
        /// </summary>
        /// <param name="routeName">The route name</param>
        /// <param name="routeParameters">Route parameters</param>
        public InternalReference(string routeName, object? routeParameters = null) : base()
        {
            RouteName = routeName;
            RouteParameters = routeParameters;
        }

        [FormatterIgnoreHypermediaProperty]
        public string RouteName { get; }

        [FormatterIgnoreHypermediaProperty]
        public object? RouteParameters { get; }
    }
}