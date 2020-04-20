using WebApi.HypermediaExtensions.Hypermedia.Attributes;

namespace WebApi.Hypermedia.Serializer.Tests.TestHtos.HypermediaObject
{
    [HypermediaObject(Title = "A small title", Classes = new[] {"test.minimal.WithMetaInfo"})]
    public class MinimalWithMetaInfoHto
    {
    }
}