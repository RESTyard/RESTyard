using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Bluehands.Hypermedia.Client.Reader
{
    public class NewtonsoftJsonStringParser : IStringParser
    {
        public IToken Parse(string contentString)
        {
            var jObject = JObject.Parse(contentString);
            return JTokenWrapper.Wrap(jObject);
        }

        private class JTokenWrapper : IToken
        {
            private readonly JToken jToken;

            private JTokenWrapper(JToken jToken)
            {
                this.jToken = jToken;
            }

            public static IToken Wrap(JToken jToken)
            {
                return jToken != null ? new JTokenWrapper(jToken) : null;
            }

            public IEnumerator<IToken> GetEnumerator()
            {
                return this.jToken.Select(Wrap).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public string ValueAsString()
            {
                return this.jToken.Value<string>();
            }

            public IEnumerable<string> ChildrenAsStrings()
            {
                return this.jToken.Values<string>();
            }

            public object ToObject(Type type)
            {
                return this.jToken.ToObject(type);
            }

            public IToken this[string key] => Wrap(this.jToken[key]);
        }
    }
}