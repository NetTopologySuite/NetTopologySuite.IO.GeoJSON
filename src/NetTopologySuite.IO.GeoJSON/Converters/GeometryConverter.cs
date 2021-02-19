using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using NetTopologySuite.Geometries;

using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts a <see cref="Geometry"/> to and from its JSON representation
    /// </summary>
    public class GeometryConverter : JsonConverter
    {
        private readonly GeometryFactory _factory;
        private readonly int _dimension;
        private readonly bool _allowMeasurements;

        /// <summary>
        /// Creates an instance of this class using <see cref="GeoJsonSerializer.Wgs84Factory"/> to create geometries.
        /// </summary>
        public GeometryConverter() : this(GeoJsonSerializer.Wgs84Factory) { }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> to create geometries.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        public GeometryConverter(GeometryFactory geometryFactory) : this(geometryFactory, 2) { }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> to create geometries.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <param name="dimension">The number of dimensions to handle.  Must be between 2 and 4.</param>
        public GeometryConverter(GeometryFactory geometryFactory, int dimension) : this(geometryFactory, dimension, false) { }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/> to create geometries.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <param name="dimension">The number of dimensions to handle.  Must be between 2 and 4.</param>
        /// <param name="allowMeasurements">If the geometry allow measurement values or not, must be of dimension 3 or 4</param>
        public GeometryConverter(GeometryFactory geometryFactory, int dimension, bool allowMeasurements)
        {
            if (dimension < 2 || dimension > 4)
            {
                throw new ArgumentException("Must be between 2 and 4", nameof(dimension));
            }

            _factory = geometryFactory;
            _dimension = dimension;
            _allowMeasurements = allowMeasurements;
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

            bool writeCoordinateData = serializer.NullValueHandling == NullValueHandling.Include || !geom.IsEmpty;

            switch (geom)
            {
                case Point pt:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.Point));

                    if (writeCoordinateData)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, pt.Coordinate);
                    }

                    break;

                case MultiPoint multiPoint:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.MultiPoint));

                    if (writeCoordinateData)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, GetCoordinatesFromMultiPoint(multiPoint));
                    }

                    break;

                case LineString lineString:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.LineString));

                    if (writeCoordinateData)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, GetCoordinatesFromLineString(lineString));
                    }

                    break;

                case MultiLineString multiLineString:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.MultiLineString));

                    if (writeCoordinateData)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, GetCoordinatesFromMultiLineString(multiLineString));
                    }

                    break;

                case Polygon polygon:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.Polygon));

                    if (writeCoordinateData)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, GetCoordinatesFromPolygon(polygon));
                    }

                    break;

                case MultiPolygon multiPolygon:
                    writer.WritePropertyName("type");
                    writer.WriteValue(nameof(GeoJsonObjectType.MultiPolygon));

                    if (writeCoordinateData)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, GetCoordinatesFromMultiPolygon(multiPolygon));
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
            IEnumerable<Coordinate> GetCoordinatesFromMultiPoint(MultiPoint multiPoint)
            {
                int measures = _allowMeasurements ? 1 : 0;
                var coord = Coordinates.Create(_dimension, measures);
                foreach (Point pt in multiPoint.Geometries)
                {
                    var seq = pt.CoordinateSequence;
                    PopulateCoordinate(ref coord, seq, 0);

                    yield return coord;
                }
            }

            IEnumerable<Coordinate> GetCoordinatesFromLineString(LineString lineString)
            {
                int measures = _allowMeasurements ? 1 : 0;
                var seq = lineString.CoordinateSequence;
                var coord = Coordinates.Create(_dimension, measures); //todo, dynamic measures
                for (int i = 0, cnt = seq.Count; i < cnt; i++)
                {
                    PopulateCoordinate(ref coord, seq, i);

                    yield return coord;
                }
            }

            IEnumerable<IEnumerable<Coordinate>> GetCoordinatesFromMultiLineString(MultiLineString multiLineString)
            {
                foreach (LineString lineString in multiLineString.Geometries)
                {
                    yield return GetCoordinatesFromLineString(lineString);
                }
            }

            IEnumerable<IEnumerable<Coordinate>> GetCoordinatesFromPolygon(Polygon polygon)
            {
                var interiorRings = polygon.InteriorRings;
                var allRings = new LineString[interiorRings.Length + 1];
                allRings[0] = polygon.Shell;
                Array.Copy(interiorRings, 0, allRings, 1, interiorRings.Length);
                return allRings.Select(GetCoordinatesFromLineString);
            }

            IEnumerable<IEnumerable<IEnumerable<Coordinate>>> GetCoordinatesFromMultiPolygon(MultiPolygon multiPolygon)
            {
                foreach (Polygon polygon in multiPolygon.Geometries)
                {
                    yield return GetCoordinatesFromPolygon(polygon);
                }
            }
        }

        private static void PopulateCoordinate(ref Coordinate coord, CoordinateSequence seq, int i)
        {
            coord.X = seq.GetX(i);
            coord.Y = seq.GetY(i);
            if (coord.GetType() == typeof(CoordinateZ))
            {
                coord.Z = seq.GetZ(i);
            }
            else if (coord.GetType() == typeof(CoordinateM))
            {
                coord.M = seq.GetM(i);
            }
            else if (coord.GetType() == typeof(CoordinateZM))
            {
                coord.Z = seq.GetZ(i);
                coord.M = seq.GetM(i);
            }
        }

        private List<object> ReadCoordinates(JsonReader reader)
        {
            var coords = new List<object>(3);
            bool startArray = reader.TokenType == JsonToken.StartArray;
            reader.ReadOrThrow();

            while (reader.TokenType != JsonToken.EndArray)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartArray:
                        coords.Add(ReadCoordinates(reader));
                        break;

                    case JsonToken.Integer:
                    case JsonToken.Float:
                        coords.Add(reader.Value);
                        reader.ReadOrThrow();
                        break;

                    case JsonToken.Null:
                        coords.Add(Coordinate.NullOrdinate);
                        reader.ReadOrThrow();
                        break;

                    default:
                        reader.ReadOrThrow();
                        break;
                }
            }

            if (startArray)
            {
                Debug.Assert(reader.TokenType == JsonToken.EndArray);
                reader.ReadOrThrow();
            }

            return coords;
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
                throw new JsonReaderException("Expected Start object '{' Token");
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
                            geometryType = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), (string)reader.Value, true);
                            reader.ReadOrThrow();
                        }

                        break;

                    case "geometries":
                        //only geom collection has "geometries"
                        reader.ReadOrThrow();  //read past start array tag
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

                    case "bbox":
                        // Read, but can't do anything with it, assigning Envelopes is impossible without reflection
                        /*var bbox = */
                        serializer.Deserialize<Envelope>(reader);
                        break;

                    default:
                        reader.ReadOrThrow();
                        /*var item = */
                        serializer.Deserialize(reader);
                        break;
                }

                reader.SkipComments();
            }

            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new ArgumentException("Expected token '}' not found.");
            }

            switch (geometryType)
            {
                case GeoJsonObjectType.Point when coords is null:
                    return _factory.CreatePoint();

                case GeoJsonObjectType.Point:
                    return CreatePoint(reader, coords);

                case GeoJsonObjectType.MultiPoint when coords is null:
                    return _factory.CreateMultiPoint();

                case GeoJsonObjectType.MultiPoint:
                    return _factory.CreateMultiPoint(coords.Select(obj => CreatePoint(reader, (List<object>)obj)).ToArray());

                case GeoJsonObjectType.LineString when coords is null:
                    return _factory.CreateLineString();

                case GeoJsonObjectType.LineString:
                    return CreateLineString(reader, coords);

                case GeoJsonObjectType.MultiLineString when coords is null:
                    return _factory.CreateMultiLineString();

                case GeoJsonObjectType.MultiLineString:
                    return _factory.CreateMultiLineString(coords.Select(obj => CreateLineString(reader, (List<object>)obj)).ToArray());

                case GeoJsonObjectType.Polygon when coords is null:
                    return _factory.CreatePolygon();

                case GeoJsonObjectType.Polygon:
                    return CreatePolygon(reader, coords);

                case GeoJsonObjectType.MultiPolygon when coords is null:
                    return _factory.CreateMultiPolygon();

                case GeoJsonObjectType.MultiPolygon:
                    return _factory.CreateMultiPolygon(coords.Select(obj => CreatePolygon(reader, (List<object>)obj)).ToArray());

                case GeoJsonObjectType.GeometryCollection when coords is null:
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
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ParseGeometry(reader, serializer);
        }

        private Point CreatePoint(JsonReader reader, List<object> list) => _factory.CreatePoint(CreateCoordinate(reader, list));

        private LineString CreateLineString(JsonReader reader, List<object> list) => _factory.CreateLineString(CreateCoordinateArray(reader, list));

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
