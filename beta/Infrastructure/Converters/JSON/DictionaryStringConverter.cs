using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace beta.Infrastructure.Converters.JSON
{
    public class DictionaryStringConverter : JsonConverter<Dictionary<string, string>>
    {
        public override Dictionary<string, string> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var dictionary = new Dictionary<string, string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                // Add to dictionary.
                dictionary.Add(propertyName, Encoding.UTF8.GetString(reader.ValueSpan));
            }

            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            Dictionary<string, string> dictionary,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
