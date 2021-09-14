using Bluehands.Hypermedia.Client.Exceptions;

namespace Bluehands.Hypermedia.Client.Reader
{
    public interface IProblemStringReader
    {
        bool TryReadProblemString(string problemString, out ProblemDescription problemDescription);
    }
}