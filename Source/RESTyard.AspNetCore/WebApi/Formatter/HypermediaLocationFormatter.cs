using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Primitives;
using RESTyard.AspNetCore.Exceptions;
using RESTyard.AspNetCore.Util.Extensions;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public abstract class HypermediaLocationFormatter<T> : HypermediaOutputFormatter
    {
        protected HypermediaLocationFormatter(
            IRouteResolverFactory routeResolverFactory)
            : base(routeResolverFactory)
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

            var routeResolver = CreateRouteResolver(context.HttpContext);

            var location = GetLocation(routeResolver, item);
            var response = context.HttpContext.Response;
            response.Headers["Location"] = location;
            
                
            SetResponseValues(response, item);

            await Task.CompletedTask;
        }

        protected abstract void SetResponseValues(HttpResponse response, T item);

        protected abstract string GetLocation(IHypermediaRouteResolver routeResolver, T item);
        
        protected abstract T? GetObject(object? locationObject);
    }
}