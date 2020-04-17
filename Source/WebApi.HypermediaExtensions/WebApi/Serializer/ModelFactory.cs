using System;
using System.Linq;
using FunicularSwitch;
using FunicularSwitch.Extensions;
using WebApi.HypermediaExtensions.Hypermedia.Attributes;
using WebApi.HypermediaExtensions.WebApi.Serializer.Model;
using WebApi.HypermediaExtensions.WebApi.Serializer.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Serializer
{
    public class ModelFactory
    {
        public ModelFactory()
        {

        }

        public Base Create(object hypermediaObject)
        {
            var reflectionResult = new ObjectReflectionService().Reflect(hypermediaObject.GetType());
            //CreateBase(reflectionResult).Bind(modelBase =>
            //{
            //    // todo add properties, links, entities, etc and validate type eg.valid link property under attribute
                
            //});
            throw new NotImplementedException();
        }

        private Result<Base> CreateBase(Result<ObjectReflection> reflectionResult)
        {
            // todo how to handle missing classes; what about derived classes
            return reflectionResult.Bind(reflection =>
            {
                var hypermediaObjectAttribute = reflection.HypermediaObjectAttribute;
                if (!hypermediaObjectAttribute.Classes.Any())
                    return Result.Error<Base>(
                        $"Missing classes in '{typeof(HypermediaObjectAttribute).BeautifulName()}' attribute in class '{reflection.HypermediaObjectType.BeautifulName()}'");
                return Result.Ok(new Base(hypermediaObjectAttribute.Title, hypermediaObjectAttribute.Classes.Select(c => new Class(c)).ToList()));
            });
        }

        //private Result<Base> AddProperties(Result<ObjectReflection> reflectionResult)
        //{

        //}
    }
}