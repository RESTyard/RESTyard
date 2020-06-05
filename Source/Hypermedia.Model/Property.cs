namespace Bluehands.Hypermedia.Model
{
    public class Property
    {
        public string Name { get; }
        public string PropertyName { get; }
        public TypeDescriptor TypeDescriptor { get; }

        public Property(string name, string propertyName, TypeDescriptor typeDescriptor)
        {
            Name = name;
            PropertyName = propertyName;
            TypeDescriptor = typeDescriptor;
        }
    }

    public class KeyProperty
    {
        public Property Property { get; }
        public string TemplateParameterName { get; }

        public KeyProperty(Property property, string templateParameterName)
        {
            Property = property;
            TemplateParameterName = templateParameterName;
        }
    }
}