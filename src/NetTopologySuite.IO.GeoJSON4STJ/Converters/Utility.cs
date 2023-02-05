using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    internal static class Utility
    {
        internal static bool ShouldWriteNullValues(this JsonSerializerOptions options)
        {
#pragma warning disable SYSLIB0020
            return options.DefaultIgnoreCondition == JsonIgnoreCondition.Never && !options.IgnoreNullValues;
#pragma warning restore SYSLIB0020
        }

        internal static void SkipComments(this ref Utf8JsonReader reader)
        {
            // Skip comments
            while (reader.TokenType == JsonTokenType.Comment)
            {
                if (!reader.Read())
                {
                    break;
                }
            }
        }

        internal static bool ReadToken(this ref Utf8JsonReader reader, JsonTokenType tokenType, bool throwException = true)
        {
            if (reader.TokenType != tokenType)
            {
                if (throwException)
                    ThrowForUnexpectedToken(tokenType, ref reader);
                return false;
            }
            return reader.Read();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReadOrThrow(this ref Utf8JsonReader reader)
        {
            if (!reader.Read())
            {
                ThrowForUnexpectedEndOfStream();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AssertToken(this ref Utf8JsonReader reader, JsonTokenType requiredCurrentTokenType)
        {
            if (reader.TokenType != requiredCurrentTokenType)
            {
                ThrowForUnexpectedToken(requiredCurrentTokenType, ref reader);
            }
        }

        internal static object ObjectFromJsonNode(JsonNode node, JsonSerializerOptions serializerOptions)
        {
            switch (node)
            {
                case null:
                    return null;

                case JsonObject obj:
                    return new JsonObjectAttributesTable(obj, serializerOptions);

                case JsonArray arr:
                    return new JsonArrayInAttributesTableWrapper(arr, serializerOptions);
            }

            // else it's a JsonValue... not sure of a cleaner way to handle this than to just reuse
            // the code we have that deals with JsonElement.
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(node, serializerOptions);
            object result = JsonElementAttributesTable.ConvertValue(jsonElement);
            if (result is JsonElementAttributesTable elementTable)
            {
                result = new JsonObjectAttributesTable(JsonObject.Create(elementTable.RootElement.Clone(), node.Options), serializerOptions);
            }

            return result;
        }

        internal static JsonNode ObjectToJsonNode(object obj, JsonSerializerOptions serializerOptions)
        {
            return JsonSerializer.SerializeToNode(obj, obj?.GetType() ?? typeof(object), serializerOptions);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowForUnexpectedEndOfStream()
            => throw new JsonException(Resources.EX_UnexpectedEndOfStream);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowForUnexpectedToken(JsonTokenType requiredNextTokenType, ref Utf8JsonReader reader)
            => throw new JsonException(string.Format(Resources.EX_UnexpectedToken, requiredNextTokenType, reader.TokenType, reader.GetString()));
    }
}
