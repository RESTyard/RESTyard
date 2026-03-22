using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Json.Schema;
using Json.Schema.Generation;
using Json.Schema.Generation.Generators;
using Json.Schema.Generation.Intents;

namespace RESTyard.Schema.SchemaGeneration
{
    // DataAnnotations like [Required] not handled. 'required' keyword works
    // required to support e.g. [Required] but this is an own class by the lib
    // using Json.Schema.Generation.DataAnnotations;
    // DataAnnotationsSupport.AddDataAnnotations();
    public class JsonSchemaFactory : IJsonSchemaFactory
    {
        private readonly SchemaGeneratorConfiguration config;
        private readonly ConcurrentDictionary<Type, JsonDocument> cache = new();

        public JsonSchemaFactory()
        {
            config = new SchemaGeneratorConfiguration()
            {
                Generators = {
#if NET6_0_OR_GREATER
                    new DateOnlyGenerator(),
                    new TimeOnlyGenerator(),
#endif
                    new DateTimeOffsetGenerator(),
                    new DateTimeGenerator(),
                    new TimeSpanGenerator(),
                },
            };


            AttributeHandler.AddHandler(new DisplayNameAttributeHandler());
            AttributeHandler.AddHandler(new DescriptionAttributeHandler());
        }

        public JsonDocument Generate(Type type)
        {
            return cache.GetOrAdd(type, GenerateCore);
        }

        private JsonDocument GenerateCore(Type type)
        {
            var schema = new JsonSchemaBuilder()
                .Schema(MetaSchemas.Draft202012Id)
                .FromType(type, config)
                .Build();
            return JsonSerializer.SerializeToDocument(schema);
        }

        public Json.Schema.JsonSchema GenerateSchema(Type type)
        {
            return Generate(type).RootElement.Deserialize<Json.Schema.JsonSchema>()
                   ?? throw new Exception($"Can not create schema from presented {nameof(JsonDocument)}");
        }
    }

    // see https://github.com/json-everything/json-everything/issues/143
    // we need to use the generic interface so it will be called
    internal sealed class DisplayNameAttributeHandler : IAttributeHandler<System.ComponentModel.DisplayNameAttribute> {
        public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
        {
            var att = (System.ComponentModel.DisplayNameAttribute)attribute;
             context.Intents.Insert(0, new TitleIntent(att.DisplayName));
        }
    }
    internal sealed class DescriptionAttributeHandler : IAttributeHandler<System.ComponentModel.DescriptionAttribute> {
        public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
        {
            var att = (System.ComponentModel.DescriptionAttribute)attribute;
            context.Intents.Insert(0, new DescriptionIntent(att.Description));
        }
    }

#if NET6_0_OR_GREATER
    public class DateOnlyGenerator : ISchemaGenerator
    {
        public bool Handles(Type type)
        {
            // Handle both DateOnly and DateOnly?
            return type == typeof(DateOnly) || type == typeof(DateOnly?);
        }

        public void AddConstraints(SchemaGenerationContextBase context)
        {
            context.Intents.Add(new TypeIntent(SchemaValueType.String));
            context.Intents.Add(new FormatIntent(Formats.Date));
        }
    }

    public class TimeOnlyGenerator : ISchemaGenerator
    {
        public bool Handles(Type type)
        {
            return type == typeof(TimeOnly) || type == typeof(TimeOnly?);
        }

        public void AddConstraints(SchemaGenerationContextBase context)
        {
            context.Intents.Add(new TypeIntent(SchemaValueType.String));
            context.Intents.Add(new FormatIntent(Formats.Time));
        }
    }
#endif

    public class DateTimeOffsetGenerator : ISchemaGenerator
    {
        public bool Handles(Type type)
        {
            return type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?);
        }

        public void AddConstraints(SchemaGenerationContextBase context)
        {
            context.Intents.Add(new TypeIntent(SchemaValueType.String));
            context.Intents.Add(new FormatIntent(Formats.DateTime));
        }
    }

    public class DateTimeGenerator : ISchemaGenerator
    {
        public bool Handles(Type type)
        {
            return type == typeof(DateTime) || type == typeof(DateTime?);
        }

        public void AddConstraints(SchemaGenerationContextBase context)
        {
            context.Intents.Add(new TypeIntent(SchemaValueType.String));
            context.Intents.Add(new FormatIntent(Formats.DateTime));
        }
    }

    public class TimeSpanGenerator : ISchemaGenerator
    {
        public bool Handles(Type type)
        {
            return type == typeof(TimeSpan) || type == typeof(TimeSpan?);
        }

        public void AddConstraints(SchemaGenerationContextBase context)
        {
            context.Intents.Add(new TypeIntent(SchemaValueType.String));
            context.Intents.Add(new PatternIntent(@"^-?(\d+\.)?\d{2}:\d{2}:\d{2}(\.\d{1,7})?$"));
        }
    }
}
