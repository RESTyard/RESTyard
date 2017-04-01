using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using WebApiHypermediaExtensionsCore.Exceptions;
using WebApiHypermediaExtensionsCore.WebApi.RouteResolver;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public abstract class HypermediaLocationFormatter<T> : HypermediaOutputFormatter
    {
        protected HypermediaLocationFormatter(
            IRouteResolverFactory routeResolverFactory,
            IRouteKeyFactory routeKeyFactory,
            IHypermediaUrlConfig defaultHypermediaUrlConfig)
            : base(routeResolverFactory, routeKeyFactory, defaultHypermediaUrlConfig)
        {
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context) 
        {
            if (context.Object is T)
            {
                return true;
            }

            return false;
        }

        public override async Task WriteAsync(OutputFormatterWriteContext context)
        {
            var item = GetObject(context.Object);
            if (item == null)
            {
                throw new HypermediaFormatterException($"Formatter expected a {typeof(T).Name}  but is not.");
            }

            var routeResolver = CreateRouteResolver(context);

            var location = GetLocation(routeResolver, item);
            var response = context.HttpContext.Response;
            response.Headers["Location"] = location;

            SetResponseValues(response, item);

            await Task.FromResult(0);
        }

        protected abstract void SetResponseValues(HttpResponse response, T item);

        protected abstract StringValues GetLocation(IHypermediaRouteResolver routeResolver, T item);
        
        protected abstract T GetObject(object locationObject);
    }
}