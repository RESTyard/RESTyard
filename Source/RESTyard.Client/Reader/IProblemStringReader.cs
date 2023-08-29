using System;
using System.Diagnostics.CodeAnalysis;
using RESTyard.Client.Exceptions;

namespace RESTyard.Client.Reader
{
    public interface IProblemStringReader
    {
        bool TryReadProblemString(string problemString, [NotNullWhen(true)] out ProblemDetails? problemDescription);
    }
}