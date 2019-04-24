using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a GeoJSON Reader allowing for deserialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonReader
    {
        /// <summary>
        /// Gets a default GeometryFactory
        /// </summary>
        [Obsolete("Create your own, internally we use GeoJsonSerializer.Wgs84Factory")]
        public static IGeometryFactory Wgs84Factory { get; } = GeoJsonSerializer.Wgs84Factory;

        private readonly IGeometryFactory _factory;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly int _dimension;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public GeoJsonReader()
            : this(GeoJsonSerializer.Wgs84Factory, new JsonSerializerSettings())
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="IGeometryFactory"/> and
        /// <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="factory">The factory to use when creating geometries</param>
        /// <param name="serializerSettings">The serializer setting</param>
        public GeoJsonReader(IGeometryFactory factory, JsonSerializerSettings serializerSettings)
            : this(factory, serializerSettings, GeoJsonSerializer.DefaultDimension)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="IGeometryFactory"/> and
        /// <see cref="JsonSerializerSettings"/>.
        /// </summary>
        /// <param name="factory">The factory to use when creating geometries</param>
        /// <param name="serializerSettings">The serializer setting</param>
        /// <param name="dimension">The number of dimensions to handle</param>
        public GeoJsonReader(IGeometryFactory factory, JsonSerializerSettings serializerSettings, int dimension)
        {
            _factory = factory;
            _serializerSettings = serializerSettings;
            _dimension = dimension;
        }

        /// <summary>
        /// Reads the specified json.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public TObject Read<TObject>(string json)
            where TObject : class
        {
            var g = GeoJsonSerializer.Create(_serializerSettings, _factory, _dimension);
            using (StringReader sr = new StringReader(json))
            {
                return g.Deserialize<TObject>(new JsonTextReader(sr));
            }
        }
    }
}
