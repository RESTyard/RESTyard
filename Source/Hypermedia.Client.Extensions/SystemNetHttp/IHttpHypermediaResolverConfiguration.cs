using System;
using System.Net.Http.Headers;
using Bluehands.Hypermedia.Client.Authentication;

namespace Bluehands.Hypermedia.Client.Extensions.SystemNetHttp
{
    public interface IHttpHypermediaResolverConfiguration
    {
        /// <summary>
        /// Adds UsernamePasswordCredentials as authorization to every request
        /// </summary>
        /// <param name="usernamePasswordCredentials"></param>
        void SetCredentials(UsernamePasswordCredentials usernamePasswordCredentials);

        /// <summary>
        /// Adds additional headers to every request
        /// </summary>
        /// <param name="addCustomDefaultHeadersAction"></param>
        void SetCustomDefaultHeaders(Action<HttpRequestHeaders> addCustomDefaultHeadersAction);
    }
}