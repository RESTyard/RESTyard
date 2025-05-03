using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public class SirenHypermediaFormatter : HypermediaOutputFormatter
    {
        private readonly ISirenHypermediaConverterFactory sirenHypermediaConverterFactory;

        public SirenHypermediaFormatter(
            IRouteResolverFactory routeResolverFactory,
            ISirenHypermediaConverterFactory sirenHypermediaConverterFactory)
            : base(routeResolverFactory)
        {
            this.sirenHypermediaConverterFactory = sirenHypermediaConverterFactory;
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context.Object is null || !AttributedRouteHelper.Has<HypermediaObjectAttribute>(context.Object.GetType()))
            {
                return false;
            }
            
            var contentType = context.ContentType.ToString();
            if (string.IsNullOrEmpty(contentType))
            {
                return true;
            }

            if (contentType.Contains(DefaultMediaTypes.Siren))
            {
                return true;
            }

            return false;
        }
        
        public override async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context.Object is not IHypermediaObject hypermediaObject || !AttributedRouteHelper.Has<HypermediaObjectAttribute>(context.Object.GetType()))
            {
                throw new HypermediaFormatterException("Formatter expected a non-null object, but is null.");
            }

            // context influences how routes are resolved
            var routeResolver = CreateRouteResolver(context.HttpContext);
            var converter = sirenHypermediaConverterFactory.CreateSirenConverter(routeResolver);
            var sirenJson = converter.ConvertToString(hypermediaObject);

            var response = context.HttpContext.Response;
            response.ContentType = DefaultMediaTypes.Siren;
            await WriteToBody(context, response, sirenJson);
        }

        private static async Task WriteToBody(OutputFormatterWriteContext context, HttpResponse response, string content)
        {
            using (var writer = context.WriterFactory(response.Body, Encoding.UTF8))
            {
                await writer.WriteAsync(content);
                await writer.FlushAsync();
            }
        }
    }
}