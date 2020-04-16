using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Validator
{
    public abstract class AbstractPropertyValidator
    {
        public List<string> Errors { get; set; }
    }
}