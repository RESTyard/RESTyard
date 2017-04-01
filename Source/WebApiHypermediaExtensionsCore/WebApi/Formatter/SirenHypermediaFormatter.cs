using System.Text;
using System.Threading.Tasks;
using Hypermedia.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.Hypermedia;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public class SirenHypermediaFormatter : HypermediaOutputFormatter
    {
        private readonly ISirenHypermediaConverterFactory sirenHypermediaConverterFactory;

        public SirenHypermediaFormatter(
            IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory,
            ISirenHypermediaConverterFactory sirenHypermediaConverterFactory,
            IHypermediaUrlConfig defaultHypermediaUrlConfig)
            : base(routeResolverFactory, routeKeyFactory, defaultHypermediaUrlConfig)
        {
            this.sirenHypermediaConverterFactory = sirenHypermediaConverterFactory;
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (!(context.Object is HypermediaObject))
            {
                return false;
            }

            var contentType = context.ContentType.ToString();
            if (string.IsNullOrEmpty(contentType))
            {
                return true;
            }

            if (contentType.Contains(DefaultContentTypes.Siren))
            {
                return true;
            }

            return false;
        }
        
        public override async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var hypermediaObject = context.Object as HypermediaObject;
            if (hypermediaObject == null)
            {
                throw new HypermediaFormatterException("Formatter expected a HypermediaObject but is not.");
            }

            // context influences how routes are resolved
            var routeResolver = CreateRouteResolver(context);
            var converter = sirenHypermediaConverterFactory.CreateSirenConverter(routeResolver);
            var sirenJson = converter.ConvertToString(hypermediaObject);

            var response = context.HttpContext.Response;
            response.ContentType = DefaultContentTypes.Siren;
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