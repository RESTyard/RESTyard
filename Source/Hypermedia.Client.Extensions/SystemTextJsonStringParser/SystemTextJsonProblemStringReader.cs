using System;
using System.Text.Json;
using Bluehands.Hypermedia.Client.Exceptions;
using Bluehands.Hypermedia.Client.Reader;

namespace Bluehands.Hypermedia.Client.Extensions.SystemTextJsonStringParser
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