using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Converters
{
    using static AttributesTableExtensions;

    /// <summary>
    /// Converts IAttributesTable object to its JSON representation.
    /// </summary>
    public class StjAttributesTableConverter : JsonConverter<IAttributesTable>
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
        /// <param name="options">The calling serializer.</param>
        public override void Write(Utf8JsonWriter writer, IAttributesTable value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var c = options.GetConverter(typeof(IAttributesTable));
            if (c == null) options.Converters.Add(new StjAttributesTableConverter());

            writer.WriteStartObject();
            string[] names = value.GetNames();
            foreach (string propertyName in names)
            {
                // skip id
                if (propertyName == IdPropertyName && !WriteIdToProperties)
                    continue;

                object val = value[propertyName];
                if (val == null)
                {
                    if (!options.IgnoreNullValues)
                        writer.WriteNull(propertyName);
                    return;
                }
                var type = value.GetType(propertyName);
                writer.WritePropertyName(propertyName);

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        writer.WriteBooleanValue((bool) val);
                        break;
                    case TypeCode.Int32:
                        writer.WriteNumberValue((int) val);
                        break;
                    case TypeCode.Int64:
                        writer.WriteNumberValue((long) val);
                        break;
                    case TypeCode.Single:
                        writer.WriteNumberValue((float) val);
                        break;
                    case TypeCode.Double:
                        writer.WriteNumberValue((double) val);
                        break;
                    case TypeCode.Decimal:
                        writer.WriteNumberValue((decimal) val);
                        break;
                    case TypeCode.DateTime:
                        writer.WriteStringValue((DateTime) val);
                        break;
                    case TypeCode.String:
                        writer.WriteStringValue((string) val);
                        break;
                    case TypeCode.Object:
                        if (type == typeof(Guid))
                            writer.WriteStringValue((Guid) val);
                        else
                        {
                            var converterType = typeof(IAttributesTable).IsAssignableFrom(type)
                                ? typeof(IAttributesTable)
                                : type; 
                            var useConverter = options.GetConverter(converterType);
                            if (useConverter == null)
                                throw new JsonException();

                            var util = JsonConverterUtility.GetConverterUtility(converterType);
                            util.Write(useConverter, writer, val, options);
                        }
                        break;
                }
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
        public override IAttributesTable Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
        {
            return Read(ref reader, objectType, null, options);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="forFeature">The feature for this attribute table</param>
        /// <param name="options">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public IAttributesTable Read(ref Utf8JsonReader reader, Type objectType,
            IFeature forFeature, JsonSerializerOptions options)
        {
            // Get or create the return value
            var attributesTable = forFeature?.Attributes ?? new AttributesTable();
            if (reader.TokenType == JsonTokenType.Null)
            {
                reader.Read();
                return attributesTable;
            }

            // Advance reader, skip comments
            reader.ReadToken(JsonTokenType.StartObject);
            reader.SkipComments();

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                string attributeName = (string) reader.GetString();
                reader.Read();

                bool exists = attributesTable.Exists(attributeName);
                var attributeType = exists ? attributesTable.GetType(attributeName) : typeof(object);
                var attributeTypeCode = Type.GetTypeCode(attributeType);

                object attributeValue = null;
                switch (reader.TokenType)
                {
                    case JsonTokenType.Number:
                        if (attributeTypeCode == TypeCode.Object)
                        {
                            if (reader.TryGetInt32(out int i4))
                                attributeValue = i4;
                            else if (reader.TryGetInt64(out long i8))
                                attributeValue = i8;
                            else if (reader.TryGetDouble(out double r8))
                                attributeValue = r8;
                            else
                                throw new JsonException();
                        }
                        else
                        {
                            switch (attributeTypeCode)
                            {
                                case TypeCode.Byte:
                                    attributeValue = reader.GetByte();
                                    break;
                                case TypeCode.SByte:
                                    attributeValue = reader.GetSByte();
                                    break;
                                case TypeCode.Int16:
                                    attributeValue = reader.GetInt16();
                                    break;
                                case TypeCode.UInt16:
                                    attributeValue = reader.GetUInt16();
                                    break;
                                case TypeCode.Int32:
                                    attributeValue = reader.GetInt32();
                                    break;
                                case TypeCode.UInt32:
                                    attributeValue = reader.GetUInt32();
                                    break;
                                case TypeCode.Int64:
                                    attributeValue = reader.GetInt64();
                                    break;
                                case TypeCode.UInt64:
                                    attributeValue = reader.GetUInt64();
                                    break;
                                case TypeCode.Single:
                                    attributeValue = reader.GetSingle();
                                    break;
                                case TypeCode.Double:
                                    attributeValue = reader.GetDouble();
                                    break;
                            }
                        }
                        break;
                    case JsonTokenType.String:
                        if (attributeTypeCode == TypeCode.Object)
                        {
                            if (reader.TryGetGuid(out var g))
                                attributeValue = g;
                            else if (reader.TryGetDateTime(out var dt))
                                attributeValue = dt;
                            else if (reader.TryGetDateTimeOffset(out var dto))
                                attributeValue = dto;
                            else
                                attributeValue = reader.GetString();
                        }
                        else
                        {
                            switch (attributeTypeCode)
                            {
                                case TypeCode.DateTime:
                                    attributeValue = reader.GetDateTime();
                                    break;
                                default:
                                    attributeValue = reader.GetString();
                                    break;
                            }
                        }
                        break;

                    case JsonTokenType.StartArray:
                        attributeValue = InternalReadJsonArray(ref reader, options);
                        break;

                    case JsonTokenType.Null:
                        attributeValue = null;
                        break;

                    // This is going to be another attribute table
                    case JsonTokenType.StartObject:
                        attributeType = typeof(IAttributesTable);
                        var jcu = JsonConverterUtility.GetConverterUtility(attributeType);
                        //var jc = options.GetConverter(attributeType);
                        attributeValue = jcu.Read(this, ref reader, attributeType, options);

                        break;
                }

                if (exists)
                    attributesTable[attributeName] = attributeValue;
                else
                    attributesTable.Add(attributeName, attributeValue);

                reader.Read();
                reader.SkipComments();
            }

            reader.ReadToken(JsonTokenType.EndObject);

            return attributesTable;
        }

        private static IList<object> InternalReadJsonArray(ref Utf8JsonReader reader, JsonSerializerOptions serializer)
        {
            // We need to have a start array token!
            reader.ReadToken(JsonTokenType.StartArray);
            reader.SkipComments();

            // create result object
            var res = new List<object>();

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                        var vs = reader.ValueSequence;
                        var spStart = reader.Position;
                        reader.Skip();
                        var spEnd = reader.Position;
                        res.Add(JsonSerializer.Deserialize(vs.Slice(spStart, spEnd).ToArray(), typeof(AttributesTable)));
                        break;

                    case JsonTokenType.StartArray:
                        // add new array to result
                        res.Add(InternalReadJsonArray(ref reader, serializer));
                        break;

                    case JsonTokenType.Comment:
                        break;

                    case JsonTokenType.EndObject:
                    case JsonTokenType.PropertyName:
                        throw new JsonException("Expected ']' token , '[' token or a value");

                    default:
                        // add value to list
                        res.Add(JsonSerializer.Deserialize(ref reader, typeof(object)));
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

    internal abstract class JsonConverterUtility
    {
        private static readonly Dictionary<Type, JsonConverterUtility> Converters = new Dictionary<Type, JsonConverterUtility>();

        public static JsonConverterUtility GetConverterUtility(Type type)
        {
            if (!Converters.TryGetValue(type, out var jcuInstance))
            {
                var jcuType = typeof(JsonConverterUtility<>).MakeGenericType(type);
                jcuInstance = (JsonConverterUtility) Activator.CreateInstance(jcuType);
                Converters[jcuType] = jcuInstance;
            }

            return jcuInstance;
        }


        public abstract object Read(JsonConverter c, ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
        public abstract void Write(JsonConverter c, Utf8JsonWriter writer, object value, JsonSerializerOptions options);
    }

    internal class JsonConverterUtility<T> : JsonConverterUtility
    {
        public override object Read(JsonConverter c, ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Read((JsonConverter<T>) c, ref reader, typeToConvert, options);
        }

        private T Read(JsonConverter<T> c, ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return c.Read(ref reader, typeToConvert, options);  
        }
        public override void Write(JsonConverter c, Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            Write((JsonConverter<T>)c, writer, (T)value, options);
        }

        private void Write(JsonConverter<T> c, Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            c.Write(writer, value, options);
        }
    }
}
