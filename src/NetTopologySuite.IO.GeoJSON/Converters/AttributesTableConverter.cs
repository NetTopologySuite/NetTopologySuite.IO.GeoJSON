using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Features;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    using static AttributesTableExtensions;

    /// <summary>
    /// Converts IAttributesTable object to its JSON representation.
    /// </summary>
    public class AttributesTableConverter : JsonConverter
    {
        /// <summary>
        /// Gets or sets a value indicating that a feature's id property should be written to the properties block as well
        /// </summary>
        public static bool WriteIdToProperties { get; set; } = false;

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

            if (!(value is IAttributesTable attributes))
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            string[] names = attributes.GetNames();
            foreach (string name in names)
            {
                // skip id
                if (name == IdPropertyName && !WriteIdToProperties)
                {
                    continue;
                }

                writer.WritePropertyName(name);
                object val = attributes[name];
                serializer.Serialize(writer, val);
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
            return InternalReadJson(reader, serializer, false);
        }

        private static IList<object> InternalReadJsonArray(JsonReader reader, JsonSerializer serializer)
        {
            // We need to have a start array token!
            Debug.Assert(reader.TokenType == JsonToken.StartArray);

            // advance
            reader.Read();
            reader.SkipComments();

            // create result object
            var res = new List<object>();

            while (reader.TokenType != JsonToken.EndArray)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:
                        res.Add(InternalReadJson(reader, serializer, true));
                        Debug.Assert(reader.TokenType == JsonToken.EndObject);
                        // advance
                        reader.Read();
                        break;

                    case JsonToken.StartArray:
                        // add new array to result
                        res.Add(InternalReadJsonArray(reader, serializer));
                        break;

                    case JsonToken.Comment:
                        break;

                    case JsonToken.EndConstructor:
                    case JsonToken.EndObject:
                    case JsonToken.PropertyName:
                        throw new JsonException("Expected token ']' or '[' token, or a value");

                    default:
                        // add value to list
                        res.Add(reader.Value);
                        // advance
                        reader.Read();
                        break;
                }
                reader.SkipComments();
            }

            // Read past end array
            reader.Read();

            return res;
        }

        private static object InternalReadJson(JsonReader reader, JsonSerializer serializer,
            bool innerObject)
        {
            //// TODO: refactor to remove check when reading TopoJSON
            //if (reader.TokenType == JsonToken.StartArray)
            //{
            //    reader.Read(); // move to first item
            //    IList<object> array = new List<object>();
            //    do
            //    {
            //        if (reader.TokenType == JsonToken.EndArray) break;
            //        object inner = InternalReadJson(reader, serializer);
            //        array.Add(inner);
            //        reader.Read(); // move to next item
            //    } while (true);
            //    return array;
            //}

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new ArgumentException("Expected token '{' not found.");
            }

            // Advance reader
            reader.Read();
            reader.SkipComments();
            var attributesTable = new AttributesTable();

            if (reader.TokenType != JsonToken.Null)
            {
                while (reader.TokenType == JsonToken.PropertyName)
                {
                    string attributeName = (string)reader.Value;
                    reader.Read();
                    object attributeValue;
                    if (reader.TokenType == JsonToken.StartObject)
                    {
                        if (serializer.TypeNameHandling != TypeNameHandling.Objects)
                        {
                            // inner object to AttributeTable
                            attributeValue = InternalReadJson(reader, serializer, true);
                            if (reader.TokenType != JsonToken.EndObject)
                            {
                                throw new ArgumentException("Expected token '}' not found.");
                            }

                            // read EndObject token
                            reader.Read();
                        }
                        else
                        {
                            // deserialize the inner object
                            attributeValue = serializer.Deserialize(reader);
                        }
                    }
                    else if (reader.TokenType == JsonToken.StartArray)
                    {
                        attributeValue = InternalReadJsonArray(reader, serializer);
                        //reader.Read(); // move to first item
                        //IList<object> array = new List<object>();
                        //do
                        //{
                        //    object inner = InternalReadJson(reader, serializer);
                        //    array.Add(inner);
                        //    reader.Read(); // move to next item
                        //} while (reader.TokenType != JsonToken.EndArray);
                        //attributeValue = array;
                    }
                    else
                    {
                        attributeValue = reader.Value;
                        reader.Read();
                    }

                    if (!attributesTable.Exists(attributeName))
                    {
                        attributesTable.Add(attributeName, attributeValue);
                    }
                }

                reader.SkipComments();
            }

            // TODO: refactor to remove check when reading TopoJSON
            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new ArgumentException("Expected token '}' not found.");
            }

            return attributesTable;
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
            return typeof(IAttributesTable).IsAssignableFrom(objectType);
        }
    }
}
