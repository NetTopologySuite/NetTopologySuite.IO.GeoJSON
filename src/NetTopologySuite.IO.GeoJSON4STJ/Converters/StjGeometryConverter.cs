using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    internal partial class StjGeometryConverter : JsonConverter<Geometry>
    {
        /// <summary>
        /// Gets the default geometry factory to use with this converter.
        /// </summary>
        public static GeometryFactory DefaultGeometryFactory { get; } = new GeometryFactory(new PrecisionModel(), 4326);

        /*
        private static readonly ReadOnlySequence<byte> Utf8Point;
        private static readonly ReadOnlySequence<byte> Utf8LineString;
        private static readonly ReadOnlySequence<byte> Utf8Polygon;
        private static readonly ReadOnlySequence<byte> Utf8MultiPoint;
        private static readonly ReadOnlySequence<byte> Utf8MultiLineString;
        private static readonly ReadOnlySequence<byte> Utf8MultiPolygon;
        private static readonly ReadOnlySequence<byte> Utf8GeometryCollection;

        static StjGeometryConverter()
        {
            var enc = System.Text.Encoding.UTF8;
            Utf8Point = new ReadOnlySequence<byte>(enc.GetBytes(nameof(GeoJsonObjectType.Point)));
            Utf8LineString = new ReadOnlySequence<byte>(enc.GetBytes(nameof(GeoJsonObjectType.LineString)));
            Utf8Polygon = new ReadOnlySequence<byte>(enc.GetBytes(nameof(GeoJsonObjectType.Polygon)));
            Utf8MultiPoint = new ReadOnlySequence<byte>(enc.GetBytes(nameof(GeoJsonObjectType.MultiPoint)));
            Utf8MultiLineString = new ReadOnlySequence<byte>(enc.GetBytes(nameof(GeoJsonObjectType.MultiLineString)));
            Utf8MultiPolygon = new ReadOnlySequence<byte>(enc.GetBytes(nameof(GeoJsonObjectType.MultiPolygon)));
            Utf8GeometryCollection = new ReadOnlySequence<byte>(enc.GetBytes(nameof(GeoJsonObjectType.GeometryCollection)));
        }
        */
        private readonly GeometryFactory _geometryFactory;

        private bool _writeGeometryBBox;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        /// <param name="writeGeometryBBox">Whether or not to write "bbox" with the geometry.</param>
        public StjGeometryConverter(GeometryFactory geometryFactory, bool writeGeometryBBox)
        {
            _geometryFactory = geometryFactory ?? DefaultGeometryFactory;
            _writeGeometryBBox = writeGeometryBBox;
        }

        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            Geometry[] geometries = null;

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
                        reader.Read();
                        break;
                    default: // included "bbox" property
                        //read, but can't do anything with it (see NetTopologySuite.IO.GeoJSON => NetTopologySuite.IO.Converters.GeometryConverter.ParseGeometry)
                        reader.Skip();
                        reader.Read();
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

            Geometry geometry;
            switch (geometryType)
            {
                case GeoJsonObjectType.Point:
                    geometry = coordinateData.ToPoint(_geometryFactory);
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
                    geometry = coordinateData.ToMultiPolygon(_geometryFactory);
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

        private Geometry[] ReadGeometries(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.ReadToken(JsonTokenType.StartArray);

            var geometries = new List<Geometry>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                geometries.Add(Read(ref reader, typeof(Geometry), options));
                reader.ReadToken(JsonTokenType.EndObject);
            }

            reader.ReadToken(JsonTokenType.EndArray);
            return geometries.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options)
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
            else if (value.IsEmpty)
            {
                writer.WriteStartArray("coordinates");
                writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName("coordinates");
                switch (value.OgcGeometryType)
                {
                    case OgcGeometryType.Point:
                        WriteCoordinateSequence(writer, ((Point)value).CoordinateSequence, options, false);
                        break;
                    case OgcGeometryType.LineString:
                        WriteCoordinateSequence(writer, ((LineString)value).CoordinateSequence, options);
                        break;
                    case OgcGeometryType.Polygon:
                        WritePolygon(writer, (Polygon)value, options);
                        break;
                    case OgcGeometryType.MultiPoint:
                        writer.WriteStartArray();
                        for (int i = 0; i < value.NumGeometries; i++)
                            WriteCoordinateSequence(writer, ((Point)value.GetGeometryN(i)).CoordinateSequence, options, false);
                        writer.WriteEndArray();
                        break;
                    case OgcGeometryType.MultiLineString:
                        writer.WriteStartArray();
                        for (int i = 0; i < value.NumGeometries; i++)
                            WriteCoordinateSequence(writer, ((LineString)value.GetGeometryN(i)).CoordinateSequence, options);
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

            if (_writeGeometryBBox)
                WriteBBox(writer, value.EnvelopeInternal, options);

            writer.WriteEndObject();
        }

        private void WritePolygon(Utf8JsonWriter writer, Polygon value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            WriteCoordinateSequence(writer, value.ExteriorRing.CoordinateSequence, options, orientation:OrientationIndex.Clockwise);
            for (int i = 0; i < value.NumInteriorRings; i++)
                WriteCoordinateSequence(writer, value.GetInteriorRingN(i).CoordinateSequence, options, orientation: OrientationIndex.CounterClockwise);
            writer.WriteEndArray();
        }
    }
}
