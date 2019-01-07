using System;
using System.Net.Http;
using Bluehands.Hypermedia.Client.Exceptions;
using Newtonsoft.Json;

namespace Bluehands.Hypermedia.Client.Reader.ProblemJson
{
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
            return problemDescription != null;
        }
    }
}
