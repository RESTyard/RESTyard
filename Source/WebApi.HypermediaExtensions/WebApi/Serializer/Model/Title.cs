namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Title
    {
        public Title(string description = "")
        {
            Description = string.IsNullOrEmpty(description) ? string.Empty: description;
        }

        public string Description { get; private set; }
    }
}