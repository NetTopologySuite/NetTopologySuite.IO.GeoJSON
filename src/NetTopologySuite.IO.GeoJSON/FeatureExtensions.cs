using NetTopologySuite.Features;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Defines extensions for <see cref="IAttributesTable"/>.
    /// </summary>
    public static class AttributesTableExtensions
    {
        /// <summary>
        /// The name of the "id" property.
        /// </summary>
        public static readonly string IdPropertyName = "id";

        /// <summary>
        /// Gets the GeoJSON-defined "id" property, if present.
        /// </summary>
        /// <param name="attributes">
        /// The <see cref="IAttributesTable"/> to query, or <see langword="null"/>.
        /// </param>
        /// <param name="id">
        /// Receives the "id" value, if present, <see langword="null"/> otherwise.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the "id" value was present, <see langword="false"/> otherwise.
        /// </returns>
        public static bool TryGetId(this IAttributesTable attributes, out object id)
        {
            id = attributes?.GetOptionalValue(IdPropertyName);
            return !(id is null);
        }
    }
}
