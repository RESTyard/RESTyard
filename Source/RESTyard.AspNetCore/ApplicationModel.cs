using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Util;
using RESTyard.AspNetCore.Util.Extensions;

namespace RESTyard.AspNetCore
{
    public class ApplicationModel
    {
        public ImmutableDictionary<Type, ActionParameterType> ActionParameterTypes { get; }

        public static ApplicationModel Create(Assembly[] assemblies)
        {
            var implementingAssemblies = (assemblies.Length > 0
                ? assemblies
                : Assembly.GetEntryAssembly().Yield()).ToImmutableArray();

            var actionParameterTypes = implementingAssemblies
                .SelectMany(a => a?.GetTypes()
                    .Where(t => typeof(IHypermediaActionParameter).GetTypeInfo().IsAssignableFrom(t))
                    .Select(t => new ActionParameterType(t))
                        ?? []
                ).ToImmutableDictionary(_ => _.Type);

            return new ApplicationModel(actionParameterTypes);
        }

        public ApplicationModel(ImmutableDictionary<Type, ActionParameterType> actionParameterTypes)
        {
            ActionParameterTypes = actionParameterTypes;
        }

        public class ActionParameterType
        {
            public Type Type { get; }

            public ActionParameterType(Type type)
            {
                Type = type;
            }

            public override string ToString()
            {
                return $"{Type.BeautifulName()}";
            }
        }
    }
}
