using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Model
{
    public abstract class Link
    {
        public static Link KeyReference(string name, EntityKey referencedEntity, IEnumerable<string> relations) => new KeyReference_(name, referencedEntity, relations);
        public static Link ObjectReference(string name, EntityKey referencedEntity, IEnumerable<string> relations) => new ObjectReference_(name, referencedEntity, relations);
        public static Link ExternalReference(string name, IEnumerable<string> relations) => new ExternalReference_(name, relations);

        public string Name { get; }
        public ImmutableArray<string> Relations { get; }

        public class KeyReference_ : Link
        {
            public EntityKey ReferencedEntity { get; }

            public KeyReference_(string name, EntityKey referencedEntity, IEnumerable<string> relations) : base(UnionCases.KeyReference, name, relations.ToImmutableArray()) => ReferencedEntity = referencedEntity;
        }

        public class ObjectReference_ : Link
        {
            public EntityKey ReferencedEntity { get; }

            public ObjectReference_(string name, EntityKey referencedEntity, IEnumerable<string> relations) : base(UnionCases.ObjectReference, name, relations.ToImmutableArray()) => ReferencedEntity = referencedEntity;
        }

        public class ExternalReference_ : Link
        {
            public ExternalReference_(string name, IEnumerable<string> relations) : base(UnionCases.ExternalReference, name, relations.ToImmutableArray())
            {
            }
        }

        internal enum UnionCases
        {
            KeyReference,
            ObjectReference,
            ExternalReference
        }

        internal UnionCases UnionCase { get; }
        Link(UnionCases unionCase, string name, ImmutableArray<string> relations)
        {
            UnionCase = unionCase;
            Name = name;
            Relations = relations;
        }

        public override string ToString() => Enum.GetName(typeof(UnionCases), UnionCase) ?? UnionCase.ToString();
        bool Equals(Link other) => UnionCase == other.UnionCase;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Link)obj);
        }

        public override int GetHashCode() => (int)UnionCase;
    }

    public static class LinkExtension
    {
        public static T Match<T>(this Link link, Func<Link.KeyReference_, T> keyReference, Func<Link.ObjectReference_, T> objectReference, Func<Link.ExternalReference_, T> externalReference)
        {
            switch (link.UnionCase)
            {
                case Link.UnionCases.KeyReference:
                    return keyReference((Link.KeyReference_)link);
                case Link.UnionCases.ObjectReference:
                    return objectReference((Link.ObjectReference_)link);
                case Link.UnionCases.ExternalReference:
                    return externalReference((Link.ExternalReference_)link);
                default:
                    throw new ArgumentException($"Unknown type implementing Link: {link.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this Link link, Func<Link.KeyReference_, Task<T>> keyReference, Func<Link.ObjectReference_, Task<T>> objectReference, Func<Link.ExternalReference_, Task<T>> externalReference)
        {
            switch (link.UnionCase)
            {
                case Link.UnionCases.KeyReference:
                    return await keyReference((Link.KeyReference_)link).ConfigureAwait(false);
                case Link.UnionCases.ObjectReference:
                    return await objectReference((Link.ObjectReference_)link).ConfigureAwait(false);
                case Link.UnionCases.ExternalReference:
                    return await externalReference((Link.ExternalReference_)link).ConfigureAwait(false);
                default:
                    throw new ArgumentException($"Unknown type implementing Link: {link.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this Task<Link> link, Func<Link.KeyReference_, T> keyReference, Func<Link.ObjectReference_, T> objectReference, Func<Link.ExternalReference_, T> externalReference) => (await link.ConfigureAwait(false)).Match(keyReference, objectReference, externalReference);
        public static async Task<T> Match<T>(this Task<Link> link, Func<Link.KeyReference_, Task<T>> keyReference, Func<Link.ObjectReference_, Task<T>> objectReference, Func<Link.ExternalReference_, Task<T>> externalReference) => await (await link.ConfigureAwait(false)).Match(keyReference, objectReference, externalReference).ConfigureAwait(false);
    }
}