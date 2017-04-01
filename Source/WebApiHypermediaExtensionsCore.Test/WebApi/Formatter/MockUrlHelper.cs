using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace WebApiHypermediaExtensionsCore.Test.WebApi.Formatter
{
    public class MockUrlHelper : IUrlHelper
    {
        public string Action(UrlActionContext actionContext)
        {
            throw new System.NotImplementedException();
        }

        public string Content(string contentPath)
        {
            throw new System.NotImplementedException();
        }

        public bool IsLocalUrl(string url)
        {
            throw new System.NotImplementedException();
        }

        public string RouteUrl(UrlRouteContext routeContext)
        {
            var result = $"{routeContext.Protocol}/{routeContext.Host}/{routeContext.RouteName}";
            if (routeContext.Values?.GetType().GetProperties().Length != 0)
            {
                result += $"/{routeContext.Values}";
            }
            return result;
        }

        public string Link(string routeName, object values)
        {
            throw new System.NotImplementedException();
        }

        public ActionContext ActionContext { get; }
    }
}