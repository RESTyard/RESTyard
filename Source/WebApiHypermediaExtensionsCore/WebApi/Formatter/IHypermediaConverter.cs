using WebApiHypermediaExtensionsCore.Hypermedia;

namespace WebApiHypermediaExtensionsCore.WebApi.Formatter
{
    public interface IHypermediaConverter
    {
        string ConvertToString(HypermediaObject hypermediaObject);
    }
}