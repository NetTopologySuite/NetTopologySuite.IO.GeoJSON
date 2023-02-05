using System.Text.Json;

namespace NetTopologySuite.Features
{
    /// <summary>
    /// An <see cref="IAttributesTable"/> that has been <b>partially</b> deserialized to a strongly-
    /// typed CLR object model, but which still contains some remnants of the JSON source that
    /// produced it which may require the consumer to tell us more about what types they expected.
    /// <para/>
    /// Due to an intentional limitation in <c>System.Text.Json</c>, there is no way to produce a
    /// standalone GeoJSON object that includes enough information to produce an object graph that's
    /// complete with nested members of arbitrary types.
    /// <para/>
    /// In that spirit, this interface allows you to pick up where this library left off and use the
    /// Feature's attributes in a more strongly-typed fashion using your own knowledge of whatever
    /// internal structure the GeoJSON object is expected to have.
    /// </summary>
    public interface IPartiallyDeserializedAttributesTable : IAttributesTable
    {
        /// <summary>
        /// Attempts to convert this entire table to a strongly-typed CLR object.
        /// <para>
        /// Modifications to the result <b>WILL NOT</b> propagate back to this table, or vice-versa.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of object to convert to.
        /// </typeparam>
        /// <param name="options">
        /// The <see cref="JsonSerializerOptions"/> to use for the deserialization.
        /// </param>
        /// <param name="deserialized">
        /// Receives the converted value on success, or the default value on failure.
        /// </param>
        /// <returns>
        /// A value indicating whether or not the conversion succeeded.
        /// </returns>
        bool TryDeserializeJsonObject<T>(JsonSerializerOptions options, out T deserialized);

        /// <summary>
        /// Attempts to get a strongly-typed CLR object that corresponds to a single property that's
        /// present in this table.
        /// <para>
        /// Modifications to the result <b>WILL NOT</b> propagate back to this table, or vice-versa.
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
        bool TryGetJsonObjectPropertyValue<T>(string propertyName, JsonSerializerOptions options, out T deserialized);
    }
}
