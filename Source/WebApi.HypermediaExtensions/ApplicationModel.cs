using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.Util.Extensions;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;

namespace WebApi.HypermediaExtensions
{
    class ApplicationModel
    {
        public ImmutableDictionary<Type, HmoType> HmoTypes { get; }
        public ImmutableDictionary<Type, ActionParameterType> ActionParameterTypes { get; }
        public ImmutableArray<ControllerType> ControllerTypes { get; }

        public static ApplicationModel Create(params Assembly[] assemblies)
        {
            var implementingAssemblies = (assemblies.Length > 0
                ? assemblies
                : Assembly.GetEntryAssembly().Yield()).ToImmutableArray();

            var controllerTypes = implementingAssemblies
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(Controller).GetTypeInfo().IsAssignableFrom(t))
                    .Select(t =>
                    {
                        return new ControllerType(t,
                            controllerType => t.GetTypeInfo().GetMethods().Select(m => GetControllerMethodOrNull(m, controllerType)).Where(_ => _ != null)
                            );
                    }))
                .ToImmutableArray();

            var hmoTypes = implementingAssemblies
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(HypermediaObject).GetTypeInfo().IsAssignableFrom(t))
                    .Select(t => new HmoType(t, FindGetMethodOrNull(controllerTypes, t)))
                ).ToImmutableDictionary(_ => _.Type);

            var actionParameterTypes = implementingAssemblies
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(IHypermediaActionParameter).GetTypeInfo().IsAssignableFrom(t))
                    .Select(t => new ActionParameterType(t, FindGetParameterInfoMethodOrNull(controllerTypes, t)))
                ).ToImmutableDictionary(_ => _.Type);

            return new ApplicationModel(hmoTypes, actionParameterTypes, controllerTypes);
        }

        static GetActionParameterInfoMethod FindGetParameterInfoMethodOrNull(ImmutableArray<ControllerType> controllerTypes, Type type)
        {
            return controllerTypes.SelectMany(t => t.Methods).OfType<GetActionParameterInfoMethod>()
                .FirstOrDefault(m => m.ActionParameterType == type);
        }

        static GetHmoMethod FindGetMethodOrNull(ImmutableArray<ControllerType> controllerTypes, Type type)
        {
            return controllerTypes.SelectMany(t => t.Methods).OfType<GetHmoMethod>()
                .FirstOrDefault(m => m.HmoType == type);
        }


        static ControllerMethod GetControllerMethodOrNull(MethodInfo methodInfo, ControllerType controllerType)
        {
            var httpGetHypermediaObject = methodInfo.GetCustomAttribute<HttpGetHypermediaObject>();
            if (httpGetHypermediaObject != null)
            {
                return new GetHmoMethod(httpGetHypermediaObject.RouteType, httpGetHypermediaObject.Template, controllerType);
            }

            var httpPostHypermediaAction = methodInfo.GetCustomAttribute<HttpPostHypermediaAction>();
            if (httpPostHypermediaAction != null)
            {
                return new ActionMethod(httpPostHypermediaAction.RouteType, httpPostHypermediaAction.Template, controllerType);
            }
            var httpGetHypermediaActionParameterInfo = methodInfo.GetCustomAttribute<HttpGetHypermediaActionParameterInfo>();
            if (httpGetHypermediaActionParameterInfo != null)
            {
                return new GetActionParameterInfoMethod(httpGetHypermediaActionParameterInfo.RouteType, httpGetHypermediaActionParameterInfo.Template, controllerType);
            }

            return null;
        }

        public ApplicationModel(ImmutableDictionary<Type, HmoType> hmoTypes, ImmutableDictionary<Type, ActionParameterType> actionParameterTypes, ImmutableArray<ControllerType> controllerTypes)
        {
            HmoTypes = hmoTypes;
            ActionParameterTypes = actionParameterTypes;
            ControllerTypes = controllerTypes;
        }

        public class HmoType
        {
            public Type Type { get; }

            public GetHmoMethod GetHmoMethod { get; }

            public HmoType(Type type, GetHmoMethod getHmoMethod)
            {
                Type = type;
                GetHmoMethod = getHmoMethod;
            }

            public override string ToString()
            {
                return $"{Type.BeautifulName()}, Get route: {GetHmoMethod?.RouteTemplateFull}";
            }
        }

        public class ActionParameterType
        {
            public Type Type { get; }
            public GetActionParameterInfoMethod GetActionParameterInfoMethod { get; }

            public ActionParameterType(Type type, GetActionParameterInfoMethod getActionParameterInfoMethod)
            {
                Type = type;
                GetActionParameterInfoMethod = getActionParameterInfoMethod;
            }

            public override string ToString()
            {
                return $"{Type.BeautifulName()}";
            }
        }

        public class ControllerType
        {
            public Type Type { get; }
            public string RouteTemplate { get; }
            public ImmutableList<ControllerMethod> Methods { get; }

            public ControllerType(Type type, Func<ControllerType, IEnumerable<ControllerMethod>> methods)
            {
                RouteTemplate = type.GetTypeInfo().GetCustomAttribute<RouteAttribute>()?.Template;
                Type = type;
                Methods = methods(this).ToImmutableList();
            }

            public override string ToString()
            {
                return $"{Type.BeautifulName()}";
            }
        }

        public abstract class ControllerMethod
        {
            public ControllerType Parent { get; }
            public string RouteTemplate { get; }
            public string RouteTemplateFull { get; }

            protected ControllerMethod(string routeTemplate, ControllerType parent)
            {
                RouteTemplate = routeTemplate;
                Parent = parent;
                RouteTemplateFull = string.Concat(parent.RouteTemplate, "/", routeTemplate).Replace("//", "/").Replace("///", "/");
            }

        }

        public class GetHmoMethod : ControllerMethod
        {
            public Type HmoType { get; }

            public GetHmoMethod(Type hmoType, string routeTemplate, ControllerType parent) : base(routeTemplate, parent)
            {
                HmoType = hmoType;
            }
        }

        public class ActionMethod : ControllerMethod
        {
            public Type ActionType { get; }

            public ActionMethod(Type actionType, string routeTemplate, ControllerType parent) : base(routeTemplate, parent)
            {
                ActionType = actionType;
            }
        }

        public class GetActionParameterInfoMethod : ControllerMethod
        {
            public Type ActionParameterType { get; }

            public GetActionParameterInfoMethod(Type actionParameterType, string routeTemplate, ControllerType parent) : base(routeTemplate, parent)
            {
                ActionParameterType = actionParameterType;
            }
        }
    }
}
