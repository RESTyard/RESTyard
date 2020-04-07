using System.Buffers;
using Bluehands.Hypermedia.MediaTypes;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    public class ProblemJsonFormatter : JsonOutputFormatter
    {
        public ProblemJsonFormatter() : base(new JsonSerializerSettings(), ArrayPool<char>.Shared)
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(DefaultMediaTypes.ProblemJson);
        }
    }
}