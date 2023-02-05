using System;
using System.Text.Json;

using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Adapter for code that targets older versions of this library that did not expose the
    /// <see cref="IPartiallyDeserializedAttributesTable"/> interface publicly.
    /// </summary>
    public static class StjAttributesTableExtensions
    {
        /// <summary>
        /// Attempts to convert this table to a strongly-typed value, if the table implements
        /// the <see cref="IPartiallyDeserializedAttributesTable"/> interface.
        /// <para>
        /// <c>System.Text.Json</c> intentionally omits the functionality that would let us do this
        /// automatically, for security reasons, so this is the workaround for now.
        /// </para>
        /// <para>
        /// This will always return <see langword="false"/> for tables that do not implement the
        /// <see cref="IPartiallyDeserializedAttributesTable"/> interface.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to convert to.
        /// </typeparam>
        /// <param name="table">
        /// This table.
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
        [Obsolete("Cast to IPartiallyDeserializedAttributesTable and call the instance method instead.")]
        public static bool TryDeserializeJsonObject<T>(this IAttributesTable table, JsonSerializerOptions options, out T deserialized)
        {
            if (!(table is IPartiallyDeserializedAttributesTable ourAttributesTable))
            {
                deserialized = default;
                return false;
            }

            return ourAttributesTable.TryDeserializeJsonObject(options, out deserialized);
        }

        /// <summary>
        /// Attempts to get a strongly-typed value for that corresponds to a property of this table,
        /// if the table implements the <see cref="IPartiallyDeserializedAttributesTable"/> interface.
        /// <para>
        /// <c>System.Text.Json</c> intentionally omits the functionality that would let us do this
        /// automatically, for security reasons, so this is the workaround for now.
        /// </para>
        /// <para>
        /// This will always return <see langword="false"/> for tables that do not implement the
        /// <see cref="IPartiallyDeserializedAttributesTable"/> interface.
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
        [Obsolete("Cast to IPartiallyDeserializedAttributesTable and call the instance method instead.")]
        public static bool TryGetJsonObjectPropertyValue<T>(this IAttributesTable table, string propertyName, JsonSerializerOptions options, out T deserialized)
        {
            if (!(table is IPartiallyDeserializedAttributesTable ourAttributesTable))
            {
                deserialized = default;
                return false;
            }

            return ourAttributesTable.TryGetJsonObjectPropertyValue(propertyName, options, out deserialized);
        }
    }
}
