using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;
using Bluehands.Hypermedia.Client.Resolver;
using Bluehands.Hypermedia.Client.Util;
using Newtonsoft.Json.Linq;

namespace Bluehands.Hypermedia.Client.Reader
{
    internal enum HypermediaPropertyType
    {
        Unknown,
        Property,
        Entity,
        EntityCollection,
        Link,
        Command
    }

    public class SirenHypermediaReader : IHypermediaReader
    {
        private readonly IHypermediaObjectRegister hypermediaObjectRegister;
        private readonly IHypermediaCommandFactory hypermediaCommandFactory;
        private readonly IHypermediaResolver resolver;
        private readonly StringCollectionComparer stringCollectionComparer = new StringCollectionComparer();

        public SirenHypermediaReader(IHypermediaObjectRegister hypermediaObjectRegister, IHypermediaResolver resolver)
        {
            this.hypermediaObjectRegister = hypermediaObjectRegister;
            this.hypermediaCommandFactory = RegisterHypermediaCommandFactory.Create();
            this.resolver = resolver;
        }

        public HypermediaClientObject Read(string contentString)
        {
            // TODO inject deserializer
            // todo catch exception: invalid format
            var jObject = JObject.Parse(contentString);
            var result = this.ReadHypermediaObject(jObject);
            return result;
        }

        private HypermediaClientObject ReadHypermediaObject(JObject jObject)
        {
            var classes = ReadClasses(jObject);
            var hypermediaObjectInstance = this.hypermediaObjectRegister.CreateFromClasses(classes);

            this.ReadTitle(hypermediaObjectInstance, jObject);
            this.ReadRelations(hypermediaObjectInstance, jObject);
            this.FillHypermediaProperties(hypermediaObjectInstance, jObject);
            

            return hypermediaObjectInstance;
        }

        private void FillHypermediaProperties(HypermediaClientObject hypermediaObjectInstance, JObject jObject)
        {
            var typeInfo = hypermediaObjectInstance.GetType().GetTypeInfo();
            var properties = typeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in properties)
            {
                var ignore = propertyInfo.GetCustomAttribute<ClientIgnoreHypermediaPropertyAttribute>() != null;
                if (ignore)
                {
                    continue;
                }

                var hypermediaPropertyType = GetHypermediaPropertyType(propertyInfo);
                switch (hypermediaPropertyType)
                {
                    case HypermediaPropertyType.Property:
                        FillProperty(hypermediaObjectInstance, propertyInfo, jObject);
                        break;
                   
                    case HypermediaPropertyType.Link:
                        this.FillLink(hypermediaObjectInstance, propertyInfo, jObject);
                        break;

                    case HypermediaPropertyType.Entity:
                        this.FillEntity(hypermediaObjectInstance, propertyInfo, jObject);
                        break;
                    case HypermediaPropertyType.EntityCollection:
                        this.FillEntities(hypermediaObjectInstance, propertyInfo, jObject);
                        break;
                    case HypermediaPropertyType.Command:
                        this.FillCommand(hypermediaObjectInstance, propertyInfo, jObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }
        
        //todo linked entities
        //todo no derived types considered
        private void FillEntities(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, JObject jObject)
        {
            var relationsAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            var classes = this.GetClassesFromEntitiesListProperty(propertyInfo);

            var entityCollection = Activator.CreateInstance(propertyInfo.PropertyType);
            propertyInfo.SetValue(hypermediaObjectInstance, entityCollection);

            var entities = jObject["entities"] as JArray;
            if (entities == null)
            {
                return;
            }

            var jEntities = entities.Cast<JObject>().Where(e =>
                this.EntityRelationsMatch(e, relationsAttribute.Relations) && this.EntityClassMatch(e, classes, propertyInfo.Name));

            var genericAddFunction = entityCollection.GetType().GetTypeInfo().GetMethod("Add");

            foreach (var jEntity in jEntities)
            {
                var entity = this.ReadHypermediaObject(jEntity);
                genericAddFunction.Invoke(entityCollection, new object[] { entity });
            }

        }


        private void FillCommand(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, JObject jObject)
        {
            var commandAttribute = propertyInfo.GetCustomAttribute<HypermediaCommandAttribute>();
            if (commandAttribute == null)
            {
                throw new Exception($"Hypermedia command '{propertyInfo.Name}' requires a {typeof(HypermediaCommandAttribute).Name} ");
            }

            // create instance in any case so CanExecute can be called
            var commandInstance = this.CreateHypermediaClientCommand(propertyInfo.PropertyType);
            commandInstance.Resolver = this.resolver;

            propertyInfo.SetValue(hypermediaObjectInstance, commandInstance);

            var actions = jObject["actions"] as JArray;
            var jAction = actions.Cast<JObject>().FirstOrDefault(e => this.IsDesiredAction(e, commandAttribute.Name));
            if (actions == null || jAction == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    throw new Exception($"Mandatory hypermedia command '{propertyInfo.Name}' not found.");
                }
                return;
            }
            
            this.FillCommandParameters(commandInstance, jAction, commandAttribute.Name);
        }

        private void FillCommandParameters(IHypermediaClientCommand commandInstance, JObject jAction, string commandName)
        {
            commandInstance.Name = commandName;
            commandInstance.CanExecute = true;

            var title = jAction["title"]?.Value<string>();
            if (title == null)
            {
                title = string.Empty;
            }
            commandInstance.Title = title;


            var uri = jAction["href"]?.Value<string>();
            if (uri == null)
            {
                throw new Exception($"Siren action without href: '{commandName}'");
            }
            commandInstance.Uri = new Uri(uri);

            var method = jAction["method"]?.Value<string>();
            if (method == null)
            {
                method = "GET";
            }
            commandInstance.Method = method;

            var jFields = jAction["fields"] as JArray;
            if (!commandInstance.HasParameters && jFields != null)
            {
                throw new Exception($"hypermedia Command '{commandName}' has no parameter but hypermedia document indicates parameters.");
            }
            if (jFields == null)
            {
                if (commandInstance.HasParameters)
                {
                    throw new Exception($"hypermedia Command '{commandName}' has parameter but hypermedia document has not.");
                }

                return;
            }

            foreach (var jField in jFields)
            {
                var parameterDescription = new ParameterDescription();
                parameterDescription.Name = jField["name"].Value<string>();
                parameterDescription.Type = jField["type"].Value<string>();
                parameterDescription.Classes = jField["class"]?.Values<string>().ToList(); // todo optional but not save, or check annotation on class

                commandInstance.ParameterDescriptions.Add(parameterDescription);
            }
        }

        private IHypermediaClientCommand CreateHypermediaClientCommand(Type commandType)
        {
            var commandInstance = this.hypermediaCommandFactory.Create(commandType);
            return commandInstance;
        }

        private bool IsDesiredAction(JObject jObject, string commandName)
        {
            var name = jObject["name"]?.Value<string>();
            if (name == null)
            {
                return false;
            }

            return name.Equals(commandName);
        }

        private void FillEntity(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, JObject jObject)
        {
            var relationsAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            var classes = GetClassesFromEntityProperty(propertyInfo.PropertyType.GetTypeInfo());

            var entities = jObject["entities"] as JArray;
            if (entities == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    throw new Exception($"Mandatory hypermedia property can not be filled {propertyInfo.Name}: server object contains no entities.");
                }
                return;
            }

            var jEntity = entities.Cast<JObject>().FirstOrDefault( e =>
                this.EntityRelationsMatch(e, relationsAttribute.Relations) && this.EntityClassMatch(e, classes, propertyInfo.Name));

            if (jEntity == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo)) { 
                    throw new Exception($"Mandatory hypermedia property can not be filled {propertyInfo.Name}: server object contains no entity of matching type (relation and class).");
                }
                return;
            }

            var entity = this.ReadHypermediaObject(jEntity);
            propertyInfo.SetValue(hypermediaObjectInstance, entity);
        }

        private string[] GetClassesFromEntitiesListProperty(PropertyInfo propertyInfo)
        {
            var genericListType = GetGenericFromICollection(propertyInfo.PropertyType);
            return GetClassesFromEntityProperty(genericListType.GetTypeInfo());
        }

        private static string[] GetClassesFromEntityProperty(TypeInfo targetTypeInfo)
        {
            var classAttribute = targetTypeInfo.GetCustomAttribute<HypermediaClientObjectAttribute>();

            string[] classes;
            if (classAttribute == null || classAttribute.Classes == null)
            {
                classes = new[] {targetTypeInfo.Name};
            }
            else
            {
                classes = classAttribute.Classes;
            }

            return classes;
        }

        private bool EntityClassMatch(JObject jObject, string[] classes, string propertyName)
        {
            if (classes == null || classes.Length == 0)
            {
                throw new Exception($"No class provided for entity property '{propertyName}'.");
            }

            var jClasses = jObject["class"];
            if (jClasses == null)
            {
                return false;
            }

            return this.stringCollectionComparer.Equals(jClasses.Values<string>().ToArray(), classes);
        }

        private bool EntityRelationsMatch(JObject jObject, string[] relations)
        {
            if (relations == null || relations.Length == 0)
            {
                return true;
            }

            var rels = jObject["rel"];
            if (rels == null)
            {
                return false;
            }

            return this.stringCollectionComparer.Equals(rels.Values<string>().ToArray(), relations);
        }


        private void FillLink(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, JObject jObject)
        {
            var linkAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            if (linkAttribute == null)
            {
                throw new Exception($"{typeof(IHypermediaLink).Name} requires a {typeof(HypermediaRelationsAttribute).Name} Attribute.");
            }

            var hypermediaLink = (IHypermediaLink)Activator.CreateInstance(propertyInfo.PropertyType);
            hypermediaLink.Resolver = this.resolver;
            propertyInfo.SetValue(hypermediaObjectInstance, hypermediaLink);

            var links = jObject["links"];
            if (links == null)
            {
                if (IsMandatoryHypermediaLink(propertyInfo))
                {
                    throw new Exception($"Mandatory link not found {propertyInfo.Name}");
                }
                return;
            }

            var link = links.FirstOrDefault(l => this.stringCollectionComparer.Equals(l["rel"].Values<string>().ToList(), linkAttribute.Relations.ToList()));
            if (link == null)
            {
                if (IsMandatoryHypermediaLink(propertyInfo))
                {
                    throw new Exception($"Mandatory link not found {propertyInfo.Name}");
                }
                return;
            }
            
            hypermediaLink.Uri = new Uri(link["href"].Value<string>());
            hypermediaLink.Relations = link["rel"].Values<string>().ToList();
        }

        // todo attribute with different property name
        private static void FillProperty(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, JObject jObject)
        {
            var properties = jObject["properties"];
            if (properties == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo)) { 
                    throw new Exception($"Mandatory property not found {propertyInfo.Name}");
                }

                return;
            }

            var jPropertyValue = properties[propertyInfo.Name];
            if (jPropertyValue == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    throw new Exception($"Mandatory property not found {propertyInfo.Name}");
                }

                return;
            }

            propertyInfo.SetValue(hypermediaObjectInstance, jPropertyValue.ToObject(propertyInfo.PropertyType));
        }

        private static bool IsMandatoryHypermediaProperty(PropertyInfo propertyInfo)
        {
            return propertyInfo.GetCustomAttribute<MandatoryAttribute>() != null;
        }

        private static bool IsMandatoryHypermediaLink(PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.GetTypeInfo().IsAssignableFrom(typeof(MandatoryHypermediaLink<>))
                || propertyInfo.GetCustomAttribute<MandatoryAttribute>() != null;
        }

        private static HypermediaPropertyType GetHypermediaPropertyType(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            if (typeof(IHypermediaLink).GetTypeInfo().IsAssignableFrom(propertyType))
            {
                return HypermediaPropertyType.Link;
            }

            if (typeof(HypermediaClientObject).GetTypeInfo().IsAssignableFrom(propertyType))
            {
                return HypermediaPropertyType.Entity;
            }

            if (typeof(IHypermediaClientCommand).GetTypeInfo().IsAssignableFrom(propertyType))
            {
                return HypermediaPropertyType.Command;
            }

            var isCollection = IsGenericCollection(propertyType);
            if (isCollection)
            {
                var collectionType = GetGenericFromICollection(propertyType);
                if (typeof(HypermediaClientObject).GetTypeInfo().IsAssignableFrom(collectionType))
                {
                    return HypermediaPropertyType.EntityCollection;
                }
            }

            return HypermediaPropertyType.Property;
        }

        private static bool IsGenericCollection(Type propertyType)
        {
            return propertyType.GetTypeInfo().IsGenericType && propertyType.GetTypeInfo().GetInterface("ICollection") != null;
        }

        private static Type GetGenericFromICollection(Type type)
        {
            return type.GetTypeInfo().GetGenericArguments()[0];
        }

        private void ReadRelations(HypermediaClientObject hypermediaObjectInstance, JObject jobject)
        {
            var rels = jobject["rel"]?.Values<string>();
            if (rels == null)
            {
                return;
            }

            hypermediaObjectInstance.Relations = rels.ToList();
        }

        private void ReadTitle(HypermediaClientObject hypermediaObjectInstance, JObject jobject)
        {
            var title = jobject["title"];
            if (title == null)
            {
                return;
            }

            hypermediaObjectInstance.Title = title.Value<string>();
        }      

        private static List<string> ReadClasses(JObject jobject)
        {
            // todo catch exception
            // rel migth be missing so provide better error message
            return jobject["class"].Values<string>().ToList();
        }
    }
}