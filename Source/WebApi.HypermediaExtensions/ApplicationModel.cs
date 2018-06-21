using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.Hypermedia;
using WebApi.HypermediaExtensions.Hypermedia.Actions;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.Util;
using WebApi.HypermediaExtensions.Util.Extensions;
using WebApi.HypermediaExtensions.WebApi.AttributedRoutes;

namespace WebApi.HypermediaExtensions
{
    public class ApplicationModel
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
                    .Select(t =>
                    {
                        var hmoAttribute = t.GetTypeInfo().GetCustomAttribute<HypermediaObjectAttribute>();
                        return new HmoType(t, FindGetMethods(controllerTypes, t), hmoAttribute?.Classes, hmoAttribute?.Title, GetHmoProperties(t));
                    })
                ).ToImmutableDictionary(_ => _.Type);

            var actionParameterTypes = implementingAssemblies
                .SelectMany(a => a.GetTypes()
                    .Where(t => typeof(IHypermediaActionParameter).GetTypeInfo().IsAssignableFrom(t))
                    .Select(t => new ActionParameterType(t, FindGetParameterInfoMethodOrNull(controllerTypes, t)))
                ).ToImmutableDictionary(_ => _.Type);

            return new ApplicationModel(hmoTypes, actionParameterTypes, controllerTypes);
        }

        static IEnumerable<HmoProperty> GetHmoProperties(Type t)
        {
            return t.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(ToHmoProperty);
        }

        static HmoProperty ToHmoProperty(PropertyInfo property)
        {
            bool isIgnored = PropertyHasIgnoreAttribute(property);
            if (isIgnored)
                return null;
            bool isAction = IsHypermediaAction(property);
            if (isAction)
                return null;

            var propertyType = property.PropertyType;
            var propertyTypeInfo = propertyType.GetTypeInfo();
            if (propertyTypeInfo.IsClass && propertyType != typeof(string))
                return null;

            var name = GetPropertyName(property);

            return new HmoProperty(name, property);
        }

        static bool PropertyHasIgnoreAttribute(PropertyInfo publicProperty)
        {
            return publicProperty.CustomAttributes.Any(a => a.AttributeType == typeof(FormatterIgnoreHypermediaPropertyAttribute));
        }

        static bool IsHypermediaAction(PropertyInfo property)
        {
            return typeof(HypermediaActionBase).GetTypeInfo().IsAssignableFrom(property.PropertyType);
        }

        private static string GetPropertyName(PropertyInfo publicProperty)
        {
            string propertyName;
            var hypermediaPropertyAttribute = GetHypermediaPropertyAttribute(publicProperty);
            if (!string.IsNullOrEmpty(hypermediaPropertyAttribute?.Name))
            {
                propertyName = hypermediaPropertyAttribute.Name;
            }
            else
            {
                propertyName = publicProperty.Name;
            }
            return propertyName;
        }

        private static HypermediaPropertyAttribute GetHypermediaPropertyAttribute(PropertyInfo hypermediaPropertyInfo)
        {
            return hypermediaPropertyInfo.GetCustomAttribute<HypermediaPropertyAttribute>();
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

            /// <summary>
            /// Should be single method for concrete hypermedia object. For a hmo base type should be all get methods of types derived from that type.
            /// </summary>
            public ImmutableArray<GetHmoMethod> GetHmoMethods { get; }

            public ImmutableArray<string> Classes { get; }
            public string Title { get; }
            public ImmutableArray<HmoProperty> Properties { get; }

            public HmoType(Type type, IEnumerable<GetHmoMethod> getHmoMethods, IEnumerable<string> classes, string title, IEnumerable<HmoProperty> properties)
            {
                Type = type;
                GetHmoMethods = getHmoMethods.ToImmutableArraySafe();
                Classes = classes.ToImmutableArraySafe();
                Title = title;
                Properties = properties.ToImmutableArraySafe();
            }

            public override string ToString()
            {
                return $"{Type.BeautifulName()}, Get routes: {string.Join(",", GetHmoMethods.Select(g => g.RouteTemplateFull))}";
            }
        }

        public class HmoProperty
        {
            public string Name { get; }
            public PropertyInfo PropertyInfo { get; }

            public HmoProperty(string name, PropertyInfo propertyInfo)
            {
                Name = name;
                PropertyInfo = propertyInfo;
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
