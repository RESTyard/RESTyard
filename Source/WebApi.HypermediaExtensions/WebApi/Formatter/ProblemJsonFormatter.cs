namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    using Bluehands.Hypermedia.MediaTypes;
    using Microsoft.AspNetCore.Mvc.Formatters;

#if NETSTANDARD1_6
    using System.Buffers;
    using Newtonsoft.Json;
    
    public class ProblemJsonFormatter : JsonOutputFormatter
    {
        public ProblemJsonFormatter() : base(new JsonSerializerSettings(), ArrayPool<char>.Shared)
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(DefaultMediaTypes.ProblemJson);
        }
    }

#elif NETCOREAPP3_0

    using System.Text.Json;
    
    public class ProblemJsonFormatter : SystemTextJsonOutputFormatter
    {
        public ProblemJsonFormatter() : base(new JsonSerializerOptions())
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(DefaultMediaTypes.ProblemJson);
        }
    }
#endif

}