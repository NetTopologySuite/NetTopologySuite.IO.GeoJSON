using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using NetTopologySuite.IO.Converters;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// An implementation of <see cref="IAttributesTable"/> backed by a <see cref="JsonObject"/>.
    /// </summary>
    /// <remarks>
    /// Modifications to this table will be observed on <see cref="RootObject"/>, and vice-versa,
    /// including modifications to nested objects and arrays.
    /// </remarks>
    public sealed class JsonObjectAttributesTable : IPartiallyDeserializedAttributesTable
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
                    ? Utility.ObjectFromJsonNode(prop, SerializerOptions)
                    : throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            set
            {
                RootObject[attributeName] = Utility.ObjectToJsonNode(value, SerializerOptions);
            }
        }

        /// <inheritdoc />
        public int Count => RootObject.Count;

        /// <inheritdoc />
        public void Add(string attributeName, object value)
        {
            RootObject.Add(attributeName, Utility.ObjectToJsonNode(value, SerializerOptions));
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
                ? Utility.ObjectFromJsonNode(prop, SerializerOptions)
                : null;
        }

        /// <inheritdoc />
        public Type GetType(string attributeName)
        {
            if (!RootObject.TryGetPropertyValue(attributeName, out var prop))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            return Utility.ObjectFromJsonNode(prop, SerializerOptions)?.GetType() ?? typeof(object);
        }

        /// <inheritdoc />
        public string[] GetNames()
        {
            return RootObject.Select(kvp => kvp.Key).ToArray();
        }

        /// <inheritdoc />
        public object[] GetValues()
        {
            return RootObject.Select(kvp => Utility.ObjectFromJsonNode(kvp.Value, SerializerOptions)).ToArray();
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
    }
}
