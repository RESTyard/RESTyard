using System;
using System.Collections;
using System.Collections.Generic;

namespace Bluehands.Hypermedia.Client.Reader
{
    public interface IStringParser
    {
        IToken Parse(string contentString);
    }

    public interface IToken : IEnumerable<IToken>
    {
        string ValueAsString();

        IEnumerable<string> ChildrenAsStrings();

        object ToObject(Type type);

        IToken this[string key] { get; }
    }
}