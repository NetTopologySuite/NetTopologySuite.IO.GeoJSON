using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts IAttributesTable object to its JSON representation.
    /// </summary>
    internal class StjAttributesTableConverter : JsonConverter<IAttributesTable>
    {
        private static readonly StjAttributesTable EmptyTable = new StjAttributesTable();

        private readonly string _idPropertyName;

        public StjAttributesTableConverter(string idPropertyName)
        {
            _idPropertyName = idPropertyName;
        }

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

            writer.WriteStartObject();
            foreach (string propertyName in value.GetNames())
            {
                // skip id
                if (propertyName != _idPropertyName)
                {
                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, value[propertyName], value.GetType(propertyName), options);
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
            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                switch (doc.RootElement.ValueKind)
                {
                    case JsonValueKind.Null:
                        return EmptyTable;

                    case JsonValueKind.Object:
                        return new StjAttributesTable(doc.RootElement);

                    default:
                        throw new JsonException();
                }
            }
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
