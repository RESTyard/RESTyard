using System.Collections.Generic;
using FunicularSwitch;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Model
{
    public class Base
    {
        public Base(string title, List<Class> classes)
        {
            Title = string.IsNullOrEmpty(title) ? Option<Title>.None : new Title(title);
            Classes = classes;
        }
        public Option<Title> Title { get; private set; }

        // todo add base classes too
        public List<Class> Classes { get; private set; }

        public List<Property> Properties { get; set; }

        public List<Link> Links { get; set; }

        public List<Action> Actions { get; set; }
        
    }
}