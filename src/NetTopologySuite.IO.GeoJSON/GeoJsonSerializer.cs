using System;
using System.Diagnostics;
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
        private static int _dimension = 2;

        /// <summary>
        /// Gets a default GeometryFactory
        /// </summary>
        internal static GeometryFactory Wgs84Factory { get => NtsGeometryServices.Instance.CreateGeometryFactory(4326); }

        /// <summary>
        /// Gets or sets a value indicating if the polygon ring orientation must adhere to
        /// rule defined in RFC7964 §3.1.6
        /// </summary>
        /// <remarks>The default is <see cref="RingOrientationOption.EnforceRfc9746"/></remarks>
        /// <a href="https://www.rfc-editor.org/rfc/rfc7946#section-3.1.6">Polygon</a>
        public static RingOrientationOption RingOrientationOption { get; set; } = RingOrientationOption.EnforceRfc9746;

        /// <summary>
        /// Gets or sets a value indicating the number of dimensions the serializer should handle
        /// </summary>
        /// <remarks>Value must be 2 or 3.</remarks>
        public static int Dimension
        {
            get => _dimension;
            set
            {
                if (value < Dimension || 3 < value)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be Dimension or 3.");
                _dimension = value;
            }
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// Calls <see cref="CreateDefault()"/> internally.
        /// </remarks>
        public new static JsonSerializer Create()
        {
            return CreateDefault();
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="GeometryFactory"/> uses WGS-84.
        /// </remarks>
        public new static JsonSerializer CreateDefault()
        {
            var s = JsonSerializer.CreateDefault();
            s.NullValueHandling = NullValueHandling.Ignore;

            AddGeoJsonConverters(s, Wgs84Factory, Dimension, RingOrientationOption, null);
            return s;
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="GeometryFactory"/> uses WGS-84.
        /// </remarks>
        public new static JsonSerializer CreateDefault(JsonSerializerSettings settings)
        {
            var s = Create(settings);
            AddGeoJsonConverters(s, Wgs84Factory, Dimension, RingOrientationOption, null);

            return s;
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// The <see cref="GeometryFactory"/> uses WGS-84.
        /// </remarks>
        public static JsonSerializer CreateDefault(JsonSerializerSettings settings, JsonSerializerSettings coordinateSerializerSettings)
        {
            var s = Create(settings);
            AddGeoJsonConverters(s, Wgs84Factory, Dimension, RingOrientationOption, coordinateSerializerSettings);

            return s;
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <param name="factory">
        /// A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// Creates a serializer using <see cref="Create(GeometryFactory,int)"/> internally.
        /// </remarks>
        public static JsonSerializer Create(GeometryFactory factory)
        {
            return Create(factory, Dimension);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <param name="factory">
        /// A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.
        /// </param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// Creates a serializer using <see cref="Create(JsonSerializerSettings,GeometryFactory,int)"/> internally.
        /// </remarks>
        public static JsonSerializer Create(GeometryFactory factory, int dimension)
        {
            return Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, factory, dimension);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory)
        {
            return Create(settings, factory, Dimension);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <param name="settings">
        /// Serializer settings
        /// </param>
        /// <param name="factory">
        /// The factory to use when creating a new geometry
        /// </param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory, int dimension)
        {
            return Create(settings, factory, dimension, RingOrientationOption);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <param name="settings">
        /// Serializer settings
        /// </param>
        /// <param name="factory">
        /// The factory to use when creating a new geometry
        /// </param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory, int dimension,
            RingOrientationOption enforceRingOrientation)
        {
            return Create(settings, factory, dimension, enforceRingOrientation, null);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <param name="settings">
        /// Serializer settings
        /// </param>
        /// <param name="factory">
        /// The factory to use when creating a new geometry
        /// </param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out whatever we have.
        /// </param>
        /// <param name="coordinateSerializerSettings">
        /// The <see cref="JsonSerializerSettings"/> to use when writing out "coordinates" arrays,
        /// or <see langword="null"/> if we should just use <paramref name="settings"/>.  Intended
        /// to help fine-tune output, as "coordinates" arrays tend to be extremely long with many
        /// nested arrays that can look better stuffed onto one line by themselves, even if the rest
        /// of the JSON object is indented and split across multiple lines.
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory, int dimension,
            RingOrientationOption enforceRingOrientation, JsonSerializerSettings coordinateSerializerSettings)
        {
            if (dimension != 2 && dimension != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Must be 2 or 3.");
            }

            var s = Create(settings);
            AddGeoJsonConverters(s, factory, dimension, enforceRingOrientation, coordinateSerializerSettings);
            return s;
        }

        private static void AddGeoJsonConverters(JsonSerializer s, GeometryFactory factory, int dimension,
            RingOrientationOption enforceRingOrientation, JsonSerializerSettings coordinateSerializerSettings)
        {
#if DEBUG
            if (factory.SRID != 4326)
            {
                Trace.WriteLine($"Factory with SRID of unsupported coordinate reference system. Supposed to be 4326 (WGS84) but is {factory.SRID}", "Information");
            }
#endif

            var c = s.Converters;
            c.Add(new FeatureCollectionConverter());
            c.Add(new FeatureConverter());
            c.Add(new AttributesTableConverter());
            c.Add(new GeometryConverter(factory, dimension, enforceRingOrientation, coordinateSerializerSettings));
            //c.Add(new GeometryArrayConverter(factory, dimension));
            //c.Add(new CoordinateConverter(factory.PrecisionModel, dimension));
            c.Add(new EnvelopeConverter(factory.PrecisionModel));
        }
    }
}
