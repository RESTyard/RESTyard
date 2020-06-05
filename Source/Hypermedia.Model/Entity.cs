using System.Collections.Generic;
using System.Collections.Immutable;
using FunicularSwitch;

namespace Bluehands.Hypermedia.Model
{
    public class Entity
    {
        public string Name => Key.Name;
        public string Namespace => Key.Namespace;

        public EntityKey Key { get; }
        public string Title { get; }
        public ImmutableArray<string> Classes { get; }
        public ImmutableArray<KeyProperty> KeyProperties { get; }
        public ImmutableArray<Property> Properties { get; }
        public ImmutableArray<Link> Links { get; }
        public ImmutableArray<SubEntity> Entities { get; }


        public Entity(string name, string title, string ns, IEnumerable<string> classes,
            IEnumerable<Property> properties, IEnumerable<KeyProperty> keyProperties, IEnumerable<Link> links, IEnumerable<SubEntity> entities)
        {
            Key = new EntityKey(name, ns);
            Title = title;
            Classes = classes.ToImmutableArray();
            Properties = properties.ToImmutableArray();
            KeyProperties = keyProperties.ToImmutableArray();
            Links = links.ToImmutableArray();
            Entities = entities.ToImmutableArray();
        }
    }

    public class EntityKey
    {
        public string Name { get; }
        public string Namespace { get; }

        public EntityKey(string name, string ns = null)
        {
            Name = name;
            Namespace = ns;
        }

        protected bool Equals(EntityKey other) => Name == other.Name && Namespace == other.Namespace;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((EntityKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
            }
        }
    }

    public class Action
    {
        public string Name { get; }
        public string ActionName { get; }
        public string Title { get; }
        public Option<TypeDescriptor> ParameterType { get; }
        public Option<TypeDescriptor> ReturnType { get; }

        public Action(string name, string actionName, string title, Option<TypeDescriptor> parameterType, Option<TypeDescriptor> returnType)
        {
            Name = name;
            ActionName = actionName;
            Title = title;
            ParameterType = parameterType;
            ReturnType = returnType;
        }
    }
}
