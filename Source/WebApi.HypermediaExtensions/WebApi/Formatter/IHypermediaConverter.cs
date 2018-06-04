using WebApi.HypermediaExtensions.Hypermedia;

namespace WebApi.HypermediaExtensions.WebApi.Formatter
{
    public interface IHypermediaConverter
    {
        string ConvertToString(HypermediaObject hypermediaObject);
    }
}