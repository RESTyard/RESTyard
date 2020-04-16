using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Validator
{
    public abstract class AbstractPropertyValidator
    {
        public List<string> Errors { get; set; }
    }
}