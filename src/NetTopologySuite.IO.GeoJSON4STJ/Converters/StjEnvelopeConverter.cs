using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts <see cref="Envelope"/>s to and from JSON
    /// </summary>
    [Obsolete("Not needed, because bbox is not an object in GeoJSON")]
    public class StjEnvelopeConverter : JsonConverter<Envelope>
    {
        /// <summary>
        /// Writes an <see cref="Envelope"/> to JSON
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The envelope</param>
        /// <param name="options">The serializer options</param>
        public override void Write(Utf8JsonWriter writer, Envelope value, JsonSerializerOptions options)
        {
            writer.WritePropertyName("bbox");

            if (!(value is Envelope envelope))
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            writer.WriteNumberValue(envelope.MinX);
            writer.WriteNumberValue(envelope.MinY);
            writer.WriteNumberValue(envelope.MaxX);
            writer.WriteNumberValue(envelope.MaxY);
            writer.WriteEndArray();
        }

        /// <summary>
        /// Reads an <see cref="Envelope"/> from JSON
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="typeToConvert">The object type</param>
        /// <param name="options">The serializer options</param>
        /// <returns></returns>
        public override Envelope Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Envelope res = null;

            Debug.Assert(reader.TokenType == JsonTokenType.PropertyName);
            Debug.Assert(reader.ValueTextEquals("bbox"));

            reader.Read(); // move to array start

            if (reader.TokenType != JsonTokenType.Null)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException(string.Format(Resources.EX_StartArrayTokenExpected, reader.ValueSpan.ToString()));
                reader.Read();

                double minX = reader.GetDouble();
                reader.Read();
                double minY = reader.GetDouble();
                reader.Read();
                double maxX = reader.GetDouble();
                reader.Read();
                double maxY = reader.GetDouble();
                reader.Read();

                if (reader.TokenType != JsonTokenType.EndArray)
                    throw new JsonException(string.Format(Resources.EX_StartArrayTokenExpected, reader.ValueSpan.ToString()));

                res = new Envelope(minX, maxX, minY, maxY);
            }

            reader.Read(); // move away from array end
            return res;
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Envelope);
        }
    }
}
