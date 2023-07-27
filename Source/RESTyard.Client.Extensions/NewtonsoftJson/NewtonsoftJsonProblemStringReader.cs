using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using RESTyard.Client.Exceptions;
using RESTyard.Client.Reader;

namespace RESTyard.Client.Extensions.NewtonsoftJson
{
    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public class NewtonsoftJsonProblemStringReader : IProblemStringReader
    {
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public bool TryReadProblemString(string problemString, [NotNullWhen(true)] out ProblemDetails? problemDescription)
        {
            problemDescription = null;
            try
            {
                problemDescription = JsonConvert.DeserializeObject<ProblemDetails>(problemString);
            }
            catch (Exception)
            {
                return false;
            }
            return problemDescription != null;
        }
    }
}