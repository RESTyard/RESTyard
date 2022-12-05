using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.Util.Extensions;
using RESTyard.AspNetCore.WebApi.AttributedRoutes;

namespace RESTyard.AspNetCore
{
    public class ApplicationModel
    {
        public ImmutableDictionary<Type, HmoType> HmoTypes { get; }
        public ImmutableDictionary<Type, ActionParameterType> ActionParameterTypes { get; }
        public ImmutableArray<ControllerType> ControllerTypes { get; }

        public static ApplicationModel Create(Assembly[] assemblies)
        {
            var implementingAssemblies = (assemblies.Length > 0
                ? assemblies
                : Assembly.GetEntryAssembly().Yield()).ToImmutableArray();

            var controllerTypes = implementingAssemblies
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(ControllerBase).GetTypeInfo().IsAssignableFrom(t))
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
                    .Select(t => new HmoType(t, FindGetMethods(controllerTypes, t)))
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

        static IEnumerable<GetHmoMethod> FindGetMethods(ImmutableArray<ControllerType> controllerTypes, Type type)
        {
            return controllerTypes
                .SelectMany(t => t.Methods)
                .OfType<GetHmoMethod>()
                .Where(m => type.GetTypeInfo().IsAssignableFrom(m.HmoType))
                .OrderBy(m => m.HmoType == type ? 0 : 1);
        }

        static ControllerMethod GetControllerMethodOrNull(MethodInfo methodInfo, ControllerType controllerType)
        {
            var httpGetHypermediaObject = methodInfo.GetCustomAttribute<HttpGetHypermediaObject>();
            if (httpGetHypermediaObject != null)
            {
                return new GetHmoMethod(httpGetHypermediaObject.RouteType, httpGetHypermediaObject.Template, controllerType, methodInfo.Name);
            }

            var httpPostHypermediaAction = methodInfo.GetCustomAttribute<HttpPostHypermediaAction>();
            if (httpPostHypermediaAction != null)
            {
                return new ActionMethod(httpPostHypermediaAction.RouteType, httpPostHypermediaAction.Template, controllerType, methodInfo.Name);
            }
            var httpDeleteHypermediaAction = methodInfo.GetCustomAttribute<HttpDeleteHypermediaAction>();
            if (httpDeleteHypermediaAction != null)
            {
                return new ActionMethod(httpDeleteHypermediaAction.RouteType, httpDeleteHypermediaAction.Template, controllerType, methodInfo.Name);
            }
            var httpPatchHypermediaAction = methodInfo.GetCustomAttribute<HttpPatchHypermediaAction>();
            if (httpPatchHypermediaAction != null)
            {
                return new ActionMethod(httpPatchHypermediaAction.RouteType, httpPatchHypermediaAction.Template, controllerType, methodInfo.Name);
            }


            var httpGetHypermediaActionParameterInfo = methodInfo.GetCustomAttribute<HttpGetHypermediaActionParameterInfo>();
            if (httpGetHypermediaActionParameterInfo != null)
            {
                return new GetActionParameterInfoMethod(httpGetHypermediaActionParameterInfo.RouteType, httpGetHypermediaActionParameterInfo.Template, controllerType, methodInfo.Name);
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

            /// <summary>
            /// Should be single method for concrete hypermedia object. For a hmo base type should be all get methods of types derived from that type.
            /// </summary>
            public ImmutableArray<GetHmoMethod> GetHmoMethods { get; }

            public HmoType(Type type, IEnumerable<GetHmoMethod> getHmoMethods)
            {
                Type = type;
                GetHmoMethods = getHmoMethods.ToImmutableArray();
            }

            public override string ToString()
            {
                return $"{Type.BeautifulName()}, Get routes: {string.Join(",", GetHmoMethods.Select(g => g.RouteTemplateFull))}";
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
                var routeTemplate = type.GetTypeInfo().GetCustomAttribute<RouteAttribute>()?.Template;
                RouteTemplate = FillControllerTokenInRouteTemplate(routeTemplate, type);
                Type = type;
                Methods = methods(this).ToImmutableList();
            }

            private static string FillControllerTokenInRouteTemplate(string routeTemplate, Type controllerType)
            {
                if (string.IsNullOrWhiteSpace(routeTemplate))
                {
                    return string.Empty;
                }

                if (!routeTemplate.Contains("[controller]") && !routeTemplate.Contains("[Controller]"))
                {
                    return routeTemplate;
                }

                var controllerNameReplacement = RemoveControllerFromName(controllerType.Name);

                return routeTemplate.Replace("[controller]", controllerNameReplacement).Replace("[Controller]", controllerNameReplacement);
            }

            private static string RemoveControllerFromName(string controllerTypeName)
            {
                if (controllerTypeName.EndsWith("controller", StringComparison.OrdinalIgnoreCase))
                {
                    return controllerTypeName.Substring(0, controllerTypeName.LastIndexOf("controller", StringComparison.OrdinalIgnoreCase));
                }
                return controllerTypeName;
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

            protected ControllerMethod(string routeTemplate, ControllerType parent, string methodName)
            {
                RouteTemplate = ReplaceActionTokenInRouteTemplate(routeTemplate, methodName); ;
                Parent = parent;
                RouteTemplateFull = string.Concat(parent.RouteTemplate, "/", RouteTemplate).Replace("//", "/").Replace("///", "/");
            }

            private string ReplaceActionTokenInRouteTemplate(string routeTemplate, string methodName)
            {
                if (routeTemplate == null 
                    || (!routeTemplate.Contains("[action]") && !routeTemplate.Contains("[Action]")))
                {
                    return routeTemplate;
                }

                return routeTemplate.Replace("[action]", methodName).Replace("[Action]", methodName);
            }
        }

        public class GetHmoMethod : ControllerMethod
        {
            public Type HmoType { get; }

            public GetHmoMethod(Type hmoType, string routeTemplate, ControllerType parent, string methodName) : base(routeTemplate, parent, methodName)
            {
                HmoType = hmoType;
            }
        }

        public class ActionMethod : ControllerMethod
        {
            public Type ActionType { get; }

            public ActionMethod(Type actionType, string routeTemplate, ControllerType parent, string methodName) : base(routeTemplate, parent, methodName)
            {
                ActionType = actionType;
            }
        }

        public class GetActionParameterInfoMethod : ControllerMethod
        {
            public Type ActionParameterType { get; }

            public GetActionParameterInfoMethod(Type actionParameterType, string routeTemplate, ControllerType parent, string methodName) : base(routeTemplate, parent, methodName)
            {
                ActionParameterType = actionParameterType;
            }
        }
    }
}
