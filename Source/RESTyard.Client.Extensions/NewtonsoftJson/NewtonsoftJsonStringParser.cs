using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RESTyard.Client.Reader;

namespace RESTyard.Client.Extensions.NewtonsoftJson
{
    public class NewtonsoftJsonStringParser : IStringParser
    {
        public IToken? Parse(string contentString)
        {
            var jObject = JObject.Parse(contentString);
            return JTokenWrapper.Wrap(jObject);
        }

        public async Task<IToken?> ParseAsync(Stream contentStream)
        {
            using (var textReader = new StreamReader(contentStream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                var jObject = await JObject.LoadAsync(jsonReader);
                return JTokenWrapper.Wrap(jObject);
            }
        }

        private class JTokenWrapper : IToken
        {
            private readonly JToken jToken;

            private JTokenWrapper(JToken jToken)
            {
                this.jToken = jToken;
            }

            [return: NotNullIfNotNull(nameof(jToken))]
            public static IToken? Wrap(JToken? jToken)
            {
                return jToken != null ? new JTokenWrapper(jToken) : null;
            }

            public IEnumerator<IToken> GetEnumerator()
            {
                return this.jToken.Select(token => Wrap(token)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public string? ValueAsString()
            {
                return this.jToken.Value<string>();
            }

            public IEnumerable<string> ChildrenAsStrings()
            {
                return this.jToken
                    .Values<string>()
                    .OfType<string>();
            }

            public object? ToObject(Type type)
            {
                return this.jToken.ToObject(type);
            }

            public string Serialize()
            {
                return this.jToken.ToString(Formatting.None);
            }

            public IToken? this[string key] => Wrap(this.jToken[key]);
        }
    }
}