using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

            RootElement = rootElement.Clone();
        }

        /// <summary>
        /// Gets the <see cref="JsonElement"/> object that this instance adapts to fit the
        /// <see cref="IAttributesTable"/> interface.
        /// </summary>
        /// <remarks>
        /// <see cref="JsonElement.ValueKind"/> will be <see cref="JsonValueKind.Object"/>.
        /// </remarks>
        public JsonElement RootElement { get; }

        /// <inheritdoc />
        public object this[string attributeName]
        {
            get
            {
                return RootElement.TryGetProperty(attributeName, out var prop)
                    ? ConvertValue(prop)
                    : throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            set
            {
                ThrowNotSupportedExceptionForReadOnlyTable();
            }
        }

        /// <inheritdoc />
        public int Count => RootElement.EnumerateObject().Count();

        /// <inheritdoc />
        public bool Exists(string attributeName)
        {
            return RootElement.TryGetProperty(attributeName, out _);
        }

        /// <inheritdoc />
        public object GetOptionalValue(string attributeName)
        {
            return RootElement.TryGetProperty(attributeName, out var prop)
                ? ConvertValue(prop)
                : null;
        }

        /// <inheritdoc />
        public Type GetType(string attributeName)
        {
            if (!RootElement.TryGetProperty(attributeName, out var prop))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            return ConvertValue(prop)?.GetType() ?? typeof(object);
        }

        /// <inheritdoc />
        public string[] GetNames()
        {
            return RootElement.EnumerateObject()
                              .Select(prop => prop.Name)
                              .ToArray();
        }

        /// <inheritdoc />
        public object[] GetValues()
        {
            return RootElement.EnumerateObject()
                              .Select(prop => GetOptionalValue(prop.Name))
                              .ToArray();
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
            return TryDeserializeElement(this.RootElement, options, out deserialized);
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
            if (!this.RootElement.TryGetProperty(propertyName, out var elementToTransform))
            {
                deserialized = default;
                return false;
            }

            return TryDeserializeElement(elementToTransform, options, out deserialized);
        }

        void IAttributesTable.Add(string attributeName, object value)
        {
            ThrowNotSupportedExceptionForReadOnlyTable();
        }

        void IAttributesTable.DeleteAttribute(string attributeName)
        {
            ThrowNotSupportedExceptionForReadOnlyTable();
        }

        private static void ThrowNotSupportedExceptionForReadOnlyTable()
        {
            throw new NotSupportedException("Modifying this attribute table is not supported.");
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
    }
}
