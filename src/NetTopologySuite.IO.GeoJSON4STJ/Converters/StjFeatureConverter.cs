using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts Feature object to its JSON representation.
    /// </summary>
    internal sealed class StjFeatureConverter : JsonConverter<IFeature>
    {
        private readonly string _idPropertyName;
        private readonly bool _writeBBox;

        public StjFeatureConverter(string idPropertyName, bool writeBBox)
        {
            _idPropertyName = idPropertyName;
            _writeBBox = writeBBox;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="options">The calling serializer.</param>
        public override void Write(Utf8JsonWriter writer, IFeature value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            // type
            writer.WriteString("type", nameof(GeoJsonObjectType.Feature));

            // Add the id here if present.
            if (value.GetOptionalId(_idPropertyName) is object id)
            {
                writer.WritePropertyName("id");
                JsonSerializer.Serialize(writer, id, id.GetType(), options);
            }

            // bbox (optional)
            if (_writeBBox)
                StjGeometryConverter.WriteBBox(writer, value.BoundingBox, options, value.Geometry);

            // geometry
            if (value.Geometry != null || !options.IgnoreNullValues)
            {
                writer.WritePropertyName("geometry");
                JsonSerializer.Serialize(writer, value.Geometry, options);
            }

            // properties
            if (value.Attributes != null || ! options.IgnoreNullValues)
            {
                writer.WritePropertyName("properties");
                JsonSerializer.Serialize(writer, value.Attributes, options);
            }

            writer.WriteEndObject();
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
        public override IFeature Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                reader.Read();
                return null;
            }

            reader.ReadToken(JsonTokenType.StartObject);
            reader.SkipComments();

            // Create a new feature
            var feature = new StjFeature(_idPropertyName);
            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                // Read the property name
                string propertyName = reader.GetString();

                // Advance to value
                reader.Read();

                switch (propertyName)
                {
                    case "type":
                        if (reader.GetString() != "Feature")
                            throw new ArgumentException("Expected value 'Feature' not found.");
                        reader.Read();
                        break;

                    case "id":
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.Number when reader.TryGetDecimal(out decimal decimalValue):
                                feature.Id = decimalValue;
                                break;

                            case JsonTokenType.Number:
                                throw new NotSupportedException("Number value cannot be boxed as a decimal: " + reader.GetString());

                            case JsonTokenType.String:
                                feature.Id = reader.GetString();
                                break;

                            default:
                                throw new JsonException("A GeoJSON Feature's \"id\", if specified, must be either a JSON string or number, per RFC7946 section 3.2");
                        }

                        reader.Read();
                        break;

                    case "bbox":
                        var bbox = StjGeometryConverter.ReadBBox(ref reader, options);
                        feature.BoundingBox = bbox;
                        break;

                    case "geometry":
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            // #57: we're expected to read past the last token
                            reader.Read();
                        }
                        else
                        {
                            var geometry = JsonSerializer.Deserialize<Geometry>(ref reader, options);
                            feature.Geometry = geometry;
                            reader.ReadToken(JsonTokenType.EndObject);
                        }

                        break;

                    case "properties":
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            // #57: we're expected to read past the last token
                            reader.Read();
                        }
                        else
                        {
                            feature.Attributes = JsonSerializer.Deserialize<IAttributesTable>(ref reader, options);
                            reader.ReadToken(JsonTokenType.EndObject);
                        }

                        break;

                    default:
                        // If property name is not one of the above: skip it entirely (foreign member)
                        reader.Skip();
                        // Advance
                        while (reader.Read())
                        {
                            if (reader.TokenType == JsonTokenType.PropertyName || reader.TokenType == JsonTokenType.EndObject)
                                break;
                        }
                        break;
                }

                reader.SkipComments();
            }

            //reader.ReadToken(JsonTokenType.EndObject);
            return feature;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IFeature).IsAssignableFrom(objectType);
        }
    }
}
