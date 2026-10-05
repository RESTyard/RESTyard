namespace RESTyard.Generator.Templates.csharp_base;

public static class VersionExtensions
{
    extension(Version version)
    {
        public bool HypermediaQueryResultParentExists()
            => version <= new Version(5, 0);

        public bool HypermediaObjectParentExists()
            => version < new Version(5, 0);

        public bool HtoTitlePropertyExists() => version >= new Version(7, 0);

        public bool CustomParameterFromBodyAttributeShouldBeUsed() => version < new Version(7, 0);
    }
}