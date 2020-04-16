using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Model
{
    public class ModelBase
    {

        public ModelTitle SirenTitle { get; set; }

        // todo add base classes too
        public List<ModelClass> SirenClasses { get; set; }

        public List<ModelProperty> SirenProperties { get; set; }

        public List<ModelLink> SirenLinks { get; set; }

        public List<ModelAction> SirenActions { get; set; }
        
    }
}