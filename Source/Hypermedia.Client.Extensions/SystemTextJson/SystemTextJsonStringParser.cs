using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RESTyard.Client.Reader;

namespace RESTyard.Client.Extensions.SystemTextJson
{
    public class SystemTextJsonStringParser : IStringParser
    {
        public IToken Parse(string contentString)
        {
            var document = JsonDocument.Parse(contentString);
            return JsonElementWrapper.Wrap(document.RootElement);
        }

        public async Task<IToken> ParseAsync(Stream contentStream)
        {
            var document = await JsonDocument.ParseAsync(contentStream);
            return JsonElementWrapper.Wrap(document.RootElement);
        }

        private class JsonElementWrapper : IToken
        {
            private readonly JsonElement element;

            private JsonElementWrapper(JsonElement element)
            {
                this.element = element;
            }

            public static IToken Wrap(JsonElement element)
            {
                return new JsonElementWrapper(element);
            }

            public IEnumerator<IToken> GetEnumerator()
            {
                return this.element.EnumerateArray().Select(Wrap).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public string ValueAsString()
            {
                return this.element.GetString();
            }

            public IEnumerable<string> ChildrenAsStrings()
            {
                return this.element.EnumerateArray().Select(x => x.GetString());
            }

            public object ToObject(Type type)
            {
                var json = this.element.GetRawText();
                return JsonSerializer.Deserialize(json, type);
            }

            public IToken this[string key] => this.element.TryGetProperty(key, out var jsonElement) ? Wrap(jsonElement) : null;

            public string Serialize()
            {
                if (this.element.ValueKind == JsonValueKind.Undefined)
                {
                    return string.Empty;
                }

                return JsonSerializer.Serialize(
                    this.element,
                    new JsonSerializerOptions()
                    {
                        AllowTrailingCommas = false,
                        IgnoreNullValues = true,
                        WriteIndented = false,
                    });
            }
        }
    }
}