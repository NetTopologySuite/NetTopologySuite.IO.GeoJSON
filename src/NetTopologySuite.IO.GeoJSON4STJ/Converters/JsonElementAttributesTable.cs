using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// An implementation of <see cref="IAttributesTable"/> backed by a <see cref="JsonElement"/>
    /// whose <see cref="JsonElement.ValueKind"/> is <see cref="JsonValueKind.Object"/>.
    /// </summary>
    /// <remarks>
    /// JSON <see cref="JsonValueKind.Number"/>-valued properties are boxed as <see cref="decimal"/>
    /// values unconditionally, throwing an error if the number is not convertible.
    /// <para/>
    /// JSON <see cref="JsonValueKind.Null"/>-valued properties are considered to be of type
    /// <see cref="object"/>, for the purposes of <see cref="IAttributesTable.GetType"/> only.
    /// <para/>
    /// JSON <see cref="JsonValueKind.Object"/>-valued properties are wrapped in their own
    /// <see cref="JsonElementAttributesTable"/> objects.
    /// </remarks>
    public sealed class JsonElementAttributesTable : IAttributesTable
    {
        private readonly JsonElement _rootElement;

        private JsonObject _writableObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonElementAttributesTable"/> class.
        /// </summary>
        /// <param name="rootElement">
        /// The value for <see cref="RootElement"/>.
        /// </param>
        /// <remarks>
        /// <paramref name="rootElement"/> will be <see cref="JsonElement.Clone">cloned</see> to
        /// ensure that it is safe for this object to outlive the <see cref="JsonDocument"/> that
        /// the given element is attached to, if any.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="rootElement"/>'s <see cref="JsonElement.ValueKind"/> is not
        /// <see cref="JsonValueKind.Object"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// Thrown when <paramref name="rootElement"/> is attached to a <see cref="JsonDocument"/>
        /// parent that has already been disposed.
        /// </exception>
        public JsonElementAttributesTable(JsonElement rootElement)
        {
            if (rootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException($"{nameof(rootElement.ValueKind)} must be {nameof(JsonValueKind.Object)}, not {rootElement.ValueKind}", nameof(rootElement));
            }

            _rootElement = rootElement.Clone();
        }

        private JsonElementAttributesTable(JsonObject writableObject)
        {
            _writableObject = writableObject;
        }

        /// <summary>
        /// Gets the <see cref="JsonElement"/> object that this instance adapts to fit the
        /// <see cref="IAttributesTable"/> interface.
        /// <para/>
        /// Any modifications that are made through the <see cref="IAttributesTable"/> methods will
        /// <b>NOT</b> be observed through this object.
        /// </summary>
        /// <remarks>
        /// <see cref="JsonElement.ValueKind"/> will be <see cref="JsonValueKind.Object"/>.
        /// </remarks>
        public JsonElement RootElement
        {
            get
            {
                if (!(_writableObject is null))
                {
                    throw new InvalidOperationException("The RootElement property is invalid after the table has been modified.");
                }

                return _rootElement;
            }
        }

        private JsonObject WritableObject => _writableObject ?? (_writableObject = JsonObject.Create(_rootElement));

        /// <inheritdoc />
        public object this[string attributeName]
        {
            get
            {
                if (_writableObject is JsonObject writable)
                {
                    return GetItemFromWritable(writable, attributeName);
                }

                return RootElement.TryGetProperty(attributeName, out var prop)
                    ? ConvertValue(prop)
                    : throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            set
            {
                WritableObject[attributeName] = value is null
                    ? null
                    : JsonSerializer.SerializeToNode(value, value.GetType());
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                return _writableObject is JsonObject writable
                    ? writable.Count
                    : RootElement.EnumerateObject().Count();
            }
        }

        /// <inheritdoc />
        public bool Exists(string attributeName)
        {
            return _writableObject is JsonObject writable
                ? writable.ContainsKey(attributeName)
                : _rootElement.TryGetProperty(attributeName, out _);
        }

        /// <inheritdoc />
        public object GetOptionalValue(string attributeName)
        {
            if (_writableObject is JsonObject writable)
            {
                return GetOptionalValueFromWritable(writable, attributeName);
            }

            return RootElement.TryGetProperty(attributeName, out var prop)
                ? ConvertValue(prop)
                : null;
        }

        /// <inheritdoc />
        public Type GetType(string attributeName)
        {
            if (_writableObject is JsonObject writable)
            {
                return GetTypeFromWritable(writable, attributeName);
            }

            if (!RootElement.TryGetProperty(attributeName, out var prop))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            return ConvertValue(prop)?.GetType() ?? typeof(object);
        }

        /// <inheritdoc />
        public string[] GetNames()
        {
            if (_writableObject is JsonObject writable)
            {
                return GetNamesFromWritable(writable);
            }

            return RootElement.EnumerateObject()
                              .Select(prop => prop.Name)
                              .ToArray();
        }

        /// <inheritdoc />
        public object[] GetValues()
        {
            if (_writableObject is JsonObject writable)
            {
                return GetValuesFromWritable(writable);
            }

            return RootElement.EnumerateObject()
                              .Select(prop => GetOptionalValue(prop.Name))
                              .ToArray();
        }

        /// <inheritdoc />
        public void Add(string attributeName, object value)
        {
            WritableObject.Add(attributeName, value is null ? null : JsonSerializer.SerializeToNode(value, value.GetType()));
        }

        /// <inheritdoc />
        public void DeleteAttribute(string attributeName)
        {
            if (!WritableObject.Remove(attributeName))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }
        }

        /// <summary>
        /// Attempts to convert this table to a strongly-typed value.
        /// <para>
        /// This is essentially just a way of calling
        /// <see cref="JsonSerializer.Deserialize{TValue}(ref Utf8JsonReader, JsonSerializerOptions)"/>
        /// on a Feature's <c>"properties"</c> object.
        /// </para>
        /// <para>
        /// <c>System.Text.Json</c> intentionally omits the functionality that would let us do this
        /// automatically, for security reasons, so this is the workaround for now.
        /// </para>.
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to convert to.
        /// </typeparam>.
        /// <param name="options">
        /// The <see cref="JsonSerializerOptions"/> to use for the deserialization.
        /// </param>
        /// <param name="deserialized">
        /// Receives the converted value on success, or the default value on failure.
        /// </param>
        /// <returns>
        /// A value indicating whether or not the conversion succeeded.
        /// </returns>
        public bool TryDeserializeJsonObject<T>(JsonSerializerOptions options, out T deserialized)
        {
            return _writableObject is JsonObject writable
                ? TryDeserializeJsonObjectFromWritable(writable, options, out deserialized)
                : TryDeserializeElement(_rootElement, options, out deserialized);
        }

        /// <summary>
        /// Attempts to get a strongly-typed value for that corresponds to a property of this table.
        /// <para>
        /// This is essentially just a way of calling
        /// <see cref="JsonSerializer.Deserialize{TValue}(ref Utf8JsonReader, JsonSerializerOptions)"/>
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
        /// The <see cref="JsonSerializerOptions"/> to use for the deserialization.
        /// </param>
        /// <param name="deserialized">
        /// Receives the converted value on success, or the default value on failure.
        /// </param>
        /// <returns>
        /// A value indicating whether or not the conversion succeeded.
        /// </returns>
        public bool TryGetJsonObjectPropertyValue<T>(string propertyName, JsonSerializerOptions options, out T deserialized)
        {
            if (_writableObject is JsonObject writable)
            {
                return TryGetJsonObjectPropertyValueFromWritable(writable, propertyName, options, out deserialized);
            }

            if (!_rootElement.TryGetProperty(propertyName, out var elementToTransform))
            {
                deserialized = default;
                return false;
            }

            return TryDeserializeElement(elementToTransform, options, out deserialized);
        }

        private static bool TryDeserializeElement<T>(JsonElement elementToTransform, JsonSerializerOptions options, out T deserialized)
        {
            deserialized = default;

            // try the types that the framework has builtins for.  all but one of these top-level
            // branches should go away at JIT time.
            if (typeof(T) == typeof(byte))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref byte u8 = ref Unsafe.As<T, byte>(ref deserialized);
                return elementToTransform.TryGetByte(out u8);
            }
            else if (typeof(T) == typeof(byte?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetByte(out byte u8)))
                {
                    return false;
                }

                Unsafe.As<T, byte?>(ref deserialized) = u8;
                return true;
            }
            else if (typeof(T) == typeof(sbyte))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref sbyte s8 = ref Unsafe.As<T, sbyte>(ref deserialized);
                return elementToTransform.TryGetSByte(out s8);
            }
            else if (typeof(T) == typeof(sbyte?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetSByte(out sbyte s8)))
                {
                    return false;
                }

                Unsafe.As<T, sbyte?>(ref deserialized) = s8;
                return true;
            }
            else if (typeof(T) == typeof(short))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref short s16 = ref Unsafe.As<T, short>(ref deserialized);
                return elementToTransform.TryGetInt16(out s16);
            }
            else if (typeof(T) == typeof(short?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetInt16(out short s16)))
                {
                    return false;
                }

                Unsafe.As<T, short?>(ref deserialized) = s16;
                return true;
            }
            else if (typeof(T) == typeof(ushort))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref ushort u16 = ref Unsafe.As<T, ushort>(ref deserialized);
                return elementToTransform.TryGetUInt16(out u16);
            }
            else if (typeof(T) == typeof(ushort?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetUInt16(out ushort u16)))
                {
                    return false;
                }

                Unsafe.As<T, ushort?>(ref deserialized) = u16;
                return true;
            }
            else if (typeof(T) == typeof(int))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref int s32 = ref Unsafe.As<T, int>(ref deserialized);
                return elementToTransform.TryGetInt32(out s32);
            }
            else if (typeof(T) == typeof(int?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetInt32(out int s32)))
                {
                    return false;
                }

                Unsafe.As<T, int?>(ref deserialized) = s32;
                return true;
            }
            else if (typeof(T) == typeof(uint))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref uint u32 = ref Unsafe.As<T, uint>(ref deserialized);
                return elementToTransform.TryGetUInt32(out u32);
            }
            else if (typeof(T) == typeof(uint?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetUInt32(out uint u32)))
                {
                    return false;
                }

                Unsafe.As<T, uint?>(ref deserialized) = u32;
                return true;
            }
            else if (typeof(T) == typeof(long))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref long s64 = ref Unsafe.As<T, long>(ref deserialized);
                return elementToTransform.TryGetInt64(out s64);
            }
            else if (typeof(T) == typeof(long?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetInt64(out long s64)))
                {
                    return false;
                }

                Unsafe.As<T, long?>(ref deserialized) = s64;
                return true;
            }
            else if (typeof(T) == typeof(ulong))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref ulong u64 = ref Unsafe.As<T, ulong>(ref deserialized);
                return elementToTransform.TryGetUInt64(out u64);
            }
            else if (typeof(T) == typeof(ulong?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetUInt64(out ulong u64)))
                {
                    return false;
                }

                Unsafe.As<T, ulong?>(ref deserialized) = u64;
                return true;
            }
            else if (typeof(T) == typeof(float))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref float f32 = ref Unsafe.As<T, float>(ref deserialized);
                return elementToTransform.TryGetSingle(out f32);
            }
            else if (typeof(T) == typeof(float?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetSingle(out float f32)))
                {
                    return false;
                }

                Unsafe.As<T, float?>(ref deserialized) = f32;
                return true;
            }
            else if (typeof(T) == typeof(double))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref double f64 = ref Unsafe.As<T, double>(ref deserialized);
                return elementToTransform.TryGetDouble(out f64);
            }
            else if (typeof(T) == typeof(double?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetDouble(out double f64)))
                {
                    return false;
                }

                Unsafe.As<T, double?>(ref deserialized) = f64;
                return true;
            }
            else if (typeof(T) == typeof(decimal))
            {
                if (elementToTransform.ValueKind != JsonValueKind.Number)
                {
                    return false;
                }

                ref decimal d128 = ref Unsafe.As<T, decimal>(ref deserialized);
                return elementToTransform.TryGetDecimal(out d128);
            }
            else if (typeof(T) == typeof(decimal?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.Number && elementToTransform.TryGetDecimal(out decimal d128)))
                {
                    return false;
                }

                Unsafe.As<T, decimal?>(ref deserialized) = d128;
                return true;
            }
            else if (typeof(T) == typeof(DateTime))
            {
                if (elementToTransform.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                ref var dt = ref Unsafe.As<T, DateTime>(ref deserialized);
                return elementToTransform.TryGetDateTime(out dt);
            }
            else if (typeof(T) == typeof(DateTime?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.String && elementToTransform.TryGetDateTime(out var dt)))
                {
                    return false;
                }

                Unsafe.As<T, DateTime?>(ref deserialized) = dt;
                return true;
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                if (elementToTransform.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                ref var dto = ref Unsafe.As<T, DateTimeOffset>(ref deserialized);
                return elementToTransform.TryGetDateTimeOffset(out dto);
            }
            else if (typeof(T) == typeof(DateTimeOffset?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.String && elementToTransform.TryGetDateTimeOffset(out var dto)))
                {
                    return false;
                }

                Unsafe.As<T, DateTimeOffset?>(ref deserialized) = dto;
                return true;
            }
            else if (typeof(T) == typeof(Guid))
            {
                if (elementToTransform.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                ref var guid = ref Unsafe.As<T, Guid>(ref deserialized);
                return elementToTransform.TryGetGuid(out guid);
            }
            else if (typeof(T) == typeof(Guid?))
            {
                if (elementToTransform.ValueKind == JsonValueKind.Null)
                {
                    return true;
                }

                if (!(elementToTransform.ValueKind == JsonValueKind.String && elementToTransform.TryGetGuid(out var guid)))
                {
                    return false;
                }

                Unsafe.As<T, Guid?>(ref deserialized) = guid;
                return true;
            }
            else if (typeof(T) == typeof(string))
            {
                if (elementToTransform.ValueKind != JsonValueKind.String && elementToTransform.ValueKind != JsonValueKind.Null)
                {
                    return false;
                }

                ref string str = ref Unsafe.As<T, string>(ref deserialized);
                str = elementToTransform.GetString();
                return true;
            }
            else
            {
                try
                {
                    deserialized = JsonSerializer.Deserialize<T>(elementToTransform, options);
                    return true;
                }
                catch (JsonException)
                {
                    return false;
                }
            }
        }

        private static object ConvertValue(JsonElement prop)
        {
            switch (prop.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.String:
                    return prop.GetString();

                case JsonValueKind.Object:
                    return new JsonElementAttributesTable(prop);

                case JsonValueKind.Array:
                    return prop.EnumerateArray()
                               .Select(ConvertValue)
                               .ToArray();

                case JsonValueKind.Number when prop.TryGetDecimal(out decimal d):
                    return d;

                case JsonValueKind.Number:
                    throw new NotSupportedException("Number value cannot be boxed as a decimal: " + prop.GetRawText());

                default:
                    throw new NotSupportedException("Unrecognized JsonValueKind: " + prop.ValueKind);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object GetItemFromWritable(JsonObject writable, string attributeName)
        {
            return writable.TryGetPropertyValue(attributeName, out var prop)
                ? ConvertValue(prop)
                : throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string[] GetNamesFromWritable(JsonObject writable)
        {
            return writable.Select(kvp => kvp.Key).ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object[] GetValuesFromWritable(JsonObject writable)
        {
            return writable.Select(kvp => ConvertValue(kvp.Value)).ToArray();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object GetOptionalValueFromWritable(JsonObject writable, string attributeName)
        {
            return writable.TryGetPropertyValue(attributeName, out JsonNode prop)
                ? ConvertValue(prop)
                : null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Type GetTypeFromWritable(JsonObject writable, string attributeName)
        {
            if (!writable.TryGetPropertyValue(attributeName, out var prop))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            return ConvertValue(prop)?.GetType() ?? typeof(object);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryGetJsonObjectPropertyValueFromWritable<T>(JsonObject writable, string propertyName, JsonSerializerOptions options, out T deserialized)
        {
            if (!writable.TryGetPropertyValue(propertyName, out var elementToTransform))
            {
                deserialized = default;
                return false;
            }

            return TryDeserializeNode(elementToTransform, options, out deserialized);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryDeserializeJsonObjectFromWritable<T>(JsonObject writable, JsonSerializerOptions options, out T deserialized)
        {
            return TryDeserializeNode(writable, options, out deserialized);
        }

        private static object ConvertValue(JsonNode prop)
        {
            switch (prop)
            {
                case JsonObject jObject:
                    return new JsonElementAttributesTable(jObject);

                case JsonArray jArray:
                    return jArray.Select(ConvertValue)
                                 .ToArray();

                case JsonValue jValue:
                    if (jValue.TryGetValue(out JsonElement element))
                    {
                        return ConvertValue(element);
                    }

                    if (!jValue.TryGetValue(out object obj))
                    {
                        System.Diagnostics.Debug.Fail("Documented to 'always succeed and return the underlying value as object'.");
                    }

                    return obj;

                default:
                    throw new NotSupportedException("Unrecognized JsonNode subclass: " + prop?.GetType());
            }
        }

        private static bool TryDeserializeNode<T>(JsonNode writable, JsonSerializerOptions options, out T deserialized)
        {
            try
            {
                deserialized = JsonSerializer.Deserialize<T>(writable, options);
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
