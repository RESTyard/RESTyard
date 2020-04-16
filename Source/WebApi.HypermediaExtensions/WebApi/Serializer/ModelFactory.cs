using WebApi.HypermediaExtensions.WebApi.Serializer.Model;
using WebApi.HypermediaExtensions.WebApi.Serializer.Reflection;

namespace WebApi.HypermediaExtensions.WebApi.Serializer
{
    public class ModelFactory
    {
        public ModelFactory()
        {

        }

        public ModelBase Create(object hypermediaObject)
        {
            var reflectionResult = new ObjectReflectionService().Reflect(hypermediaObject.GetType());
            return new ModelBase();
            //return reflectionResult.Bind(reflection =>
            //{
            //    return new Model.ModelBase()
            //    {
            //        SirenTitle = this.CreateSirenTitle(reflection.HypermediaObjectAttribute),
            //        SirenClasses = this.CreateSirenClasses(reflection.HypermediaObjectAttribute,
            //            reflection.HypermediaObjectType),
            //        SirenProperties = this.CreateSirenProperties(reflection.Properties),
            //        SirenLinks = this.CreateSirenLinks(reflection.Links),
            //        SirenActions = this.CreateSirenActions(reflection.Actions)
            //    };
            //});
        }

        //    private ModelTitle CreateSirenTitle(HypermediaObjectAttribute hypermediaObjectAttribute)
        //    {
        //        return new ModelTitle(hypermediaObjectAttribute?.Title);
        //    }

        //    private List<ModelClass> CreateSirenClasses(HypermediaObjectAttribute hypermediaObjectAttribute, Type hypermediaObjectType)
        //    {
        //        var sirenClasses = new List<ModelClass>();
        //        if (hypermediaObjectAttribute != null && hypermediaObjectAttribute.Classes.Any())
        //        {
        //            sirenClasses.AddRange(hypermediaObjectAttribute.Classes.Select(c => new ModelClass(c)));
        //        }
        //        else
        //        {
        //            sirenClasses.Add(new ModelClass(hypermediaObjectType.BeautifulName()));
        //        }
        //        return sirenClasses;
        //    }

        //    private List<ModelProperty> CreateSirenProperties(IEnumerable<ReflectedProperty> hypermediaProperty)
        //    {
        //        return hypermediaProperty.Select(hp => 
        //            !hp.LeadingHypermediaAttribute.HasValue 
        //                ? new ModelProperty(hp.PropertyInfo) 
        //                : new ModelProperty(hp.PropertyInfo, (hp.LeadingHypermediaAttribute.Value as Property)?.Name))
        //            .ToList();
        //    }

        //    private List<ModelLink> CreateSirenLinks(IEnumerable<ReflectedProperty> hypermediaProperty)
        //    {
        //        return hypermediaProperty.Select(hp => 
        //            new ModelLink(
        //                (hp.LeadingHypermediaAttribute.Value as Link)?.Relations.Select(r => new ModelRelation(r)).ToList(),
        //                hp.PropertyInfo, 
        //                hp.PropertyInfo.GetType().IsAssignableFrom(typeof(HypermediaObjectReferenceBase)),
        //                null)) // attribute needs title too
        //            .ToList();
        //    }

        //    private List<ModelAction> CreateSirenActions(IEnumerable<ReflectedProperty> hypermediaProperty)
        //    {
        //        return hypermediaProperty.Select(hp =>
        //        {
        //            if (!hp.LeadingHypermediaAttribute.HasValue)
        //            {
        //                return new ModelAction(hp.PropertyInfo);
        //            }
        //            var actionAttribute = hp.LeadingHypermediaAttribute.Value as Action;
        //            return new ModelAction(
        //                hp.PropertyInfo,
        //                new ModelTitle(actionAttribute?.Title),
        //                actionAttribute?.Name);
        //        }).ToList();
        //    }
    }
}