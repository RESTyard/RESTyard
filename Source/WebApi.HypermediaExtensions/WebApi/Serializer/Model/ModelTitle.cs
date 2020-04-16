namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class ModelTitle
    {
        public ModelTitle(string description = "")
        {
            Description = string.IsNullOrEmpty(description) ? string.Empty: description;
        }

        public string Description { get; private set; }
    }
}