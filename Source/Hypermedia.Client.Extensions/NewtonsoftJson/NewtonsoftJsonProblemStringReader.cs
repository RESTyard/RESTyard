using System;
using Newtonsoft.Json;
using RESTyard.Client.Exceptions;
using RESTyard.Client.Reader;

namespace RESTyard.Client.Extensions.NewtonsoftJson
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