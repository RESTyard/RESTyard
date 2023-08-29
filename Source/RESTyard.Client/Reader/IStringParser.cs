using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RESTyard.Client.Reader
{
    public interface IStringParser
    {
        IToken? Parse(string contentString);

        Task<IToken?> ParseAsync(Stream contentStream);
    }

    public interface IToken : IEnumerable<IToken>
    {
        string? ValueAsString();

        IEnumerable<string> ChildrenAsStrings();

        object? ToObject(Type type);

        IToken? this[string key] { get; }

        string Serialize();
    }
}