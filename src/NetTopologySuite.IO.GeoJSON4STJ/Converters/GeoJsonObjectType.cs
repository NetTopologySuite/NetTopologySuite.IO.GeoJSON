namespace NetTopologySuite.IO
{
    /// <summary>
    /// Defines the GeoJSON Objects types as defined in the <a href="https://tools.ietf.org/html/rfc7946#section-3.1">RFC7946 of the Internet Engineering Task Force (IETF)</a>.
    /// </summary>
    internal enum GeoJsonObjectType
    {
        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.1.2">Point</a> type.
        /// </summary>
        Point,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.1.3">MultiPoint</a> type.
        /// </summary>
        MultiPoint,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.1.4">LineString</a> type.
        /// </summary>
        LineString,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.1.5">MultiLineString</a> type.
        /// </summary>
        MultiLineString,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.1.6">Polygon</a> type.
        /// </summary>
        Polygon,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.1.7">MultiPolygon</a> type.
        /// </summary>
        MultiPolygon,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.1.8">GeometryCollection</a> type.
        /// </summary>
        GeometryCollection,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.2">Feature</a> type.
        /// </summary>
        Feature,

        /// <summary>
        /// Defines the <a href="https://tools.ietf.org/html/rfc7946#section-3.3">FeatureCollection</a> type.
        /// </summary>
        FeatureCollection
    }
}
