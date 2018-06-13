namespace Hypermedia.Client.Reader.ProblemJson
{
    using System;
    using System.Net.Http;

    using global::Hypermedia.Client.Exceptions;
    using global::Hypermedia.Client.Resolver;

    using Newtonsoft.Json;

    public static class ProblemJsonReader
    {
        public static bool TryReadProblemJson(HttpResponseMessage result, out ProblemDescription problemDescription)
        {
            problemDescription = null;
            if (result.Content == null)
            {
                return false;
            }

            try
            {
                var content = result.Content.ReadAsStringAsync().Result;
                problemDescription = JsonConvert.DeserializeObject<ProblemDescription>(content); // TODO inject deserializer
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
