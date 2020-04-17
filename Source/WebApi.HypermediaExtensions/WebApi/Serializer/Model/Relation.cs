namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Relation
    {
        public Relation(string target)
        {
            Target = target;
        }

        public string Target { get; private set; }
    }
}