using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.ErrorHandling;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;
using WebApi.HypermediaExtensions.WebApi.Formatter;

namespace WebApi.HypermediaExtensions.WebApi.Controller
{
    public class ActionParameterSchemas
    {
        readonly ImmutableDictionary<string, object> schemaByTypeName;

        public ActionParameterSchemas(IEnumerable<Type> actionParameterTypes, bool useCaseSensitiveParameterMatching)
        {
            schemaByTypeName = actionParameterTypes.ToImmutableDictionary(
                t => t.BeautifulName(),
                t => JsonSchemaFactory.Generate(t).GetAwaiter().GetResult(),
                useCaseSensitiveParameterMatching ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase
            );
        }

        public bool TryGetValue(string parameterTypeName, out object schema)
        {
            return schemaByTypeName.TryGetValue(parameterTypeName, out schema);
        }
    }

    [Route("ActionParameterTypes")]
    public class ActionParameterTypes : Microsoft.AspNetCore.Mvc.Controller
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
                return this.Problem(new ProblemJson
                {
                    ProblemType = $"Unknwon parameter type name: '{parameterTypeName}'",
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Title = "Unknown action parameter type"
                });
            }

            return Ok(schema);
        }
    }
}
