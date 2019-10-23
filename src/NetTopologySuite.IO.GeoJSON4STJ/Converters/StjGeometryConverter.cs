using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    public partial class StjGeometryConverter : JsonConverter<Geometry>
    {
        /// <summary>
        /// Gets the default geometry factory to use with this converter.
        /// </summary>
        public static GeometryFactory DefaultGeometryFactory { get; } = new OgcCompliantGeometryFactory(new PrecisionModel(), 4326);

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

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public StjGeometryConverter(GeometryFactory geometryFactory = null)
        {
            _geometryFactory = geometryFactory ?? DefaultGeometryFactory;
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
            MemoryStream coordinateData = null;
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
                        coordinateData = ReadCoordinateData(ref reader, options);
                        break;
                    case "bbox":
                        var env = ReadBBox(ref reader, options);
                        break;

                }
                // Skip comments
                reader.SkipComments();
            }
            reader.ReadToken(JsonTokenType.EndObject);

            var cdr = new Utf8JsonReader(); 
            if (geometryType != GeoJsonObjectType.GeometryCollection)
            {
                if (coordinateData == null)
                    throw new JsonException(Resources.EX_NoCoordinatesDefined);
                cdr = new Utf8JsonReader(new ReadOnlySpan<byte>(coordinateData.ToArray()));
                cdr.Read();
            }

            Geometry geometry;
            switch (geometryType)
            {
                case GeoJsonObjectType.Point:
                    geometry = _geometryFactory.CreatePoint(ReadCoordinateSequence(ref cdr, options, true));
                    break;
                case GeoJsonObjectType.LineString:
                    geometry = _geometryFactory.CreateLineString(ReadCoordinateSequence(ref cdr, options, false));
                    break;
                case GeoJsonObjectType.Polygon:
                    geometry = CreatePolygon(ReadCoordinateSequences1(ref cdr, options));
                    break;
                case GeoJsonObjectType.MultiPoint:
                    geometry = _geometryFactory.CreateMultiPoint(ReadCoordinateSequence(ref cdr, options, false));
                    break;
                case GeoJsonObjectType.MultiLineString:
                    geometry = CreateMultiLineString(ReadCoordinateSequences1(ref cdr, options));
                    break;
                case GeoJsonObjectType.MultiPolygon:
                    geometry = CreateMultiPolygon(ReadCoordinateSequences2(ref cdr, options));
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
                geometries.Add(Read(ref reader, typeof(Geometry), options));

            reader.ReadToken(JsonTokenType.EndArray);
            return geometries.ToArray();
        }

        private Polygon CreatePolygon(IReadOnlyList<CoordinateSequence> ringData)
        {
            var shell = _geometryFactory.CreateLinearRing(ringData[0]);
            LinearRing[] holes = null;
            if (ringData.Count > 1)
            {
                holes = new LinearRing[ringData.Count - 1];
                for (int i = 1; i < ringData.Count; i++)
                    holes[i - 1] = _geometryFactory.CreateLinearRing(ringData[i]);
            }
            return _geometryFactory.CreatePolygon(shell, holes);
        }

        /// <summary>
        /// Utility function to create a <see cref="MultiLineString"/> of a set of <see cref="CoordinateSequence"/>s.
        /// </summary>
        /// <param name="multiLineStringSequences">The sequences that make up the <c>MultiLineString</c>'s <c>LineString</c>s</param>
        /// <returns>A <see cref="MultiLineString"/></returns>
        private Geometry CreateMultiLineString(IReadOnlyList<CoordinateSequence> multiLineStringSequences)
        {
            var lineStrings = new LineString[multiLineStringSequences.Count];
            for (int i = 0; i < multiLineStringSequences.Count; i++)
                lineStrings[i] = _geometryFactory.CreateLineString(multiLineStringSequences[i]);

            return _geometryFactory.CreateMultiLineString(lineStrings);
        }

        /// <summary>
        /// Utility function to create a <see cref="MultiPolygon"/> of a set of <see cref="CoordinateSequence"/>s.
        /// </summary>
        /// <param name="multiPolygonSequences">The sequences that make up the <c>MultiPolygon</c>'s <c>Polygon</c>s</param>
        /// <returns>A <see cref="MultiPolygon"/></returns>
        private Geometry CreateMultiPolygon(IReadOnlyList<CoordinateSequence[]> multiPolygonSequences)
        {
            var polygons = new Polygon[multiPolygonSequences.Count];
            for (int i = 0; i < multiPolygonSequences.Count; i++)
                polygons[i] = CreatePolygon(multiPolygonSequences[i]);

            return _geometryFactory.CreateMultiPolygon(polygons);
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
            WriteBBox(writer, value.EnvelopeInternal, options);

            writer.WriteEndObject();
        }

        private void WritePolygon(Utf8JsonWriter writer, Polygon value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            WriteCoordinateSequence(writer, value.ExteriorRing.CoordinateSequence, options);
            for (int i = 0; i < value.NumInteriorRings; i++)
                WriteCoordinateSequence(writer, value.GetInteriorRingN(i).CoordinateSequence, options);
            writer.WriteEndArray();
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(Geometry).IsAssignableFrom(typeToConvert);
        }
    }
}
