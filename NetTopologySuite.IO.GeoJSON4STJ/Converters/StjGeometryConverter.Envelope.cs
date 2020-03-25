using System;
using System.Text.Json;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    internal partial class StjGeometryConverter
    {
        /// <summary>
        /// Gets or sets a value indicating if the bounding box should be written for geometries
        /// </summary>
        public bool WriteGeometryBBox { get; set; }

        internal static Envelope ReadBBox(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Envelope res = null;

            if (reader.TokenType != JsonTokenType.Null)
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

        internal static void WriteBBox(Utf8JsonWriter writer, Envelope value, JsonSerializerOptions options, IGeometry geometry)
        {
            // if we don't want to write "null" bounding boxes, bail out.
            if ((value == null || value.IsNull) && options.IgnoreNullValues)
                return;

            // Don't clutter export with bounding box if geometry is a point!
            if (geometry is Point)
                return;

            // if value == null, try to get it from geometry
            if (value == null)
                value = geometry?.EnvelopeInternal ?? new Envelope();

            writer.WritePropertyName("bbox");
            if (value.IsNull)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            writer.WriteNumberValue(value.MinX);
            writer.WriteNumberValue(value.MinY);
            writer.WriteNumberValue(value.MaxX);
            writer.WriteNumberValue(value.MaxY);
            writer.WriteEndArray();
        }
    }
}
