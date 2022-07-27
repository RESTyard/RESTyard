using System;
using System.Collections.Immutable;
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
                t => JsonSchemaFactory.Generate(t).GetAwaiter().GetResult(),
                hypermediaOptions.CaseSensitiveParameterMatching ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase
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
