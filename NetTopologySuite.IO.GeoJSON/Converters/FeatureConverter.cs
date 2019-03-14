using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GeoAPI.Geometries;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
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
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (serializer == null)
                throw new ArgumentNullException("serializer");

            var feature = value as IFeature;
            if (feature == null)
                return;

            writer.WriteStartObject();

            // type
            writer.WritePropertyName("type");
            writer.WriteValue("Feature");

            // Add the id here if present in attributes.
            // It will be skipped in serialization of properties
            if (feature.Attributes != null && feature.Attributes.Exists("id"))
            {
                var id = feature.Attributes["id"];
                writer.WritePropertyName("id");
                serializer.Serialize(writer, id);
            }

            // bbox (optional)
            if (serializer.NullValueHandling == NullValueHandling.Include || feature.BoundingBox != null)
            {
                writer.WritePropertyName("bbox");
                serializer.Serialize(writer, feature.BoundingBox, typeof(Envelope));
            }

            // geometry
            if (serializer.NullValueHandling == NullValueHandling.Include || feature.Geometry != null)
            {
                writer.WritePropertyName("geometry");
                serializer.Serialize(writer, feature.Geometry, typeof(IGeometry));
            }

            // properties
            if (serializer.NullValueHandling == NullValueHandling.Include || feature.Attributes != null)
            {
                writer.WritePropertyName("properties");
                serializer.Serialize(writer, feature.Attributes, typeof(IAttributesTable));
            }

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
                throw new JsonReaderException("Expected Start object '{' Token");

            bool read = reader.Read();
            Utility.SkipComments(reader);

            object featureId = null;
            Feature feature = new Feature();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string prop = (string)reader.Value;
                switch (prop)
                {
                    case "type":
                        read = reader.Read();
                        if ((string)reader.Value != "Feature")
                            throw new ArgumentException("Expected value 'Feature' not found.");
                        read = reader.Read();
                        break;
                    case "id":
                        read = reader.Read();
                        featureId = reader.Value;
                        if (feature.Attributes == null)
                            feature.Attributes = new AttributesTable(new[]
                                {new KeyValuePair<string, object>("id", featureId),});
                        else
                        {
                            if (feature.Attributes.Exists("id"))
                                feature.Attributes["id"] = featureId;
                            else
                                feature.Attributes.AddAttribute("id", featureId);
                        }
                        read = reader.Read();
                        break;
                    case "bbox":
                        Envelope bbox = serializer.Deserialize<Envelope>(reader);
                        feature.BoundingBox = bbox;
                        //Debug.WriteLine("BBOX: {0}", bbox.ToString());
                        break;
                    case "geometry":
                        read = reader.Read();
                        if (reader.TokenType == JsonToken.Null)
                        {
                            read = reader.Read();
                            break;
                        }

                        if (reader.TokenType != JsonToken.StartObject)
                            throw new ArgumentException("Expected token '{' not found.");
                        IGeometry geometry = serializer.Deserialize<IGeometry>(reader);
                        feature.Geometry = geometry;
                        if (reader.TokenType != JsonToken.EndObject)
                            throw new ArgumentException("Expected token '}' not found.");
                        read = reader.Read();
                        break;
                    case "properties":
                        read = reader.Read();
                        if (reader.TokenType != JsonToken.Null)
                        {
                            // #120: ensure "properties" isn't "null"
                            if (reader.TokenType != JsonToken.StartObject)
                                throw new ArgumentException("Expected token '{' not found.");
#if NETSTANDARD1_0 || NETSTANDARD1_3
                            var attributes = serializer.Deserialize<AttributesTable>(reader);
                            ((AttributesTable) feature.Attributes).MergeWith(attributes);
#else
                            var context = serializer.Context;
                            serializer.Context = new StreamingContext(serializer.Context.State, feature);
                            feature.Attributes = serializer.Deserialize<AttributesTable>(reader);
                            serializer.Context = context;
#endif
                            if (reader.TokenType != JsonToken.EndObject)
                                throw new ArgumentException("Expected token '}' not found.");
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

                Utility.SkipComments(reader);
            }

            if (read && reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");

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
