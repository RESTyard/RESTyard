using System.Collections.Generic;

namespace WebApiHypermediaExtensionsCore.Query
{
    public class NavigationQueries
    {
        public Dictionary<string, IHypermediaQuery> Queries { get; set; } = new Dictionary<string, IHypermediaQuery>();
    }
}