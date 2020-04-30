namespace Bluehands.Hypermedia.Model
{
    public class Property
    {
        public string Name { get; }

        public TypeDescriptor TypeDescriptor { get; }

        public Property(string name, TypeDescriptor typeDescriptor)
        {
            Name = name;
            TypeDescriptor = typeDescriptor;
        }
    }
}