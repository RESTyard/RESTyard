namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class ModelRelation
    {
        public ModelRelation(string target)
        {
            Target = target;
        }

        public string Target { get; private set; }
    }
}