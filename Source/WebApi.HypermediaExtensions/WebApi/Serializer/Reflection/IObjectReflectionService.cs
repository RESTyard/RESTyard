using System;
using FunicularSwitch;

namespace WebApi.HypermediaExtensions.WebApi.Serializer.Reflection
{
    public interface IObjectReflectionService
    {
        Result<ObjectReflection> Reflect(Type type);
    }
}