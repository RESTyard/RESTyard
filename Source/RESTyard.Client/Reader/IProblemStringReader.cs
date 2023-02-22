using System;
using RESTyard.Client.Exceptions;

namespace RESTyard.Client.Reader
{
    public interface IProblemStringReader
    {
        bool TryReadProblemString(string problemString, out ProblemDetails problemDescription);
    }
}