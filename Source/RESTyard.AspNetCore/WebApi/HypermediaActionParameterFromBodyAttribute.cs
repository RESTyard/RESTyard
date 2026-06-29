using System;
using Microsoft.AspNetCore.Mvc;

namespace RESTyard.AspNetCore.WebApi
{
    /// <summary>
    /// Marks a hypermedia action parameter to be deserialized from the request body.
    /// Action parameter bodies now bind through the standard framework body path, so this
    /// attribute is a plain alias for <see cref="FromBodyAttribute"/>.
    /// </summary>
    [Obsolete("Use [FromBody] instead. Hypermedia action parameters bind from the body via the standard System.Text.Json input formatter.")]
    public class HypermediaActionParameterFromBodyAttribute : FromBodyAttribute
    {
    }
}
