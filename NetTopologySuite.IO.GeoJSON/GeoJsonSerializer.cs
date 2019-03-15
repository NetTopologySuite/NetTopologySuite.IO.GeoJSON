using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Json Serializer with support for GeoJson object structure.
    /// </summary>
    public class GeoJsonSerializer : JsonSerializer
    {
        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>Calls <see cref="GeoJsonSerializer.CreateDefault()"/> internally</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer Create()
        {
            return CreateDefault();
        }


        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// Creates a serializer using <see cref="JsonSerializer.CreateDefault()"/> internally
        /// and adds the GeoJSON specific converters to it.</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public new static JsonSerializer CreateDefault()
        {
            var s = JsonSerializer.CreateDefault();
            s.NullValueHandling = NullValueHandling.Ignore;

            AddGeoJsonConverters(s, GeoJsonReader.Wgs84Factory);
            return s;
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <remarks>
        /// Creates a serializer using <see cref="GeoJsonSerializer.Create(JsonSerializerSettings,IGeometryFactory)"/> internally.
        /// <see cref="GeoJsonReader.Wgs84Factory"/> is used.</remarks>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(IGeometryFactory factory)
        {
            return Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, factory);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <returns>A <see cref="JsonSerializer"/></returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, IGeometryFactory factory)
        {
            var s = JsonSerializer.Create(settings);
            AddGeoJsonConverters(s, factory);
            return s;
        }

        private static void AddGeoJsonConverters(JsonSerializer s, IGeometryFactory factory)
        {
            var c = s.Converters;
            c.Add(new ICRSObjectConverter());
            c.Add(new FeatureCollectionConverter());
            c.Add(new FeatureConverter());
            c.Add(new AttributesTableConverter());
            c.Add(new GeometryConverter(factory));
            c.Add(new GeometryArrayConverter());
            c.Add(new CoordinateConverter());
            c.Add(new EnvelopeConverter());

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        [Obsolete("Use GeoJsonSerializer.Create...() functions")]
        public GeoJsonSerializer() : this(GeoJsonReader.Wgs84Factory) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeoJsonSerializer"/> class.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        [Obsolete("Use GeoJsonSerializer.Create...() functions")]
        public GeoJsonSerializer(IGeometryFactory geometryFactory)
        {
            base.Converters.Add(new ICRSObjectConverter());
            base.Converters.Add(new FeatureCollectionConverter());
            base.Converters.Add(new FeatureConverter());
            base.Converters.Add(new AttributesTableConverter());
            base.Converters.Add(new GeometryConverter(geometryFactory));
            base.Converters.Add(new GeometryArrayConverter());
            base.Converters.Add(new CoordinateConverter());
            base.Converters.Add(new EnvelopeConverter());
        }
    }
}
