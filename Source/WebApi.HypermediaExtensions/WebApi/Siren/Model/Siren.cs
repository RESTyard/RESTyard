using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class Siren
    {

        public SirenTitle SirenTitle { get; set; }

        public List<SirenClass> SirenClasses { get; set; }

        public List<SirenProperty> SirenProperties { get; set; }

        public List<SirenLink> SirenLinks { get; set; }
        
    }
}