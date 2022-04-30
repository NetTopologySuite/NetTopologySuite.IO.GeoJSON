using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    internal partial class StjGeometryConverter
    {
        internal static Envelope ReadBBox(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Envelope res = null;

            if (reader.TokenType == JsonTokenType.Null)
            {
                // #57: callers expect us to have read past the last token
                reader.Read();
            }
            else
            {
                reader.ReadToken(JsonTokenType.StartArray);

                double minX = reader.GetDouble();
                reader.Read();
                double minY = reader.GetDouble();
                reader.Read();
                double maxX = reader.GetDouble();
                reader.Read();
                double maxY = reader.GetDouble();
                reader.Read();

                if (reader.TokenType == JsonTokenType.Number)
                {
                    maxX = maxY;
                    maxY = reader.GetDouble();
                    reader.Read();
                    reader.Read();
                }

                reader.ReadToken(JsonTokenType.EndArray);

                res = new Envelope(minX, maxX, minY, maxY);
            }

            //reader.Read(); // move away from array end
            return res;
        }

        /// <summary>
        /// Writes the BBOX to the given <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The envelope.</param>
        /// <param name="options">The serialization options.</param>
        internal static void WriteBBox(Utf8JsonWriter writer, Envelope value, JsonSerializerOptions options)
        {
            // if we don't want to write "null" bounding boxes, bail out.
            if (value?.IsNull != false)
            {
                if (options.ShouldWriteNullValues())
                {
                    writer.WriteNull("bbox");
                }

                return;
            }

            writer.WritePropertyName("bbox");

            writer.WriteStartArray();
            writer.WriteNumberValue(value.MinX);
            writer.WriteNumberValue(value.MinY);
            writer.WriteNumberValue(value.MaxX);
            writer.WriteNumberValue(value.MaxY);
            writer.WriteEndArray();
        }
    }
}
