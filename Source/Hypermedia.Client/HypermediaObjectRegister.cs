using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Client.Util;

namespace Bluehands.Hypermedia.Client
{
    public class HypermediaObjectRegister : IHypermediaObjectRegister
    {
        private readonly Dictionary<ICollection<string>, Type> hypermediaObjectTypeDictionary = new Dictionary<ICollection<string>, Type>(new StringCollectionComparer());

        public void Register(Type hypermediaObjectType)
        {
            if (!typeof(HypermediaClientObject).GetTypeInfo().IsAssignableFrom(hypermediaObjectType))
            {
                throw new Exception($"Can only register {nameof(HypermediaClientObject)}");
            }

            var attribute = hypermediaObjectType.GetTypeInfo().GetCustomAttribute<HypermediaClientObjectAttribute>();
            if (attribute == null)
            {
                this.hypermediaObjectTypeDictionary.Add(new List<string> { hypermediaObjectType.Name }, hypermediaObjectType);
            }
            else
            {
                this.hypermediaObjectTypeDictionary.Add(attribute.Classes.ToList(), hypermediaObjectType);
            }
            
        }

        public HypermediaClientObject CreateFromClasses(ICollection<string> classes)
        {
            var hypermediaObjectType = this.GetHypermediaType(classes);

            return (HypermediaClientObject)Activator.CreateInstance(hypermediaObjectType);
        }

        public Type GetHypermediaType(ICollection<string> classes)
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
        void Register(Type hypermediaObjectType);

        Type GetHypermediaType(ICollection<string> classes);

        HypermediaClientObject CreateFromClasses(ICollection<string> classes);
    }
}