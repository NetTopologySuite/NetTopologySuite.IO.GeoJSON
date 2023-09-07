using System;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts FeatureCollection object to its JSON representation.
    /// </summary>
    public class FeatureCollectionConverter : JsonConverter
    {
        /// <summary>
        /// Writes a feature collection in its JSON representation
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The value</param>
        /// <param name="serializer">The serializer</param>
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

            if (!(value is FeatureCollection coll))
            {
                if (serializer.NullValueHandling == NullValueHandling.Ignore)
                {
                    writer.WriteToken(null);
                }
                return;
            }

            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue(nameof(GeoJsonObjectType.FeatureCollection));

            writer.WritePropertyName("features");
            var array = new IFeature[coll.Count];
            coll.CopyTo(array, 0);
            serializer.Serialize(writer, array);

            var bbox = coll.BoundingBox;
            if (serializer.NullValueHandling == NullValueHandling.Include || bbox != null)
            {
                writer.WritePropertyName("bbox");
                serializer.Serialize(writer, bbox, typeof(Envelope));
            }

            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads a feature collection from its JSON representation
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="objectType">The object type</param>
        /// <param name="existingValue">The existing value</param>
        /// <param name="serializer">The serializer</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            bool read = reader.Read();
            var fc = new FeatureCollection();
            while (reader.TokenType == JsonToken.PropertyName)
            {
                string val = (string)reader.Value;
                switch (val)
                {
                    case "features":
                        // move to begin of array
                        /*read = */
                        reader.Read();
                        if (reader.TokenType != JsonToken.StartArray)
                        {
                            throw new JsonReaderException("Expected token '[' not found");
                        }

                        // move to first feature
                        read = reader.Read();
                        while (read && reader.TokenType != JsonToken.EndArray)
                        {
                            fc.Add(serializer.Deserialize<Feature>(reader));
                            read = reader.Read();
                        }
                        read = reader.Read();
                        break;

                    case "type":
                        /*read = */
                        reader.Read();
                        if (reader.TokenType != JsonToken.String && (string)reader.Value != "FeatureCollection")
                        {
                            throw new ParseException("Expected value 'FeatureCollection' not found");
                        }

                        read = reader.Read();
                        break;

                    case "bbox":
                        fc.BoundingBox = serializer.Deserialize<Envelope>(reader);
                        /*
                        read = reader.Read();
                        if (reader.TokenType != JsonToken.StartArray)
                            throw new JsonReaderException("Expected token '{' not found");

                        var env = serializer.Deserialize<double[]>(reader);
                        fc.BoundingBox = new Envelope(env[0], env[2], env[1], env[3]);

                        if (reader.TokenType != JsonToken.EndArray)
                            throw new JsonReaderException("Expected token '}' not found");

                        read = reader.Read();
                        */
                        break;

                    default:
                        // additional members are ignored: see https://code.google.com/p/nettopologysuite/issues/detail?id=186
                        /*
                         * see also: http://gis.stackexchange.com/a/25309/463
                         * "you can have a properties element at the top level of a feature collection,
                         * but don't expect any tools to know its there"
                         */
                        read = reader.Read(); // move next
                        // jump to next property
                        while (read && reader.TokenType != JsonToken.PropertyName)
                        {
                            read = reader.Read();
                        }

                        break;
                }
            }

            if (read && reader.TokenType != JsonToken.EndObject)
            {
                throw new JsonReaderException("Expected token '}' not found");
            }

            return fc;
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(FeatureCollection).IsAssignableFrom(objectType);
        }
    }
}
