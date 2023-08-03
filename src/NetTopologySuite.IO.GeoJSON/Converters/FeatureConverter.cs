using System;

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    using static AttributesTableExtensions;

    /// <summary>
    /// Converts Feature object to its JSON representation.
    /// </summary>
    public class FeatureConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (serializer is null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            if (!(value is IFeature feature))
            {
                return;
            }

            writer.WriteStartObject();

            // type
            writer.WritePropertyName("type");
            writer.WriteValue(nameof(GeoJsonObjectType.Feature));

            // Add the id here if present in attributes.
            // It will be skipped in serialization of properties
            object id = null;
            if (feature.Attributes?.TryGetId(out id) == true)
            {
                writer.WritePropertyName(IdPropertyName);
                serializer.Serialize(writer, id);
            }

            // bbox (optional)
            if (serializer.NullValueHandling == NullValueHandling.Include || !(feature.BoundingBox is null))
            {
                var bbox = feature.BoundingBox ?? feature.Geometry?.EnvelopeInternal;

                writer.WritePropertyName("bbox");
                serializer.Serialize(writer, bbox, typeof(Envelope));
            }

            // geometry
            writer.WritePropertyName("geometry");
            serializer.Serialize(writer, feature.Geometry, typeof(Geometry));

            // properties
            writer.WritePropertyName("properties");
            serializer.Serialize(writer, feature.Attributes, typeof(IAttributesTable));

            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonReaderException("Expected Start object '{' Token");
            }

            bool read = reader.Read();
            reader.SkipComments();

            var feature = new Feature();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                switch ((string)reader.Value)
                {
                    case "type":
                        reader.Read();
                        if ((string)reader.Value != "Feature")
                        {
                            throw new ArgumentException("Expected value 'Feature' not found.");
                        }

                        read = reader.Read();
                        break;

                    case "id":
                        reader.Read();
                        object featureId = reader.Value;
                        if (feature.Attributes is null)
                        {
                            feature.Attributes = new AttributesTable
                            {
                                { "id", featureId },
                            };
                        }
                        else if (feature.Attributes.Exists("id"))
                        {
                            feature.Attributes["id"] = featureId;
                        }
                        else
                        {
                            feature.Attributes.Add("id", featureId);
                        }
                        read = reader.Read();
                        break;

                    case "bbox":
                        var bbox = serializer.Deserialize<Envelope>(reader);
                        feature.BoundingBox = bbox;
                        //Debug.WriteLine("BBOX: {0}", bbox.ToString());
                        break;

                    case "geometry":
                        reader.Read();
                        if (reader.TokenType == JsonToken.Null)
                        {
                            read = reader.Read();
                            break;
                        }

                        if (reader.TokenType != JsonToken.StartObject)
                        {
                            throw new ArgumentException("Expected token '{' not found.");
                        }

                        feature.Geometry = serializer.Deserialize<Geometry>(reader);
                        if (reader.TokenType != JsonToken.EndObject)
                        {
                            throw new ArgumentException("Expected token '}' not found.");
                        }

                        read = reader.Read();
                        break;

                    case "properties":
                        reader.Read();
                        if (reader.TokenType != JsonToken.Null)
                        {
                            // #120: ensure "properties" isn't "null"
                            if (reader.TokenType != JsonToken.StartObject)
                            {
                                throw new ArgumentException("Expected token '{' not found.");
                            }

                            var attributes = serializer.Deserialize<AttributesTable>(reader);

                            if (feature.Attributes is null)
                            {
                                feature.Attributes = attributes;
                            }
                            else
                            {
                                foreach (var attribute in attributes)
                                {
                                    if (!feature.Attributes.Exists(attribute.Key))
                                    {
                                        feature.Attributes.Add(attribute.Key, attribute.Value);
                                    }
                                }
                            }

                            if (reader.TokenType != JsonToken.EndObject)
                            {
                                throw new ArgumentException("Expected token '}' not found.");
                            }
                        }
                        read = reader.Read();
                        break;

                    default:
                        read = reader.Read(); // move next
                        // jump to next property
                        if (read)
                        {
                            reader.Skip();
                        }
                        read = reader.Read();
                        break;
                }

                reader.SkipComments();
            }

            if (read && reader.TokenType != JsonToken.EndObject)
            {
                throw new ArgumentException("Expected token '}' not found.");
            }

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
