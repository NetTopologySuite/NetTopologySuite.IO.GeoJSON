using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts FeatureCollection objects to its JSON representation.
    /// </summary>
    internal class StjFeatureCollectionConverter : JsonConverter<FeatureCollection>
    {
        private readonly bool _writeGeometryBBox;

        public StjFeatureCollectionConverter(bool writeGeometryBBox)
        {
            _writeGeometryBBox = writeGeometryBBox;
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="options">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override FeatureCollection Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            reader.AssertToken(JsonTokenType.StartObject);
            reader.ReadOrThrow();

            var fc = new FeatureCollection();
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals("type"))
                {
                    reader.ReadOrThrow();
                    reader.AssertToken(JsonTokenType.String);
                    if (!reader.ValueTextEquals(nameof(GeoJsonObjectType.FeatureCollection)))
                    {
                        throw new JsonException("must be FeatureCollection");
                    }

                    reader.ReadOrThrow();
                }
                else if (reader.ValueTextEquals("features"))
                {
                    reader.ReadOrThrow();
                    reader.AssertToken(JsonTokenType.StartArray);
                    reader.ReadOrThrow();
                    while (reader.TokenType != JsonTokenType.EndArray)
                    {
                        reader.AssertToken(JsonTokenType.StartObject);
                        fc.Add(JsonSerializer.Deserialize<IFeature>(ref reader, options));

                        reader.AssertToken(JsonTokenType.EndObject);
                        reader.ReadOrThrow();
                    }

                    reader.ReadOrThrow();
                }
                else
                {
                    reader.ReadOrThrow();
                    reader.Skip();
                    reader.ReadOrThrow();
                }
            }

            return fc;
        }

        public override void Write(Utf8JsonWriter writer, FeatureCollection value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("type", nameof(GeoJsonObjectType.FeatureCollection));

            if (_writeGeometryBBox)
                StjGeometryConverter.WriteBBox(writer, value.BoundingBox, options, null);

            writer.WriteStartArray("features");
            foreach (var feature in value)
                JsonSerializer.Serialize(writer, feature, options);
            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}
