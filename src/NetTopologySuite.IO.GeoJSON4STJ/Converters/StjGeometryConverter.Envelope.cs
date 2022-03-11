using System;
using System.Text.Json;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    internal partial class StjGeometryConverter
    {
        /// <summary>
        /// Gets or sets a value indicating if the bounding box should be written for geometries
        /// </summary>
        /// <remarks>Property will be removed in future versions.</remarks>
        [Obsolete("Property will be removed in future versions")]
        public bool WriteGeometryBBox
        {
            get { return _writeGeometryBBox; }
            set { _writeGeometryBBox = value; }
        }

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
        /// <param name="value">The "main" envelope, used if valid (i.e.: is not null).</param>
        /// <param name="options">The serialization options.</param>
        /// <param name="geometryEnvelope">
        /// The "backup" envelope, used only if the <paramref name="value"/> is not valid (i.e.: is not null).
        /// </param>
        /// <remarks>
        /// For a <see cref="Geometry"/>, both the <paramref name="value"/>
        /// and the <paramref name="geometryEnvelope"/> are the <see cref="IFeature.Geometry"/> envelope.
        /// For a <see cref="IFeature"/>, the <paramref name="value"/> is the <see cref="IFeature.BoundingBox"/>
        /// and the <paramref name="geometryEnvelope"/> is the <see cref="IFeature.Geometry"/> envelope.
        /// For a <see cref="FeatureCollection"/>, the <paramref name="value"/> is the <see cref="IFeature.BoundingBox"/>
        /// and the <paramref name="geometryEnvelope"/> is the expanded envelope of all
        /// the <see cref="IFeature.Geometry"/> envelopes that compose the feature collection.
        /// </remarks>
        internal static void WriteBBox(Utf8JsonWriter writer, Envelope value, JsonSerializerOptions options,
            Envelope geometryEnvelope)
        {
            // if we don't want to write "null" bounding boxes, bail out.
            if ((value?.IsNull != false) && options.IgnoreNullValues)
                return;
            if ((geometryEnvelope?.IsNull != false) && options.IgnoreNullValues)
                return;

            // if value == null, try to get it from geometry
            if (value == null)
                value = geometryEnvelope ?? new Envelope();

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
