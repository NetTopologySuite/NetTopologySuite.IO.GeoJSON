using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
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

                case JsonValue val:
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(val, serializerOptions);
                    Debug.Assert(jsonElement.ValueKind != JsonValueKind.Object && jsonElement.ValueKind != JsonValueKind.Array, "When this was developed, it wasn't possible to use JsonValue to model an Object or an Array.  This code will need to be updated.");
                    return JsonElementAttributesTable.ConvertValue(jsonElement);

                default:
                    throw new NotImplementedException($"This library was developed when the only public subclasses of JsonNode were JsonObject, JsonArray, and JsonValue.  Please open an issue to add support for {node.GetType().FullName}.");
            }
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

        /// <summary>
        /// Parses the current JSON token value from the source as a <see cref="double"/>. Rounds a value to the <see cref="PrecisionModel"/> grid.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="precisionModel">The precision model to round to.</param>
        /// <returns>The rounded value</returns>
        internal static double GetDouble(this Utf8JsonReader reader, PrecisionModel precisionModel)
            => precisionModel.MakePrecise(reader.GetDouble());

        /// <summary>
        /// Rounds a <see cref="double"/> value to the <see cref="PrecisionModel"/> grid. Writes the rounded value (as a JSON number) as an element of a JSON array.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="precisionModel">The precision model to round to.</param>
        internal static void WriteNumberValue(this Utf8JsonWriter writer, double value, PrecisionModel precisionModel)
            => writer.WriteNumberValue(precisionModel.MakePrecise(value));
    }
}
