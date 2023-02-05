using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// An implementation of <see cref="IAttributesTable"/> backed by a <see cref="JsonObject"/>.
    /// </summary>
    public sealed class JsonObjectAttributesTable : IAttributesTable
    {
        // request to future maintainers: please do not make this public until our System.Text.Json
        // reference is at 8.0.0 or higher. when it is, please throw ArgumentException if the given
        // serializerOptions is not read-only (dotnet/runtime#74431). thanks for your consideration.
        internal JsonObjectAttributesTable(JsonObject rootObject, JsonSerializerOptions serializerOptions)
        {
            RootObject = rootObject;
            SerializerOptions = serializerOptions;
        }

        /// <summary>
        /// Gets the <see cref="JsonObject"/> object that this instance adapts to fit the
        /// <see cref="IAttributesTable"/> interface.
        /// </summary>
        public JsonObject RootObject { get; }

        /// <summary>
        /// Gets the <see cref="JsonSerializerOptions"/> object that will be used to save all edits
        /// made to this table onto <see cref="RootObject"/>.
        /// </summary>
        public JsonSerializerOptions SerializerOptions { get; }

        /// <inheritdoc />
        public object this[string attributeName]
        {
            get
            {
                return RootObject.TryGetPropertyValue(attributeName, out var prop)
                    ? ConvertFromJsonNode(prop)
                    : throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            set
            {
                RootObject[attributeName] = ConvertToJsonNode(value);
            }
        }

        /// <inheritdoc />
        public int Count => RootObject.Count;

        /// <inheritdoc />
        public void Add(string attributeName, object value)
        {
            RootObject.Add(attributeName, ConvertToJsonNode(value));
        }

        /// <inheritdoc />
        public void DeleteAttribute(string attributeName)
        {
            RootObject.Remove(attributeName);
        }

        /// <inheritdoc />
        public bool Exists(string attributeName)
        {
            return RootObject.ContainsKey(attributeName);
        }

        /// <inheritdoc />
        public object GetOptionalValue(string attributeName)
        {
            return RootObject.TryGetPropertyValue(attributeName, out var prop)
                ? ConvertFromJsonNode(prop)
                : null;
        }

        /// <inheritdoc />
        public Type GetType(string attributeName)
        {
            if (!RootObject.TryGetPropertyValue(attributeName, out var prop))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            return ConvertFromJsonNode(prop)?.GetType() ?? typeof(object);
        }

        /// <inheritdoc />
        public string[] GetNames()
        {
            return RootObject.Select(kvp => kvp.Key).ToArray();
        }

        /// <inheritdoc />
        public object[] GetValues()
        {
            return RootObject.Select(kvp => ConvertFromJsonNode(kvp.Value)).ToArray();
        }

        /// <summary>
        /// Attempts to convert this table to a strongly-typed value.
        /// <para>
        /// This is essentially just a way of calling
        /// <see cref="JsonSerializer.Deserialize{TValue}(JsonNode, JsonSerializerOptions)"/>
        /// on a Feature's <c>"properties"</c> object.
        /// </para>
        /// <para>
        /// <c>System.Text.Json</c> intentionally omits the functionality that would let us do this
        /// automatically, for security reasons, so this is the workaround for now.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to convert to.
        /// </typeparam>
        /// <param name="options">
        /// The <see cref="JsonSerializerOptions"/> to use for the deserialization, or
        /// <see langword="null"/> to use the options from <see cref="SerializerOptions"/>.
        /// </param>
        /// <param name="deserialized">
        /// Receives the converted value on success, or the default value on failure.
        /// </param>
        /// <returns>
        /// A value indicating whether or not the conversion succeeded.
        /// </returns>
        public bool TryDeserializeJsonObject<T>(JsonSerializerOptions options, out T deserialized)
        {
            try
            {
                deserialized = JsonSerializer.Deserialize<T>(RootObject, options ?? SerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                deserialized = default;
                return false;
            }
        }

        /// <summary>
        /// Attempts to get a strongly-typed value for that corresponds to a property of this table.
        /// <para>
        /// This is essentially just a way of calling
        /// <see cref="JsonSerializer.Deserialize{TValue}(JsonNode, JsonSerializerOptions)"/>
        /// on one of the individual items from a Feature's <c>"properties"</c>.
        /// </para>
        /// <para>
        /// <c>System.Text.Json</c> intentionally omits the functionality that would let us do this
        /// automatically, for security reasons, so this is the workaround for now.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to retrieve.
        /// </typeparam>
        /// <param name="propertyName">
        /// The name of the property in this table to get as the specified type.
        /// </param>
        /// <param name="options">
        /// The <see cref="JsonSerializerOptions"/> to use for the deserialization, or
        /// <see langword="null"/> to use the options from <see cref="SerializerOptions"/>.
        /// </param>
        /// <param name="deserialized">
        /// Receives the converted value on success, or the default value on failure.
        /// </param>
        /// <returns>
        /// A value indicating whether or not the conversion succeeded.
        /// </returns>
        public bool TryGetJsonObjectPropertyValue<T>(string propertyName, JsonSerializerOptions options, out T deserialized)
        {
            if (!this.RootObject.TryGetPropertyValue(propertyName, out var elementToTransform))
            {
                deserialized = default;
                return false;
            }

            try
            {
                deserialized = JsonSerializer.Deserialize<T>(elementToTransform, options ?? SerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                deserialized = default;
                return false;
            }
        }

        private object ConvertFromJsonNode(JsonNode prop)
        {
            switch (prop)
            {
                case null:
                    return null;

                case JsonObject propObj:
                    return new JsonObjectAttributesTable(propObj, SerializerOptions);

                case JsonArray propArr:
                    return propArr.Select(ConvertFromJsonNode).ToArray();
            }

            // else it's a JsonValue... not sure of a cleaner way to handle this than to just reuse
            // the code we have that deals with JsonElement.
            JsonElement propAsElement = JsonSerializer.Deserialize<JsonElement>(prop, SerializerOptions);
            object result = JsonElementAttributesTable.ConvertValue(propAsElement);
            if (result is JsonElementAttributesTable elementTable)
            {
                result = new JsonObjectAttributesTable(JsonObject.Create(elementTable.RootElement.Clone(), RootObject.Options), SerializerOptions);
            }

            return result;
        }

        private JsonNode ConvertToJsonNode(object obj)
        {
            return JsonSerializer.SerializeToNode(obj, obj?.GetType() ?? typeof(object), SerializerOptions);
        }
    }
}
