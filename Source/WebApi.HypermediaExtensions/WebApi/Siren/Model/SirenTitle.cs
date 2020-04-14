namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenTitle
    {
        public SirenTitle(string description)
        {
            Description = description;
        }

        public string Description { get; private set; }
    }
}