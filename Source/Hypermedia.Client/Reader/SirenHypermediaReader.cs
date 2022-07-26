using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Client.Resolver;
using RESTyard.Client.Util;

namespace RESTyard.Client.Reader
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
        private readonly IStringParser stringParser;
        private readonly DistinctOrderedStringCollectionComparer distinctOrderedStringCollectionComparer = new DistinctOrderedStringCollectionComparer();

        public SirenHypermediaReader(
            IHypermediaObjectRegister hypermediaObjectRegister,
            IStringParser stringParser)
        {
            this.hypermediaObjectRegister = hypermediaObjectRegister;
            this.hypermediaCommandFactory = RegisterHypermediaCommandFactory.Create();
            this.stringParser = stringParser;
        }

        public HypermediaClientObject Read(
            string contentString,
            IHypermediaResolver resolver)
        {
            // TODO inject deserializer
            // todo catch exception: invalid format
            var rootObject = this.stringParser.Parse(contentString);
            var result = this.ReadHypermediaObject(rootObject, resolver);
            return result;
        }

        public async Task<HypermediaClientObject> ReadAsync(
            Stream contentStream,
            IHypermediaResolver resolver)
        {
            var rootObject = await this.stringParser.ParseAsync(contentStream);
            var result = this.ReadHypermediaObject(rootObject, resolver);
            return result;
        }

        public async Task<(HypermediaClientObject, string)> ReadAndSerializeAsync(
            Stream contentStream,
            IHypermediaResolver resolver)
        {
            var rootObject = await this.stringParser.ParseAsync(contentStream);
            var result = this.ReadHypermediaObject(rootObject, resolver);
            var export = rootObject.Serialize();
            return (result, export);
        }

        private HypermediaClientObject ReadHypermediaObject(
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var classes = ReadClasses(rootObject);
            var hypermediaObjectInstance = this.hypermediaObjectRegister.CreateFromClasses(classes);

            this.ReadTitle(hypermediaObjectInstance, rootObject);
            this.ReadRelations(hypermediaObjectInstance, rootObject);
            this.FillHypermediaProperties(hypermediaObjectInstance, rootObject, resolver);
            

            return hypermediaObjectInstance;
        }

        private void FillHypermediaProperties(
            HypermediaClientObject hypermediaObjectInstance,
            IToken rootObject,
            IHypermediaResolver resolver)
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
                        this.FillLink(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    case HypermediaPropertyType.Entity:
                        this.FillEntity(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    case HypermediaPropertyType.EntityCollection:
                        this.FillEntities(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    case HypermediaPropertyType.Command:
                        this.FillCommand(hypermediaObjectInstance, propertyInfo, rootObject, resolver);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }
        
        //todo linked entities
        //todo no derived types considered
        private void FillEntities(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
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
                var entity = this.ReadHypermediaObject(match, resolver);
                genericAddFunction.Invoke(entityCollection, new object[] { entity });
            }

        }

        private void FillCommand(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var commandAttribute = propertyInfo.GetCustomAttribute<HypermediaCommandAttribute>();
            if (commandAttribute == null)
            {
                throw new Exception($"Hypermedia command '{propertyInfo.Name}' requires a {nameof(HypermediaCommandAttribute)} ");
            }

            // create instance in any case so CanExecute can be called
            var commandInstance = this.CreateHypermediaClientCommand(propertyInfo.PropertyType);

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
            
            this.FillCommandParameters(commandInstance, desiredAction, commandAttribute.Name, resolver);
        }

        private void FillCommandParameters(
            IHypermediaClientCommand commandInstance,
            IToken action,
            string commandName,
            IHypermediaResolver resolver)
        {
            commandInstance.Name = commandName;
            commandInstance.CanExecute = true;
            commandInstance.Resolver = resolver;

            var title = action["title"]?.ValueAsString();
            if (title == null)
            {
                title = string.Empty;
            }
            commandInstance.Title = title;


            var uri = action["href"]?.ValueAsString();
            if (uri == null)
            {
                throw new Exception($"Siren action without href: '{commandName}'");
            }
            commandInstance.Uri = new Uri(uri);

            var method = action["method"]?.ValueAsString();
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
                    Name = field["name"].ValueAsString(),
                    Type = field["type"].ValueAsString(),
                    Classes = field["class"]?.ChildrenAsStrings().ToList(),
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
            var name = action["name"]?.ValueAsString();
            if (name == null)
            {
                return false;
            }

            return name.Equals(commandName);
        }

        private void FillEntity(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
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

            var entity = this.ReadHypermediaObject(jEntity, resolver);
            propertyInfo.SetValue(hypermediaObjectInstance, entity);
        }

        private IDistinctOrderedCollection<string> GetClassesFromEntitiesListProperty(PropertyInfo propertyInfo)
        {
            var genericListType = GetGenericFromICollection(propertyInfo.PropertyType);
            return GetClassesFromEntityProperty(genericListType.GetTypeInfo());
        }

        private static IDistinctOrderedCollection<string> GetClassesFromEntityProperty(TypeInfo targetTypeInfo)
        {
            var classAttribute = targetTypeInfo.GetCustomAttribute<HypermediaClientObjectAttribute>();

            IDistinctOrderedCollection<string> classes;
            if (classAttribute == null || classAttribute.Classes == null)
            {
                classes = new DistinctOrderedStringCollection(targetTypeInfo.Name);
            }
            else
            {
                classes = classAttribute.Classes;
            }

            return classes;
        }

        private bool EntityClassMatch(IToken entity, IDistinctOrderedCollection<string> expectedClasses, string propertyName)
        {
            if (expectedClasses == null || expectedClasses.Count == 0)
            {
                throw new Exception($"No class provided for entity property '{propertyName}'.");
            }

            var actualClasses = entity["class"];
            if (actualClasses == null)
            {
                return false;
            }

            return this.distinctOrderedStringCollectionComparer.Equals(new DistinctOrderedStringCollection(actualClasses.ChildrenAsStrings()), expectedClasses);
        }

        private bool EntityRelationsMatch(IToken entity, IDistinctOrderedCollection<string> expectedRelations)
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

            return this.distinctOrderedStringCollectionComparer.Equals(new DistinctOrderedStringCollection(actualRelations.ChildrenAsStrings()), expectedRelations);
        }


        private void FillLink(
            HypermediaClientObject hypermediaObjectInstance,
            PropertyInfo propertyInfo,
            IToken rootObject,
            IHypermediaResolver resolver)
        {
            var linkAttribute = propertyInfo.GetCustomAttribute<HypermediaRelationsAttribute>();
            if (linkAttribute == null)
            {
                throw new Exception($"{nameof(IHypermediaLink)} requires a {nameof(HypermediaRelationsAttribute)} Attribute.");
            }

            var hypermediaLink = (IHypermediaLink)Activator.CreateInstance(propertyInfo.PropertyType);
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

            var link = links.FirstOrDefault(l => this.distinctOrderedStringCollectionComparer.Equals(new DistinctOrderedStringCollection(l["rel"].ChildrenAsStrings()), linkAttribute.Relations));
            if (link == null)
            {
                if (IsMandatoryHypermediaLink(propertyInfo))
                {
                    throw new Exception($"Mandatory link not found {propertyInfo.Name}");
                }
                return;
            }
            
            hypermediaLink.Uri = new Uri(link["href"].ValueAsString());
            hypermediaLink.Relations = link["rel"].ChildrenAsStrings().ToList();
            hypermediaLink.Resolver = resolver;
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

            var propertyValue = properties[propertyInfo.Name];
            if (propertyValue == null)
            {
                if (IsMandatoryHypermediaProperty(propertyInfo))
                {
                    throw new Exception($"Mandatory property not found {propertyInfo.Name}");
                }

                return;
            }

            propertyInfo.SetValue(hypermediaObjectInstance, propertyValue.ToObject(propertyInfo.PropertyType));
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
            var relations = rootObject["rel"]?.ChildrenAsStrings();
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

            hypermediaObjectInstance.Title = title.ValueAsString();
        }      

        private static IDistinctOrderedCollection<string> ReadClasses(IToken rootObject)
        {
            // todo catch exception
            // rel migth be missing so provide better error message
            return new DistinctOrderedStringCollection(rootObject["class"].ChildrenAsStrings());
        }
    }
}