using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    internal partial class StjGeometryConverter : JsonConverter<IGeometry>
    {
        /// <summary>
        /// Gets the default geometry factory to use with this converter.
        /// </summary>
        public static IGeometryFactory DefaultGeometryFactory { get; } = NtsGeometryServices.Instance.CreateGeometryFactory(new PrecisionModel(), 4326);

        private readonly IGeometryFactory _geometryFactory;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public StjGeometryConverter(IGeometryFactory geometryFactory = null)
        {
            _geometryFactory = geometryFactory ?? DefaultGeometryFactory;
        }

        public override IGeometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                reader.Read();
                return null;
            }

            reader.ReadToken(JsonTokenType.StartObject);
            reader.SkipComments();

            GeoJsonObjectType? geometryType = null;
            StjParsedCoordinates coordinateData = default;
            IGeometry[] geometries = null;

            while (reader.TokenType == JsonTokenType.PropertyName)
            {
                // Get the property name and decide what to do
                string propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "type":
                        geometryType = (GeoJsonObjectType) Enum.Parse(typeof(GeoJsonObjectType), reader.GetString());
                        reader.ReadToken(JsonTokenType.String);
                        break;
                    case "geometries":
                        geometries = ReadGeometries(ref reader, options);
                        break;
                    case "coordinates":
                        coordinateData = StjParsedCoordinates.Parse(ref reader, _geometryFactory);
                        reader.ReadToken(JsonTokenType.EndArray);
                        break;
                    case "bbox":
                        var env = ReadBBox(ref reader, options);
                        break;

                }
                // Skip comments
                reader.SkipComments();
            }

            if (geometryType != GeoJsonObjectType.GeometryCollection)
            {
                if (!geometryType.HasValue)
                {
                    throw new JsonException(Resources.EX_NoGeometryTypeDefined);
                }

                var supportedTypes = coordinateData.SupportedTypes;
                if (supportedTypes.IsEmpty)
                {
                    throw new JsonException(Resources.EX_NoCoordinatesDefined);
                }

                bool geometryTypeIsCompatible = false;
                foreach (var supportedType in supportedTypes)
                {
                    if (supportedType == geometryType)
                    {
                        geometryTypeIsCompatible = true;
                        break;
                    }
                }

                if (!geometryTypeIsCompatible)
                {
                    throw new JsonException(string.Format(Resources.EX_CoordinatesIncompatibleWithType, geometryType));
                }
            }

            IGeometry geometry;
            switch (geometryType)
            {
                case GeoJsonObjectType.Point:
                    geometry = coordinateData.ToPoint();
                    break;
                case GeoJsonObjectType.LineString:
                    geometry = coordinateData.ToLineString(_geometryFactory);
                    break;
                case GeoJsonObjectType.Polygon:
                    geometry = coordinateData.ToPolygon(_geometryFactory);
                    break;
                case GeoJsonObjectType.MultiPoint:
                    geometry = coordinateData.ToMultiPoint(_geometryFactory);
                    break;
                case GeoJsonObjectType.MultiLineString:
                    geometry = coordinateData.ToMultiLineString(_geometryFactory);
                    break;
                case GeoJsonObjectType.MultiPolygon:
                    geometry = coordinateData.ToMultiPolygon();
                    break;
                case GeoJsonObjectType.GeometryCollection:
                    if (geometries == null)
                        throw new JsonException(Resources.EX_GCTypeWithoutGeometries);
                    geometry = _geometryFactory.CreateGeometryCollection(geometries ?? new Geometry[0]);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return geometry;
        }

        private IGeometry[] ReadGeometries(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.ReadToken(JsonTokenType.StartArray);

            var geometries = new List<IGeometry>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                geometries.Add(Read(ref reader, typeof(IGeometry), options));
                reader.ReadToken(JsonTokenType.EndObject);
            }

            reader.ReadToken(JsonTokenType.EndArray);
            return geometries.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, IGeometry value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString("type", Enum.GetName(typeof(OgcGeometryType), value.OgcGeometryType));
            if (value.OgcGeometryType == OgcGeometryType.GeometryCollection)
            {
                writer.WritePropertyName("geometries");
                writer.WriteStartArray();
                for (int i = 0; i < value.NumGeometries; i++)
                    Write(writer, value.GetGeometryN(i), options);
                writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName("coordinates");
                switch (value.OgcGeometryType)
                {
                    case OgcGeometryType.Point:
                        WriteCoordinateSequence(writer, ((IPoint)value).CoordinateSequence, options, false);
                        break;
                    case OgcGeometryType.LineString:
                        WriteCoordinateSequence(writer, ((ILineString)value).CoordinateSequence, options);
                        break;
                    case OgcGeometryType.Polygon:
                        WritePolygon(writer, (IPolygon)value, options);
                        break;
                    case OgcGeometryType.MultiPoint:
                        writer.WriteStartArray();
                        for (int i = 0; i < value.NumGeometries; i++)
                            WriteCoordinateSequence(writer, ((IPoint)value.GetGeometryN(i)).CoordinateSequence, options, false);
                        writer.WriteEndArray();
                        break;
                    case OgcGeometryType.MultiLineString:
                        writer.WriteStartArray();
                        for (int i = 0; i < value.NumGeometries; i++)
                            WriteCoordinateSequence(writer, ((ILineString)value.GetGeometryN(i)).CoordinateSequence, options);
                        writer.WriteEndArray();
                        break;
                    case OgcGeometryType.MultiPolygon:
                        writer.WriteStartArray();
                        for (int i = 0; i < value.NumGeometries; i++)
                            WritePolygon(writer, (Polygon)value.GetGeometryN(i), options);
                        writer.WriteEndArray();
                        break;
                }
            }

            if (WriteGeometryBBox)
                WriteBBox(writer, value.EnvelopeInternal, options, value);

            writer.WriteEndObject();
        }

        private void WritePolygon(Utf8JsonWriter writer, IPolygon value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            WriteCoordinateSequence(writer, value.ExteriorRing.CoordinateSequence, options, orientation:OrientationIndex.CounterClockwise);
            for (int i = 0; i < value.NumInteriorRings; i++)
                WriteCoordinateSequence(writer, value.GetInteriorRingN(i).CoordinateSequence, options, orientation: OrientationIndex.Clockwise);
            writer.WriteEndArray();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IGeometry).IsAssignableFrom(typeToConvert);
        }
    }
}
