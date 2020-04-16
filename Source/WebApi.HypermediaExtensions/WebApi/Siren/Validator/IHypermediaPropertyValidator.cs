using System;
using System.Collections.Generic;

namespace WebApi.HypermediaExtensions.WebApi.Siren.Validator
{
    public interface IHypermediaPropertyValidator
    {
        bool IsValid(Type propertyType);

        List<string> Errors { get; set; }
    }
}