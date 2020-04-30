using System;
using System.Threading.Tasks;

namespace Bluehands.Hypermedia.Model
{
    public abstract class TypeDescriptor
    {
        public static TypeDescriptor CSharp(string name, string fullName) => new CSharp_(name, fullName);

        public class CSharp_ : TypeDescriptor
        {
            public string FullName { get; }
            public string Name { get; }

            public CSharp_(string name, string fullName) : base(UnionCases.CSharp)
            {
                Name = name;
                FullName = fullName;
            }
        }

        internal enum UnionCases
        {
            CSharp
            //JsonSchema ??
        }

        internal UnionCases UnionCase { get; }
        TypeDescriptor(UnionCases unionCase) => UnionCase = unionCase;

        public override string ToString() => Enum.GetName(typeof(UnionCases), UnionCase) ?? UnionCase.ToString();
        bool Equals(TypeDescriptor other) => UnionCase == other.UnionCase;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TypeDescriptor)obj);
        }

        public override int GetHashCode() => (int)UnionCase;
    }

    public static class TypeDescriptorExtension
    {
        public static T Match<T>(this TypeDescriptor typeDescriptor, Func<TypeDescriptor.CSharp_, T> cSharp)
        {
            switch (typeDescriptor.UnionCase)
            {
                case TypeDescriptor.UnionCases.CSharp:
                    return cSharp((TypeDescriptor.CSharp_)typeDescriptor);
                default:
                    throw new ArgumentException($"Unknown type implementing TypeDescriptor: {typeDescriptor.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this TypeDescriptor typeDescriptor, Func<TypeDescriptor.CSharp_, Task<T>> cSharp)
        {
            switch (typeDescriptor.UnionCase)
            {
                case TypeDescriptor.UnionCases.CSharp:
                    return await cSharp((TypeDescriptor.CSharp_)typeDescriptor).ConfigureAwait(false);
                default:
                    throw new ArgumentException($"Unknown type implementing TypeDescriptor: {typeDescriptor.GetType().Name}");
            }
        }

        public static async Task<T> Match<T>(this Task<TypeDescriptor> typeDescriptor, Func<TypeDescriptor.CSharp_, T> cSharp) => (await typeDescriptor.ConfigureAwait(false)).Match(cSharp);
        public static async Task<T> Match<T>(this Task<TypeDescriptor> typeDescriptor, Func<TypeDescriptor.CSharp_, Task<T>> cSharp) => await(await typeDescriptor.ConfigureAwait(false)).Match(cSharp).ConfigureAwait(false);
    }
}