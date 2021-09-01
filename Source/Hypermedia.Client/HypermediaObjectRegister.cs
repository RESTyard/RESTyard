using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Client.Util;

namespace Bluehands.Hypermedia.Client
{
    internal class HypermediaObjectRegister : IHypermediaObjectRegister
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
        void Register<THco>() where THco : HypermediaClientObject;
        
        void Register(Type hypermediaObjectType);

        Type GetHypermediaType(IDistinctOrderedCollection<string> classes);

        HypermediaClientObject CreateFromClasses(IDistinctOrderedCollection<string> classes);
    }
}