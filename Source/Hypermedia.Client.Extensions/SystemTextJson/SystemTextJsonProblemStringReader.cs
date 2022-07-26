using System;
using System.Text.Json;
using RESTyard.Client.Exceptions;
using RESTyard.Client.Reader;

namespace Bluehands.Hypermedia.Client.Extensions.SystemTextJson
{
    public class SystemTextJsonProblemStringReader : IProblemStringReader
    {
        public bool TryReadProblemString(string problemString, out ProblemDescription problemDescription)
        {
            problemDescription = null;
            try
            {
                problemDescription = JsonSerializer.Deserialize<ProblemDescription>(problemString);
            }
            catch (Exception)
            {
                return false;
            }

            return problemDescription != null;
        }
    }
}