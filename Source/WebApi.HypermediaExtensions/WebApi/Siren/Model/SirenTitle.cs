namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenTitle
    {
        public SirenTitle(string description = "")
        {
            Description = string.IsNullOrEmpty(description) ? string.Empty: description;
        }

        public string Description { get; private set; }
    }
}