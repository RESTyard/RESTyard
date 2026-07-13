using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Supplies the authored API guide (manual) content for the API guide endpoint.
/// Implement this for dynamic content — per-user, localized, or assembled at request time.
/// For a static file use the <c>MapApiGuide(string filePath, …)</c> overload instead.
/// </summary>
public interface IApiGuideProvider
{
    /// <summary>
    /// Returns the guide content as Markdown for the given request.
    /// </summary>
    Task<string> GetGuideAsync(HttpContext context);
}
