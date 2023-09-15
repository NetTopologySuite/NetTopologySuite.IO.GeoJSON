using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts a <see cref="Geometry"/> to and from its JSON representation
    /// </summary>
    public partial class GeometryConverter : JsonConverter
    {
        private readonly GeometryFactory _factory;
        private readonly int _dimension;

        private readonly OrientationIndex _exteriorRingOrientation = OrientationIndex.None;
        private readonly OrientationIndex _interiorRingOrientation = OrientationIndex.None;
        private readonly JsonSerializerSettings _serializerSettingsForCoordinates;

        /// <summary>
        /// Creates an instance of this class using <see cref="GeoJsonSerializer.Wgs84Factory"/> to create geometries.
        /// </summary>
        public GeometryConverter()
            : this(GeoJsonSerializer.Wgs84Factory)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> to create geometries.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        public GeometryConverter(GeometryFactory geometryFactory)
            : this(geometryFactory, GeoJsonSerializer.Dimension)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> to create geometries.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <param name="dimension">The number of dimensions to handle.  Must be 2 or 3.</param>
        public GeometryConverter(GeometryFactory geometryFactory, int dimension)
            :this(geometryFactory, dimension, RingOrientationOption.EnforceRfc9746,
                  new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
        {
        }

        internal GeometryConverter(GeometryFactory geometryFactory, int dimension,
            RingOrientationOption enforceRingOrientation, JsonSerializerSettings settingsForCoordinates)
        {
            if (dimension != 2 && dimension != 3)
            {
                throw new ArgumentException("Must be either 2 or 3", nameof(dimension));
            }

            _factory = geometryFactory;
            _dimension = dimension;

            switch (enforceRingOrientation)
            {
                case RingOrientationOption.EnforceRfc9746:
                    _exteriorRingOrientation = OrientationIndex.CounterClockwise;
                    _interiorRingOrientation = OrientationIndex.Clockwise;
                    break;
                case RingOrientationOption.NtsGeoJsonV2:
                    _exteriorRingOrientation = OrientationIndex.Clockwise;
                    _interiorRingOrientation = OrientationIndex.CounterClockwise;
                    break;
            }
            _serializerSettingsForCoordinates = settingsForCoordinates;
        }

        private JsonSerializerSettings BackupSerializerSettings(JsonWriter writer)
        {
            var backupSettings = new JsonSerializerSettings
            {
                Formatting = writer.Formatting
            };
            return backupSettings;
        }

        private void ApplySerializerSettings(JsonWriter writer, JsonSerializerSettings serializerSettings)
        {
            if (serializerSettings is null)
            {
                throw new ArgumentNullException(nameof(serializerSettings));
            }

            writer.Formatting = serializerSettings.Formatting;
        }

        /// <summary>
        /// Writes a geometry to its JSON representation
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The value</param>
        /// <param name="serializer">The serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is Geometry geom))
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            bool customCoordinateSerializeSettings = _serializerSettingsForCoordinates != null;
            JsonSerializerSettings currentSerializerSettings = null;
            switch (geom)
            {
                case Point point:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.Point));

                    if (customCoordinateSerializeSettings)
                    {
                        currentSerializerSettings = BackupSerializerSettings(writer);
                        ApplySerializerSettings(writer, _serializerSettingsForCoordinates);
                    }
                    writer.WritePropertyName("coordinates");
                    WriteCoordinates(writer, point.CoordinateSequence, false);
                    if (customCoordinateSerializeSettings)
                    {
                        ApplySerializerSettings(writer, currentSerializerSettings);
                    }

                    break;

                case MultiPoint multiPoint:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.MultiPoint));

                    if (customCoordinateSerializeSettings)
                    {
                        currentSerializerSettings = BackupSerializerSettings(writer);
                        ApplySerializerSettings(writer, _serializerSettingsForCoordinates);
                    }
                    writer.WritePropertyName("coordinates");
                    writer.WriteStartArray();
                    for (int i = 0; i < multiPoint.NumGeometries; i++)
                        WriteCoordinates(writer, ((Point)multiPoint.GetGeometryN(i)).CoordinateSequence, false);
                    writer.WriteEndArray();
                    if (customCoordinateSerializeSettings)
                    {
                        ApplySerializerSettings(writer, currentSerializerSettings);
                    }

                    break;

                case LineString lineString:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.LineString));

                    if (customCoordinateSerializeSettings)
                    {
                        currentSerializerSettings = BackupSerializerSettings(writer);
                        ApplySerializerSettings(writer, _serializerSettingsForCoordinates);
                    }
                    writer.WritePropertyName("coordinates");
                    WriteCoordinates(writer, lineString.CoordinateSequence);
                    if (customCoordinateSerializeSettings)
                    {
                        ApplySerializerSettings(writer, currentSerializerSettings);
                    }

                    break;

                case MultiLineString multiLineString:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.MultiLineString));

                    if (customCoordinateSerializeSettings)
                    {
                        currentSerializerSettings = BackupSerializerSettings(writer);
                        ApplySerializerSettings(writer, _serializerSettingsForCoordinates);
                    }
                    writer.WritePropertyName("coordinates");
                    writer.WriteStartArray();
                    for (int i = 0; i < multiLineString.NumGeometries; i++)
                        WriteCoordinates(writer, ((LineString)multiLineString.GetGeometryN(i)).CoordinateSequence);
                    writer.WriteEndArray();
                    if (customCoordinateSerializeSettings)
                    {
                        ApplySerializerSettings(writer, currentSerializerSettings);
                    }

                    break;

                case Polygon polygon:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.Polygon));

                    if (customCoordinateSerializeSettings)
                    {
                        currentSerializerSettings = BackupSerializerSettings(writer);
                        ApplySerializerSettings(writer, _serializerSettingsForCoordinates);
                    }
                    writer.WritePropertyName("coordinates");
                    WritePolygonCoordinates(polygon);
                    if (customCoordinateSerializeSettings)
                    {
                        ApplySerializerSettings(writer, currentSerializerSettings);
                    }

                    break;

                case MultiPolygon multiPolygon:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.MultiPolygon));

                    if (customCoordinateSerializeSettings)
                    {
                        currentSerializerSettings = BackupSerializerSettings(writer);
                        ApplySerializerSettings(writer, _serializerSettingsForCoordinates);
                    }
                    writer.WritePropertyName("coordinates");
                    writer.WriteStartArray();
                    for (int i = 0; i < multiPolygon.NumGeometries; i++)
                        WritePolygonCoordinates((Polygon)multiPolygon.GetGeometryN(i));
                    writer.WriteEndArray();
                    if (customCoordinateSerializeSettings)
                    {
                        ApplySerializerSettings(writer, currentSerializerSettings);
                    }

                    break;

                // remember to always test this after all the other MultiX, because the others all
                // inherit from GeometryCollection!
                case GeometryCollection geometryCollection:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.GeometryCollection));

                    writer.WritePropertyName("geometries");
                    serializer.Serialize(writer, geometryCollection.Geometries);
                    break;

                default:
                    throw new ArgumentException("Unrecognized geometry", nameof(geom));
            }

            writer.WriteEndObject();

            // remainder of this method is some helper methods to make lazy Coordinate sequences...
            // be careful when moving these methods around: they reuse the Coordinate objects, in
            // order to reduce allocations (because Json.NET allocates a TON already), and so it's
            // only OK if consumers just loop over them like they do now (airbreather 2019-08-24).
            //IEnumerable<Coordinate> GetCoordinatesFromMultiPoint(MultiPoint multiPoint)
            //{
            //    var coord = Coordinates.Create(_dimension);
            //    foreach (Point pt in multiPoint.Geometries)
            //    {
            //        var seq = pt.CoordinateSequence;
            //        coord.X = seq.GetX(0);
            //        coord.Y = seq.GetY(0);
            //        if (_dimension > 2)
            //        {
            //            coord.Z = seq.GetZ(0);
            //        }

            //        yield return coord;
            //    }
            //}

            //IEnumerable<Coordinate> GetCoordinatesFromLineString(LineString lineString)
            //{
            //    var seq = lineString.CoordinateSequence;
            //    var coord = Coordinates.Create(_dimension);
            //    for (int i = 0, cnt = seq.Count; i < cnt; i++)
            //    {
            //        coord.X = seq.GetX(i);
            //        coord.Y = seq.GetY(i);
            //        if (_dimension > 2)
            //        {
            //            coord.Z = seq.GetZ(i);
            //        }

            //        yield return coord;
            //    }
            //}

            //IEnumerable<IEnumerable<Coordinate>> GetCoordinatesFromMultiLineString(MultiLineString multiLineString)
            //{
            //    foreach (LineString lineString in multiLineString.Geometries)
            //    {
            //        yield return GetCoordinatesFromLineString(lineString);
            //    }
            //}

            //IEnumerable<IEnumerable<Coordinate>> GetCoordinatesFromPolygon(Polygon polygon)
            //{
            //    var interiorRings = polygon.InteriorRings;
            //    var allRings = new LineString[interiorRings.Length + 1];
            //    allRings[0] = polygon.Shell;
            //    Array.Copy(interiorRings, 0, allRings, 1, interiorRings.Length);
            //    return allRings.Select(GetCoordinatesFromLineString);
            //}

            //IEnumerable<IEnumerable<IEnumerable<Coordinate>>> GetCoordinatesFromMultiPolygon(MultiPolygon multiPolygon)
            //{
            //    foreach (Polygon polygon in multiPolygon.Geometries)
            //    {
            //        yield return GetCoordinatesFromPolygon(polygon);
            //    }
            //}

            void WritePolygonCoordinates(Polygon polygon)
            {
                writer.WriteStartArray();
                if (!polygon.IsEmpty)
                {
                    WriteCoordinates(writer, polygon.ExteriorRing.CoordinateSequence,
                        orientation: _exteriorRingOrientation);

                    for (int i = 0; i < polygon.NumInteriorRings; i++)
                        WriteCoordinates(writer, polygon.GetInteriorRingN(i).CoordinateSequence,
                            orientation: _interiorRingOrientation);
                }
                writer.WriteEndArray();
            }
        }

        private List<object> ParseGeomCollection(JsonReader reader, JsonSerializer serializer)
        {
            var geometries = new List<object>();
            while (reader.Read())
            {
                // Exit if we are at the end
                if (reader.TokenType == JsonToken.EndArray)
                {
                    reader.ReadOrThrow();
                    break;
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    geometries.Add(ParseGeometry(reader, serializer));
                }
            }

            return geometries;
        }

        private Coordinate CreateCoordinate(JsonReader reader, List<object> list)
        {
            var c = Coordinates.Create(_dimension);
            c.X = Convert.ToDouble(list[0], reader.Culture);
            c.Y = Convert.ToDouble(list[1], reader.Culture);
            if (list.Count > 2 && _dimension > 2)
            {
                c.Z = Convert.ToDouble(list[2], reader.Culture);
            }

            if (double.IsNaN(c.X) && double.IsNaN(c.Y))
            {
                return null;
            }

            c.X = _factory.PrecisionModel.MakePrecise(c.X);
            c.Y = _factory.PrecisionModel.MakePrecise(c.Y);

            return c;
        }

        private Coordinate[] CreateCoordinateArray(JsonReader reader, List<object> list)
        {
            var coordinates = new List<Coordinate>(list.Count);
            foreach (List<object> coord in list)
            {
                if (CreateCoordinate(reader, coord) is Coordinate c)
                {
                    coordinates.Add(c);
                }
            }

            return coordinates.ToArray();
        }

        private Geometry ParseGeometry(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonReaderException("Expected token '{' not found");
            }

            // advance
            reader.ReadOrThrow();

            GeoJsonObjectType? geometryType = null;
            List<object> coords = null;
            while (reader.TokenType == JsonToken.PropertyName)
            {
                //read the tokens, type may come before coordinates or geometries as pr spec
                string prop = (string)reader.Value;
                switch (prop)
                {
                    case "type":
                        if (geometryType == null)
                        {
                            reader.ReadOrThrow();
                            geometryType = Enum.TryParse((string)reader.Value, true, out GeoJsonObjectType parsedGeometryType)
                                ? parsedGeometryType
                                : throw new JsonReaderException($"Unknown geometry type '{(string)reader.Value}'");
                            reader.ReadOrThrow();
                        }

                        break;

                    case "geometries":
                        //only geom collection has "geometries"
                        reader.ReadOrThrow(); //read past start array tag
                        if (reader.TokenType == JsonToken.Null)
                        {
                            reader.ReadOrThrow();
                        }
                        else
                        {
                            coords = ParseGeomCollection(reader, serializer);
                        }

                        break;

                    case "coordinates":
                        reader.ReadOrThrow(); //read past start array tag
                        if (reader.TokenType == JsonToken.Null)
                        {
                            reader.ReadOrThrow();
                        }
                        else
                        {
                            coords = ReadCoordinates(reader);
                        }

                        break;

                    default: // included "bbox" property
                        //read, but can't do anything with it
                        reader.Skip();
                        reader.ReadOrThrow();
                        break;
                }

                reader.SkipComments();
            }

            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new JsonReaderException("Expected token '}' not found");
            }

            bool CoordsIsNullOrEmpty() => coords is null || coords.Count == 0;

            switch (geometryType)
            {
                case GeoJsonObjectType.Point when CoordsIsNullOrEmpty():
                    return _factory.CreatePoint();

                case GeoJsonObjectType.Point:
                    return CreatePoint(reader, coords);

                case GeoJsonObjectType.MultiPoint when CoordsIsNullOrEmpty():
                    return _factory.CreateMultiPoint();

                case GeoJsonObjectType.MultiPoint:
                    return _factory.CreateMultiPoint(coords.Select(obj => CreatePoint(reader, (List<object>)obj))
                        .ToArray());

                case GeoJsonObjectType.LineString when CoordsIsNullOrEmpty():
                    return _factory.CreateLineString();

                case GeoJsonObjectType.LineString:
                    return CreateLineString(reader, coords);

                case GeoJsonObjectType.MultiLineString when CoordsIsNullOrEmpty():
                    return _factory.CreateMultiLineString();

                case GeoJsonObjectType.MultiLineString:
                    return _factory.CreateMultiLineString(coords
                        .Select(obj => CreateLineString(reader, (List<object>)obj)).ToArray());

                case GeoJsonObjectType.Polygon when CoordsIsNullOrEmpty():
                    return _factory.CreatePolygon();

                case GeoJsonObjectType.Polygon:
                    return CreatePolygon(reader, coords);

                case GeoJsonObjectType.MultiPolygon when CoordsIsNullOrEmpty():
                    return _factory.CreateMultiPolygon();

                case GeoJsonObjectType.MultiPolygon:
                    return _factory.CreateMultiPolygon(coords.Select(obj => CreatePolygon(reader, (List<object>)obj))
                        .ToArray());

                case GeoJsonObjectType.GeometryCollection when CoordsIsNullOrEmpty():
                    return _factory.CreateGeometryCollection();

                case GeoJsonObjectType.GeometryCollection:
                    return _factory.CreateGeometryCollection(coords.Cast<Geometry>().ToArray());

                default:
                    return null;
            }
        }

        /// <summary>
        /// Reads a geometry from its JSON representation.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            return ParseGeometry(reader, serializer);
        }

        private Point CreatePoint(JsonReader reader, List<object> list) =>
            _factory.CreatePoint(CreateCoordinate(reader, list));

        private LineString CreateLineString(JsonReader reader, List<object> list) =>
            _factory.CreateLineString(CreateCoordinateArray(reader, list));

        private Polygon CreatePolygon(JsonReader reader, List<object> list)
        {
            var shell = _factory.CreateLinearRing(CreateCoordinateArray(reader, (List<object>)list[0]));
            if (list.Count == 1)
            {
                return _factory.CreatePolygon(shell);
            }

            var holes = new LinearRing[list.Count - 1];
            for (int i = 0; i < holes.Length; i++)
            {
                holes[i] = _factory.CreateLinearRing(CreateCoordinateArray(reader, (List<object>)list[i + 1]));
            }

            return _factory.CreatePolygon(shell, holes);
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(Geometry).IsAssignableFrom(objectType);
        }
    }
}
