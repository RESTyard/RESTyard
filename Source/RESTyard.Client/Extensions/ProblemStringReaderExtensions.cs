using FunicularSwitch;
using RESTyard.Client.Exceptions;
using RESTyard.Client.Reader;

namespace RESTyard.Client.Extensions;

public static class ProblemStringReaderExtensions
{
    public static Option<ProblemDetails> TryReadProblemString(this IProblemStringReader reader, string problemString)
        => reader.TryReadProblemString(problemString, out var details)
            ? Option.Some(details)
            : Option.None();
}