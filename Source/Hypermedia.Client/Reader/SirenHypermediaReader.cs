using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Client.Hypermedia.Commands;
using Bluehands.Hypermedia.Client.Resolver;
using Bluehands.Hypermedia.Client.Util;

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
        private readonly ISirenStringParser sirenStringParser;
        private readonly StringCollectionComparer stringCollectionComparer = new StringCollectionComparer();

        public SirenHypermediaReader(
            IHypermediaObjectRegister hypermediaObjectRegister, 
            IHypermediaResolver resolver,
            ISirenStringParser sirenStringParser)
        {
            this.hypermediaObjectRegister = hypermediaObjectRegister;
            this.hypermediaCommandFactory = RegisterHypermediaCommandFactory.Create();
            this.resolver = resolver;
            this.sirenStringParser = sirenStringParser;
        }

        public HypermediaClientObject Read(string contentString)
        {
            // TODO inject deserializer
            // todo catch exception: invalid format
            var rootObject = this.sirenStringParser.Parse(contentString);
            var result = this.ReadHypermediaObject(rootObject);
            return result;
        }

        private HypermediaClientObject ReadHypermediaObject(IToken rootObject)
        {
            var classes = ReadClasses(rootObject);
            var hypermediaObjectInstance = this.hypermediaObjectRegister.CreateFromClasses(classes);

            this.ReadTitle(hypermediaObjectInstance, rootObject);
            this.ReadRelations(hypermediaObjectInstance, rootObject);
            this.FillHypermediaProperties(hypermediaObjectInstance, rootObject);
            

            return hypermediaObjectInstance;
        }

        private void FillHypermediaProperties(HypermediaClientObject hypermediaObjectInstance, IToken rootObject)
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
                        FillProperty(hypermediaObjectInstance, propertyInfo, rootObject);
                        break;
                   
                    case HypermediaPropertyType.Link:
                        this.FillLink(hypermediaObjectInstance, propertyInfo, rootObject);
                        break;

                    case HypermediaPropertyType.Entity:
                        this.FillEntity(hypermediaObjectInstance, propertyInfo, rootObject);
                        break;
                    case HypermediaPropertyType.EntityCollection:
                        this.FillEntities(hypermediaObjectInstance, propertyInfo, rootObject);
                        break;
                    case HypermediaPropertyType.Command:
                        this.FillCommand(hypermediaObjectInstance, propertyInfo, rootObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }
        
        //todo linked entities
        //todo no derived types considered
        private void FillEntities(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, IToken rootObject)
        {
            var relationsAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            var classes = this.GetClassesFromEntitiesListProperty(propertyInfo);

            var entityCollection = Activator.CreateInstance(propertyInfo.PropertyType);
            propertyInfo.SetValue(hypermediaObjectInstance, entityCollection);

            var entities = rootObject["entities"];
            if (entities == null)
            {
                return;
            }

            var matchingEntities = entities.Where(e =>
                this.EntityRelationsMatch(e, relationsAttribute.Relations) && this.EntityClassMatch(e, classes, propertyInfo.Name));

            var genericAddFunction = entityCollection.GetType().GetTypeInfo().GetMethod("Add");

            foreach (var match in matchingEntities)
            {
                var entity = this.ReadHypermediaObject(match);
                genericAddFunction.Invoke(entityCollection, new object[] { entity });
            }

        }


        private void FillCommand(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, IToken rootObject)
        {
            var commandAttribute = propertyInfo.GetCustomAttribute<HypermediaCommandAttribute>();
            if (commandAttribute == null)
            {
                throw new Exception($"Hypermedia command '{propertyInfo.Name}' requires a {nameof(HypermediaCommandAttribute)} ");
            }

            // create instance in any case so CanExecute can be called
            var commandInstance = this.CreateHypermediaClientCommand(propertyInfo.PropertyType);
            commandInstance.Resolver = this.resolver;

            propertyInfo.SetValue(hypermediaObjectInstance, commandInstance);

            var actions = rootObject["actions"];
            var desiredAction = actions.FirstOrDefault(e => this.IsDesiredAction(e, commandAttribute.Name));
            if (actions == null || desiredAction == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    throw new Exception($"Mandatory hypermedia command '{propertyInfo.Name}' not found.");
                }
                return;
            }
            
            this.FillCommandParameters(commandInstance, desiredAction, commandAttribute.Name);
        }

        private void FillCommandParameters(IHypermediaClientCommand commandInstance, IToken action, string commandName)
        {
            commandInstance.Name = commandName;
            commandInstance.CanExecute = true;

            var title = action["title"]?.AsString();
            if (title == null)
            {
                title = string.Empty;
            }
            commandInstance.Title = title;


            var uri = action["href"]?.AsString();
            if (uri == null)
            {
                throw new Exception($"Siren action without href: '{commandName}'");
            }
            commandInstance.Uri = new Uri(uri);

            var method = action["method"]?.AsString();
            if (method == null)
            {
                method = "GET";
            }
            commandInstance.Method = method;

            var fields = action["fields"];
            if (!commandInstance.HasParameters && fields != null)
            {
                throw new Exception($"hypermedia Command '{commandName}' has no parameter but hypermedia document indicates parameters.");
            }
            if (fields == null)
            {
                if (commandInstance.HasParameters)
                {
                    throw new Exception($"hypermedia Command '{commandName}' has parameter but hypermedia document has not.");
                }

                return;
            }

            foreach (var field in fields)
            {
                var parameterDescription = new ParameterDescription
                {
                    Name = field["name"].AsString(),
                    Type = field["type"].AsString(),
                    Classes = field["class"]?.AsStrings().ToList(),
                };
                // todo optional but not save, or check annotation on class

                commandInstance.ParameterDescriptions.Add(parameterDescription);
            }
        }

        private IHypermediaClientCommand CreateHypermediaClientCommand(Type commandType)
        {
            var commandInstance = this.hypermediaCommandFactory.Create(commandType);
            return commandInstance;
        }

        private bool IsDesiredAction(IToken action, string commandName)
        {
            var name = action["name"]?.AsString();
            if (name == null)
            {
                return false;
            }

            return name.Equals(commandName);
        }

        private void FillEntity(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, IToken rootObject)
        {
            var relationsAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            var classes = GetClassesFromEntityProperty(propertyInfo.PropertyType.GetTypeInfo());

            var entities = rootObject["entities"];
            if (entities == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    throw new Exception($"Mandatory hypermedia property can not be filled {propertyInfo.Name}: server object contains no entities.");
                }
                return;
            }

            var jEntity = entities.FirstOrDefault( e =>
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

        private bool EntityClassMatch(IToken entity, string[] expectedClasses, string propertyName)
        {
            if (expectedClasses == null || expectedClasses.Length == 0)
            {
                throw new Exception($"No class provided for entity property '{propertyName}'.");
            }

            var actualClasses = entity["class"];
            if (actualClasses == null)
            {
                return false;
            }

            return this.stringCollectionComparer.Equals(actualClasses.AsStrings().ToArray(), expectedClasses);
        }

        private bool EntityRelationsMatch(IToken entity, ICollection<string> expectedRelations)
        {
            if (expectedRelations == null || expectedRelations.Count == 0)
            {
                return true;
            }

            var actualRelations = entity["rel"];
            if (actualRelations == null)
            {
                return false;
            }

            return this.stringCollectionComparer.Equals(actualRelations.AsStrings().ToArray(), expectedRelations);
        }


        private void FillLink(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, IToken rootObject)
        {
            var linkAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            if (linkAttribute == null)
            {
                throw new Exception($"{nameof(IHypermediaLink)} requires a {nameof(HypermediaRelationsAttribute)} Attribute.");
            }

            var hypermediaLink = (IHypermediaLink)Activator.CreateInstance(propertyInfo.PropertyType);
            hypermediaLink.Resolver = this.resolver;
            propertyInfo.SetValue(hypermediaObjectInstance, hypermediaLink);

            var links = rootObject["links"];
            if (links == null)
            {
                if (IsMandatoryHypermediaLink(propertyInfo))
                {
                    throw new Exception($"Mandatory link not found {propertyInfo.Name}");
                }
                return;
            }

            var link = links.FirstOrDefault(l => this.stringCollectionComparer.Equals(l["rel"].AsStrings().ToList(), linkAttribute.Relations));
            if (link == null)
            {
                if (IsMandatoryHypermediaLink(propertyInfo))
                {
                    throw new Exception($"Mandatory link not found {propertyInfo.Name}");
                }
                return;
            }
            
            hypermediaLink.Uri = new Uri(link["href"].AsString());
            hypermediaLink.Relations = link["rel"].AsStrings().ToList();
        }

        // todo attribute with different property name
        private static void FillProperty(HypermediaClientObject hypermediaObjectInstance, PropertyInfo propertyInfo, IToken rootObject)
        {
            var properties = rootObject["properties"];
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

        private void ReadRelations(HypermediaClientObject hypermediaObjectInstance, IToken rootObject)
        {
            var relations = rootObject["rel"]?.AsStrings();
            if (relations == null)
            {
                return;
            }

            hypermediaObjectInstance.Relations = relations.ToList();
        }

        private void ReadTitle(HypermediaClientObject hypermediaObjectInstance, IToken rootObject)
        {
            var title = rootObject["title"];
            if (title == null)
            {
                return;
            }

            hypermediaObjectInstance.Title = title.AsString();
        }      

        private static ICollection<string> ReadClasses(IToken rootObject)
        {
            // todo catch exception
            // rel migth be missing so provide better error message
            return rootObject["class"].AsStrings().ToList();
        }
    }
}