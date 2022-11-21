namespace NetTopologySuite.IO
{
    /// <summary>
    /// An enumeration of possible ring orientation modes that can be applied when writing GeoJSON
    /// </summary>
    public enum RingOrientationOption
    {
        /// <summary>
        /// Polygon ring orientation is not altered
        /// </summary>
        DoNotModify,
        /// <summary>
        /// Polygon ring orientation is ensured to be how it is definded in RFC7946 §3.1.6:
        /// </summary>
        /// /// <remarks>
        /// This means:
        /// <list type="bullet">
        /// <item><term>Exterior</term><description>Counter-Clockwise</description></item>
        /// <item><term>Interior</term><description>Clockwise</description></item>
        /// </list>
        /// </remarks>
        EnforceRfc9746,
        /// <summary>
        /// Polygon ring orientation is altered according to how it was done in NetTopologySuite.IO.GeoJSON v2 due to misinterpreting right-hand-rule:
        /// </summary>
        /// /// <remarks>
        /// This means:
        /// <list type="bullet">
        /// <item><term>Exterior</term><description>Clockwise</description></item>
        /// <item><term>Interior</term><description>Counter-Clockwise</description></item>
        /// </list>
        /// </remarks>
        NtsGeoJsonV2
    }
}
