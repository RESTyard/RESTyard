using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.ErrorHandling;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.Formatter;

namespace RESTyard.AspNetCore.WebApi.Controller
{
    public class ActionParameterSchemas
    {
        readonly ImmutableDictionary<string, object> schemaByTypeName;

        public ActionParameterSchemas(ApplicationModel applicationModel, HypermediaExtensionsOptions hypermediaOptions)
        {
            var actionParameterTypes = applicationModel.ActionParameterTypes.Values.Select(_ => _.Type);
            schemaByTypeName = actionParameterTypes.ToImmutableDictionary(
                t => t.BeautifulName(),
                JsonSchemaFactory.Generate,
                hypermediaOptions.CaseSensitiveParameterMatching ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase
            );
        }

        public bool TryGetValue(string parameterTypeName, [NotNullWhen(true)] out object? schema)
        {
            return schemaByTypeName.TryGetValue(parameterTypeName, out schema);
        }
    }

    [Route("ActionParameterTypes")]
    public class ActionParameterTypes : ControllerBase
    {
        readonly ActionParameterSchemas schemaByTypeName;

        public ActionParameterTypes(ActionParameterSchemas schemaByTypeName)
        {
            this.schemaByTypeName = schemaByTypeName;
        }


        [HttpGet("{parameterTypeName}", Name = RouteNames.ActionParameterTypes)]
        public ActionResult GetActionParameterTypeSchema(string parameterTypeName)
        {
            if (!schemaByTypeName.TryGetValue(parameterTypeName, out var schema))
            {
                return this.Problem(new ProblemDetails()
                {
                    Type = $"Unknown parameter type name: '{parameterTypeName}'",
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Unknown action parameter type"
                });
            }

            return Ok(schema);
        }
    }
}
