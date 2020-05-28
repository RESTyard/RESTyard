namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    using Bluehands.Hypermedia.MediaTypes;
    
#if NETSTANDARD1_6
    using System.Buffers;
    using Newtonsoft.Json;
    using Microsoft.AspNetCore.Mvc.Formatters;
    
    public class ProblemJsonFormatter : JsonOutputFormatter
    {
        public ProblemJsonFormatter() : base(new JsonSerializerSettings(), ArrayPool<char>.Shared)
        {
            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(DefaultMediaTypes.ProblemJson);
        }
    }

#else

    using System.Text.Json;
    using Microsoft.AspNetCore.Mvc.Formatters;
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