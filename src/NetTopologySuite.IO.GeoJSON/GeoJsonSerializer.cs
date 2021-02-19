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
        /// <summary>
        /// Gets a default GeometryFactory
        /// </summary>
        internal static GeometryFactory Wgs84Factory { get; } = new GeometryFactory(new PrecisionModel(), 4326);

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

            AddGeoJsonConverters(s, Wgs84Factory, 2, false);
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
            AddGeoJsonConverters(s, Wgs84Factory, 2, false);

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
            return Create(factory, 2, false);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <param name="factory">
        /// A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.
        /// </param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be between 2 and 4.
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// Creates a serializer using <see cref="Create(JsonSerializerSettings,GeometryFactory,int)"/> internally.
        /// </remarks>
        public static JsonSerializer Create(GeometryFactory factory, int dimension)
        {
            return Create(factory, dimension, false);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer
        /// </summary>
        /// <param name="factory">
        /// A factory to use when creating geometries. The factories <see cref="PrecisionModel"/>
        /// is also used to format <see cref="Coordinate.X"/> and <see cref="Coordinate.Y"/> of the coordinates.
        /// </param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be between 2 and 4.
        /// </param>
        /// <param name="allowMeasurements">
        /// If the coordiantes should allow measurement values
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        /// <remarks>
        /// Creates a serializer using <see cref="Create(JsonSerializerSettings,GeometryFactory,int,bool)"/> internally.
        /// </remarks>
        public static JsonSerializer Create(GeometryFactory factory, int dimension, bool allowMeasurements)
        {
            return Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, factory, dimension, allowMeasurements);
        }

        /// <summary>
        /// Factory method to create a (Geo)JsonSerializer using the provider serializer settings and geometry factory
        /// </summary>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory)
        {
            return Create(settings, factory, 2, false);
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
            return Create(settings, factory, dimension, false);
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
        /// <param name="allowMeasurements">
        /// If the coordiantes should allow measurement values
        /// </param>
        /// <returns>
        /// A <see cref="JsonSerializer"/>.
        /// </returns>
        public static JsonSerializer Create(JsonSerializerSettings settings, GeometryFactory factory, int dimension, bool allowMeasurements)
        {
            if (dimension < 2 || dimension > 4)
            {
                throw new ArgumentException("Must be between 2 and 4", nameof(dimension));
            }

            var s = Create(settings);
            AddGeoJsonConverters(s, factory, dimension, allowMeasurements);
            return s;
        }

        private static void AddGeoJsonConverters(JsonSerializer s, GeometryFactory factory, int dimension, bool allowMeasurements)
        {
            if (factory.SRID != 4326)
            {
                Trace.WriteLine("Factory with SRID of unsupported coordinate reference system.");
            }

            var c = s.Converters;
            c.Add(new FeatureCollectionConverter());
            c.Add(new FeatureConverter());
            c.Add(new AttributesTableConverter());
            c.Add(new GeometryConverter(factory, dimension, allowMeasurements));
            c.Add(new GeometryArrayConverter(factory, dimension));
            c.Add(new CoordinateConverter(factory.PrecisionModel, dimension));
            c.Add(new EnvelopeConverter());
        }
    }
}
