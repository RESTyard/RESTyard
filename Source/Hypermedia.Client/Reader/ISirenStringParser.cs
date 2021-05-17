using System;
using System.Collections;
using System.Collections.Generic;

namespace Bluehands.Hypermedia.Client.Reader
{
    public interface ISirenStringParser
    {
        IToken Parse(string contentString);
    }

    public interface IToken : IEnumerable<IToken>
    {
        string AsString();

        IEnumerable<string> AsStrings();

        object ToObject(Type type);

        IToken this[string key] { get; }
    }
}