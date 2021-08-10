using System;
using Bluehands.Hypermedia.Client.Exceptions;
using Bluehands.Hypermedia.Client.Reader;
using Newtonsoft.Json;

namespace Bluehands.Hypermedia.Client.Extensions.NewtonsoftJson
{
    public class NewtonsoftJsonProblemStringReader : IProblemStringReader
    {
        public bool TryReadProblemString(string problemString, out ProblemDescription problemDescription)
        {
            problemDescription = null;
            try
            {
                problemDescription = JsonConvert.DeserializeObject<ProblemDescription>(problemString);
            }
            catch (Exception)
            {
                return false;
            }
            return problemDescription != null;
        }
    }
}