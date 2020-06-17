using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Defines extension methods for the <see cref="IAttributesTable"/>, to be used with instances
    /// created by the converters exposed by this library.
    /// </summary>
    public static class StjAttributesTableExtensions
    {
        /// <summary>
        /// Attempts to get a strongly-typed value for that corresponds to a property of this table,
        /// if the table came from this library.
        /// <para>
        /// This is essentially just a way of calling
        /// <see cref="JsonSerializer.Deserialize{TValue}(ref Utf8JsonReader, JsonSerializerOptions)"/>
        /// on one of the individual items from a Feature's <c>"properties"</c>.
        /// </para>
        /// <para>
        /// <c>System.Text.Json</c> intentionally omits the functionality that would let us do this
        /// automatically, for security reasons, so this is the workaround for now.
        /// </para>
        /// <para>
        /// This will always return <see langword="false"/> for tables that were not created by the
        /// converters that this library adds via <see cref="GeoJsonConverterFactory"/>.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to retrieve.
        /// </typeparam>
        /// <param name="table">
        /// This table.
        /// </param>
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
        public static bool TryGetJsonObjectPropertyValue<T>(this IAttributesTable table, string propertyName, JsonSerializerOptions options, out T deserialized)
        {
            deserialized = default;

            if (!(table is StjAttributesTable ourAttributesTable &&
                  ourAttributesTable.RootElement.TryGetProperty(propertyName, out var elementToTransform)))
            {
                return false;
            }

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

                ref DateTime dt = ref Unsafe.As<T, DateTime>(ref deserialized);
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

                ref DateTimeOffset dto = ref Unsafe.As<T, DateTimeOffset>(ref deserialized);
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

                ref Guid guid = ref Unsafe.As<T, Guid>(ref deserialized);
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
                // Consider improving this block once dotnet/runtime#31274 is available.
                using (var ms = new MemoryStream())
                {
                    using (var wr = new Utf8JsonWriter(ms))
                    {
                        elementToTransform.WriteTo(wr);
                    }

                    var reader = new Utf8JsonReader(ms.TryGetBuffer(out var buf) ? (ReadOnlySpan<byte>)buf : ms.ToArray());
                    try
                    {
                        deserialized = JsonSerializer.Deserialize<T>(ref reader, options);
                        return true;
                    }
                    catch (JsonException)
                    {
                        return false;
                    }
                }
            }
        }
    }
}
