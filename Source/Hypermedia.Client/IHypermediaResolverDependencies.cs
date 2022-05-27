using Bluehands.Hypermedia.Client.ParameterSerializer;
using Bluehands.Hypermedia.Client.Reader;

namespace Bluehands.Hypermedia.Client
{
    public interface IHypermediaResolverDependencies
    {
        IHypermediaObjectRegister ObjectRegister { get; }

        IParameterSerializer ParameterSerializer { get; }

        IStringParser StringParser { get; }

        IProblemStringReader ProblemReader { get; }

        IHypermediaReader HypermediaReader { get; }
    }
}