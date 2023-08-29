using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public class FormatterHelper
    {
        private static readonly IUrlHelperFactory urlHelperFactory = new UrlHelperFactory();

        public static IUrlHelper GetUrlHelperForCurrentContext(HttpContext httpContext)
        {
            var requestServices = httpContext.RequestServices;
            var actionContext = requestServices.GetRequiredService<IActionContextAccessor>().ActionContext!;
            
            return urlHelperFactory.GetUrlHelper(actionContext);
        }
    }
}