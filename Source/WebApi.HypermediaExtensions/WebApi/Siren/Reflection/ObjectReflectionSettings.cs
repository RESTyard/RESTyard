namespace WebApi.HypermediaExtensions.WebApi.Siren.Reflection
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