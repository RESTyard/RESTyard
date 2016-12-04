using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public class FormatterHelper
    {
        private static readonly IUrlHelperFactory urlHelperFactory = new UrlHelperFactory();

        public static IUrlHelper GetUrlHelperForCurrentContext(OutputFormatterWriteContext context)
        {
            var requestServices = context.HttpContext.RequestServices;
            var actionContext = requestServices.GetRequiredService<IActionContextAccessor>().ActionContext;
            
            return urlHelperFactory.GetUrlHelper(actionContext);
        }
    }
}