using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HypermediaClient.Hypermedia.Commands;

namespace HypermediaClient
{
    internal class RegisterHypermediaCommandFactory : IHypermediaCommandFactory
    {
        public static RegisterHypermediaCommandFactory Create()
        {
            var hypermediaCommandFactory = new RegisterHypermediaCommandFactory();
            hypermediaCommandFactory.Register(typeof(IHypermediaClientAction), typeof(HypermediaClientAction));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientAction<>), typeof(HypermediaClientAction<>));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFunction<>), typeof(HypermediaClientFunction<>));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFunction<,>), typeof(HypermediaClientFunction<,>));
            return hypermediaCommandFactory;
        }

        private Dictionary<Type, Type> InterfaceImplementationLookup { get; set; }

        public RegisterHypermediaCommandFactory()
        {
            InterfaceImplementationLookup = new Dictionary<Type, Type>();
        }

        public void Register(Type interfaceType, Type implementation)
        {
            if (InterfaceImplementationLookup.ContainsKey(interfaceType))
            {
                throw new Exception($"Interface already registered '{interfaceType.Name}'");
            }

            var isImplementation = interfaceType.IsAssignableFrom(implementation);
            var isGenericImplementation = implementation.GetInterfaces().Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

            if (!isImplementation && !isGenericImplementation)
            {
                throw new Exception($"Implementing type '{implementation}' does not imlement interface '{interfaceType.Name}'");
            }

            InterfaceImplementationLookup[interfaceType] = implementation;
        }

        public IHypermediaClientCommand Create(Type commandInterfaceType)
        {
            Type lookupType;
            IHypermediaClientCommand instance = null;

            var isGenericType = commandInterfaceType.GetTypeInfo().IsGenericType;
            if (isGenericType)
            {
                var genericTypeDefinition = commandInterfaceType.GetGenericTypeDefinition();
                var genericTypeArguments = commandInterfaceType.GetGenericArguments();

                lookupType = genericTypeDefinition;
                Type commandType;
                if (!InterfaceImplementationLookup.TryGetValue(lookupType, out commandType))
                {
                    throw new Exception($"Requested command interface type not found '{commandInterfaceType.Name}' ");
                }

                var constructedType = commandType.MakeGenericType(genericTypeArguments);
                instance = (IHypermediaClientCommand)Activator.CreateInstance(constructedType);

            }
            else
            {
                lookupType = commandInterfaceType;
                Type commandType;
                if (!InterfaceImplementationLookup.TryGetValue(lookupType, out commandType))
                {
                    throw new Exception($"Requested command interface type not found '{commandInterfaceType.Name}' ");
                }

                instance = (IHypermediaClientCommand)Activator.CreateInstance(commandType);
            }

            return instance;
        }
    }
}