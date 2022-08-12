using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;

namespace Benchmarking.Hcos
{
    public abstract class ProgressHco : HypermediaClientObject
    {
        public bool IsDone { get; set; }

        public double Progress { get; set; }

        [HypermediaCommand("Cancel")]
        public IHypermediaClientAction CancelAction { get; set; }
    }

    public abstract class ProgressHco<TResultHco>
        : ProgressHco
        where TResultHco : HypermediaClientObject
    {
        [HypermediaRelations("Result")]
        public TResultHco ResultHco { get; set; }
    }
}