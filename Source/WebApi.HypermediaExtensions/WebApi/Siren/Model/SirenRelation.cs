namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class SirenRelation
    {
        public SirenRelation(string target)
        {
            Target = target;
        }

        public string Target { get; private set; }
    }
}