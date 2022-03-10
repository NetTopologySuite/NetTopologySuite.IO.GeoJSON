using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    /// <inheritdoc cref="JsonConverterFactory"/>>
    public class GeoJsonConverterFactory : JsonConverterFactory
    {
        /// <summary>
        /// The default name that, when seen on <see cref="IAttributesTable"/> or the "properties"
        /// object of a feature, indicates that it should "really" be the feature's "id", not stored
        /// in "properties" as-is.
        /// </summary>
        public static readonly string DefaultIdPropertyName = "_NetTopologySuite_id";

        private static readonly HashSet<Type> GeometryTypes = new HashSet<Type>
        {
            typeof(Geometry),
            typeof(Point),
            typeof(LineString),
            typeof(Polygon),
            typeof(MultiPoint),
            typeof(MultiLineString),
            typeof(MultiPolygon),
            typeof(GeometryCollection),
        };

        private readonly GeometryFactory _factory;

        private readonly bool _writeBBox;

        private readonly string _idPropertyName;

        /// <summary>
        /// Creates an instance of this class using the defaults.
        /// </summary>
        public GeoJsonConverterFactory()
            : this(NtsGeometryServices.Instance.CreateGeometryFactory(4326), false)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> and
        /// defaults for other values.
        /// </summary>
        /// <param name="factory"></param>
        public GeoJsonConverterFactory(GeometryFactory factory)
            : this(factory, false)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/>, the
        /// given value for whether or not we should write out a "bbox" for a plain geometry,
        /// feature and feature collection, and defaults for all other values.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="writeBBox"></param>
        public GeoJsonConverterFactory(GeometryFactory factory, bool writeBBox)
            : this(factory, writeBBox, DefaultIdPropertyName)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/>, the
        /// given value for whether or not we should write out a "bbox" for a plain geometry,
        /// feature and feature collection, and the given "magic" string to signal
        /// when an <see cref="IAttributesTable"/> property is actually filling in for a Feature's "id".
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="writeBBox"></param>
        /// <param name="idPropertyName"></param>
        public GeoJsonConverterFactory(GeometryFactory factory, bool writeBBox, string idPropertyName)
        {
            _factory = factory;
            _writeBBox = writeBBox;
            _idPropertyName = idPropertyName ?? DefaultIdPropertyName;
        }

        ///<inheritdoc cref="JsonConverter.CanConvert(Type)"/>
        public override bool CanConvert(Type typeToConvert)
        {
            return GeometryTypes.Contains(typeToConvert)
                   || typeof(IFeature).IsAssignableFrom(typeToConvert)
                   || typeToConvert == typeof(FeatureCollection)
                   || typeof(IAttributesTable).IsAssignableFrom(typeToConvert);
        }

        ///<inheritdoc cref="JsonConverterFactory.CreateConverter(Type, JsonSerializerOptions)"/>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (GeometryTypes.Contains(typeToConvert))
                return new StjGeometryConverter(_factory, _writeBBox);
            if (typeToConvert == typeof(FeatureCollection))
                return new StjFeatureCollectionConverter(_writeBBox);
            if (typeof(IFeature).IsAssignableFrom(typeToConvert))
                return new StjFeatureConverter(_idPropertyName, _writeBBox);
            if (typeof(IAttributesTable).IsAssignableFrom(typeToConvert))
                return new StjAttributesTableConverter(_idPropertyName);

            throw new ArgumentException(nameof(typeToConvert));
        }
    }
}
