using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FunicularSwitch;
using RESTyard.Client.Hypermedia.Commands;

namespace RESTyard.Client
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
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFileUploadAction), typeof(HypermediaClientFileUploadAction));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFileUploadAction<>), typeof(HypermediaClientFileUploadAction<>));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFileUploadFunction<>), typeof(HypermediaClientFileUploadFunction<>));
            hypermediaCommandFactory.Register(typeof(IHypermediaClientFileUploadFunction<,>), typeof(HypermediaClientFileUploadFunction<,>));
            return hypermediaCommandFactory;
        }

        private Dictionary<Type, Type> InterfaceImplementationLookup { get; set; }

        public RegisterHypermediaCommandFactory()
        {
            this.InterfaceImplementationLookup = new Dictionary<Type, Type>();
        }

        public void Register(Type interfaceType, Type implementation)
        {
            if (this.InterfaceImplementationLookup.ContainsKey(interfaceType))
            {
                throw new Exception($"Interface already registered '{interfaceType.Name}'");
            }

            var isImplementation = interfaceType.GetTypeInfo().IsAssignableFrom(implementation);
            var isGenericImplementation = implementation.GetTypeInfo().GetInterfaces().Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == interfaceType);

            if (!isImplementation && !isGenericImplementation)
            {
                throw new Exception($"Implementing type '{implementation}' does not imlement interface '{interfaceType.Name}'");
            }

            this.InterfaceImplementationLookup[interfaceType] = implementation;
        }

        public Result<IHypermediaClientCommand> Create(Type commandInterfaceType)
        {
            Type typeToConstruct;

            var isGenericType = commandInterfaceType.GetTypeInfo().IsGenericType;
            if (isGenericType)
            {
                var genericTypeDefinition = commandInterfaceType.GetGenericTypeDefinition();
                var genericTypeArguments = commandInterfaceType.GetTypeInfo().GetGenericArguments();

                var lookupType = genericTypeDefinition;
                if (!this.InterfaceImplementationLookup.TryGetValue(lookupType, out var commandType))
                {
                    return Result.Error<IHypermediaClientCommand>($"Requested command interface type not found '{commandInterfaceType.Name}'");
                }

                typeToConstruct = commandType.MakeGenericType(genericTypeArguments);
            }
            else
            {
                var lookupType = commandInterfaceType;
                if (!this.InterfaceImplementationLookup.TryGetValue(lookupType, out var commandType))
                {
                    return Result.Error<IHypermediaClientCommand>($"Requested command interface type not found '{commandInterfaceType.Name}'");
                }

                typeToConstruct = commandType;
            }

            var instance = Activator.CreateInstance(typeToConstruct);
            return instance is IHypermediaClientCommand commandInstance
                ? Result.Ok(commandInstance)
                : Result.Error<IHypermediaClientCommand>($"Could not create command with type {typeToConstruct}");
        }
    }
}