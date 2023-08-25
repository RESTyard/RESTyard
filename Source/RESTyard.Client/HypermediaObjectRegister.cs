using System;
using System.Collections.Generic;
using System.Reflection;
using FunicularSwitch;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Util;

namespace RESTyard.Client
{
    public class HypermediaObjectRegister : IHypermediaObjectRegister
    {
        private readonly IDictionary<IDistinctOrderedCollection<string>, Type> hypermediaObjectTypeDictionary = new Dictionary<IDistinctOrderedCollection<string>, Type>(new DistinctOrderedStringCollectionComparer());

        public void Register<THco>() where THco : HypermediaClientObject
        {
            Register(typeof(THco));
        }

        public void Register(Type hypermediaObjectType)
        {
            if (!typeof(HypermediaClientObject).GetTypeInfo().IsAssignableFrom(hypermediaObjectType))
            {
                throw new Exception($"Can only register {nameof(HypermediaClientObject)}");
            }
            var attribute = hypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaClientObjectAttribute>();
            if (attribute == null)
            {
                this.hypermediaObjectTypeDictionary.Add(new DistinctOrderedStringCollection(hypermediaObjectType.Name), hypermediaObjectType);
            }
            else
            {
                this.hypermediaObjectTypeDictionary.Add(attribute.Classes, hypermediaObjectType);
            }
            
        }

        public Result<HypermediaClientObject> CreateFromClasses(IDistinctOrderedCollection<string> classes)
        {
            var typeResult = this.GetHypermediaType(classes);

            return typeResult.Bind(hypermediaObjectType =>
            {
                return Result.Try(() => Activator.CreateInstance(hypermediaObjectType), exc => exc.Message)
                    .Bind(obj => obj is HypermediaClientObject hco
                        ? Result.Ok(hco)
                        : Result.Error<HypermediaClientObject>(
                            $"Could not create instance of '{hypermediaObjectType.Name}'"));
            });
        }

        public Result<Type> GetHypermediaType(IDistinctOrderedCollection<string> classes)
        {
            if (!this.hypermediaObjectTypeDictionary.TryGetValue(classes, out var hypermediaObjectType))
            {
                return Result.Error<Type>($"No HCO Type registered for classes: {string.Join(", ", classes)}");
            }
            return hypermediaObjectType;
        }
    }

    public interface IHypermediaObjectRegister
    {
        /// <summary>
        /// Register a type. When parsing incoming data, the provided "classes" values are compared to the classes given in the type's HypermediaClientObjectAttribute, or to the types name
        /// </summary>
        /// <typeparam name="THco"></typeparam>
        void Register<THco>() where THco : HypermediaClientObject;
        
        /// <summary>
        /// Register a type. When parsing incoming data, the provided "classes" values are compared to the classes given in the type's HypermediaClientObjectAttribute, or to the types name
        /// </summary>
        /// <param name="hypermediaObjectType"></param>
        void Register(Type hypermediaObjectType);

        Result<Type> GetHypermediaType(IDistinctOrderedCollection<string> classes);

        Result<HypermediaClientObject> CreateFromClasses(IDistinctOrderedCollection<string> classes);
    }
}