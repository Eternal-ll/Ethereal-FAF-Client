using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace beta.Infrastructure.Converters.JSON
{
    public class SingleOrArrayConverterFactory : JsonConverterFactory
    {
        public bool CanWrite { get; }

        public SingleOrArrayConverterFactory() : this(true) { }

        public SingleOrArrayConverterFactory(bool canWrite) => CanWrite = canWrite;

        public override bool CanConvert(Type typeToConvert)
        {
            var itemType = GetItemType(typeToConvert);
            if (itemType == null)
                return false;
            if (itemType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(itemType))
                return false;
            if (typeToConvert.GetConstructor(Type.EmptyTypes) == null || typeToConvert.IsValueType)
                return false;
            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var itemType = GetItemType(typeToConvert);
            var converterType = typeof(SingleOrArrayConverter<,>).MakeGenericType(typeToConvert, itemType);
            return (JsonConverter)Activator.CreateInstance(converterType, new object[] { CanWrite });
        }

        static Type GetItemType(Type type)
        {
            // Quick reject for performance
            if (type.IsPrimitive || type.IsArray || type == typeof(string))
                return null;
            while (type != null)
            {
                if (type.IsGenericType)
                {
                    var genType = type.GetGenericTypeDefinition();
                    if (genType == typeof(List<>))
                        return type.GetGenericArguments()[0];
                    // Add here other generic collection types as required, e.g. HashSet<> or ObservableCollection<> or etc.
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}
