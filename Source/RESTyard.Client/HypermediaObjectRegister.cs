using System;
using System.Collections.Generic;
using System.Reflection;
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

        public HypermediaClientObject CreateFromClasses(IDistinctOrderedCollection<string> classes)
        {
            var hypermediaObjectType = this.GetHypermediaType(classes);

            return (HypermediaClientObject)Activator.CreateInstance(hypermediaObjectType);
        }

        public Type GetHypermediaType(IDistinctOrderedCollection<string> classes)
        {
            Type hypermediaObjectType;
            if (!this.hypermediaObjectTypeDictionary.TryGetValue(classes, out hypermediaObjectType))
            {
                throw new Exception($"No HCO Type registered for classes: {string.Join(", ", classes)}");
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

        Type GetHypermediaType(IDistinctOrderedCollection<string> classes);

        HypermediaClientObject CreateFromClasses(IDistinctOrderedCollection<string> classes);
    }
}