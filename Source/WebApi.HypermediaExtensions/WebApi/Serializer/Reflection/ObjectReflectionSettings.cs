namespace WebApi.HypermediaExtensions.WebApi.Serializer.Reflection
{
    public class ObjectReflectionSettings
    {
        public PropertyReflectionValidationMode PropertyReflectionValidationMode { get; set; } =
            PropertyReflectionValidationMode.SkipAndWarnOnInconsistencies;
    }

    public enum PropertyReflectionValidationMode
    {
        TrowOnAnyInconsistency,
        SkipAndWarnOnInconsistencies,
        IgnoreAndWarnInconsistencies,
    }
}