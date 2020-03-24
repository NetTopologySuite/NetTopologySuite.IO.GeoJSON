using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    /// <inheritdoc cref="JsonConverterFactory"/>>
    public class GeoJsonConverterFactory : JsonConverterFactory
    {
        private readonly GeometryFactory _factory;

        /// <summary>
        /// Creates an instance of this class using the (possibly) provided <see cref="IGeometryFilter"/>.
        /// </summary>
        /// <param name="factory"></param>
        public GeoJsonConverterFactory(GeometryFactory factory = null)
        {
            _factory = factory ?? NtsGeometryServices.Instance.CreateGeometryFactory(4326);
        }

        /// <summary>
        /// A delegate function to create a feature
        /// </summary>
        public Func<IFeature> CreateFeatureFunction { get; set; } = () => new Feature();


        /// <summary>
        /// A function to create an attribute table
        /// </summary>
        public Func<IAttributesTable> CreateAttributeTable { get; set; } = () => new AttributesTable();

        /// <summary>
        /// Gets or sets a value indicating if nested objects should be treated as <see cref="JsonElement"/>s.
        /// If set to <c>false</c>, nested objects will be converted to <see cref="IAttributesTable"/>s.
        /// </summary>
        public bool NestedObjectsAsJsonElement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the bounding box should be exported for geometry definition
        /// </summary>
        public bool WriteGeometryBBox { get; set; }

        ///<inheritdoc cref="JsonConverter.CanConvert(Type)"/>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Geometry)
                   || typeof(IFeature).IsAssignableFrom(typeToConvert)
                   || typeToConvert == typeof(FeatureCollection)
                   || typeof(IAttributesTable).IsAssignableFrom(typeToConvert);
        }

        ///<inheritdoc cref="JsonConverterFactory.CreateConverter(Type, JsonSerializerOptions)"/>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(Geometry))
                return new StjGeometryConverter(_factory) {WriteGeometryBBox = WriteGeometryBBox};
            if (typeToConvert == typeof(FeatureCollection))
                return new StjFeatureCollectionConverter();
            if (typeof(IFeature).IsAssignableFrom(typeToConvert))
                return new StjFeatureConverter("id", CreateFeatureFunction, CreateAttributeTable);
            if (typeof(IAttributesTable).IsAssignableFrom(typeToConvert))
                return new StjAttributesTableConverter(CreateAttributeTable) { NestedObjectsAsJsonElement = NestedObjectsAsJsonElement };

            throw new ArgumentException(nameof(typeToConvert));
        }
    }
}
