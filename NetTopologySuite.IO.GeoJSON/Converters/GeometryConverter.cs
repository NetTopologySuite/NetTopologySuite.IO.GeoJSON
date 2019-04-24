using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts a <see cref="IGeometry"/> to and from its JSON representation
    /// </summary>
    public class GeometryConverter : JsonConverter
    {
        private readonly IGeometryFactory _factory;
        private readonly int _dimension;
        /// <summary>
        /// Creates an instance of this class using <see cref="GeoJsonSerializer.Wgs84Factory"/> to create geometries.
        /// </summary>
        public GeometryConverter() : this(GeoJsonSerializer.Wgs84Factory) { }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="IGeometryFactory"/> to create geometries.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        public GeometryConverter(IGeometryFactory geometryFactory) : this(geometryFactory, GeoJsonSerializer.DefaultDimension) { }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="IGeometryFactory"/> to create geometries.
        /// </summary>
        /// <param name="geometryFactory">The geometry factory.</param>
        /// <param name="dimension">The number of dimension to handle. Valid input are 2 and 3</param>
        public GeometryConverter(IGeometryFactory geometryFactory, int dimension)
        {
            _factory = geometryFactory;
            _dimension = dimension;
        }

        /// <summary>
        /// Writes a geometry to its JSON representation
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The value</param>
        /// <param name="serializer">The serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var geom = value as IGeometry;
            if (geom == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            var geomType = ToGeoJsonObject(geom);
            writer.WritePropertyName("type");
            writer.WriteValue(Enum.GetName(typeof(GeoJsonObjectType), geomType));

            switch (geomType)
            {
                case GeoJsonObjectType.Point:
                    if (serializer.NullValueHandling == NullValueHandling.Include || geom.Coordinate != null)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, geom.Coordinate);
                    }
                    break;
                case GeoJsonObjectType.LineString:
                case GeoJsonObjectType.MultiPoint:
                    var linealCoords = geom.Coordinates;
                    if (serializer.NullValueHandling == NullValueHandling.Include || linealCoords != null)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, linealCoords);
                    }
                    break;
                case GeoJsonObjectType.Polygon:
                    var poly = geom as IPolygon;
                    Debug.Assert(poly != null);
                    var polygonCoords = PolygonCoordinates(poly);
                    if (serializer.NullValueHandling == NullValueHandling.Include || polygonCoords != null)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, polygonCoords);
                    }
                    break;

                case GeoJsonObjectType.MultiPolygon:
                    var mpoly = geom as IMultiPolygon;
                    Debug.Assert(mpoly != null);
                    var list = new List<List<Coordinate[]>>();
                    for (int i = 0; i < mpoly.NumGeometries; i++)
                        list.Add(PolygonCoordinates((IPolygon)mpoly.GetGeometryN(i)));
                    if (serializer.NullValueHandling == NullValueHandling.Include || list.Count > 0)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, list);
                    }
                    break;

                case GeoJsonObjectType.GeometryCollection:
                    var gc = geom as IGeometryCollection;
                    Debug.Assert(gc != null);
                    serializer.Serialize(writer, gc.Geometries);
                    break;
                default:
                    var coordinates = new List<Coordinate[]>();
                    foreach (var geometry in ((IGeometryCollection)geom).Geometries)
                        coordinates.Add(geometry.Coordinates);
                    if (serializer.NullValueHandling == NullValueHandling.Include || coordinates.Count > 0)
                    {
                        writer.WritePropertyName("coordinates");
                        serializer.Serialize(writer, coordinates);
                    }
                    break;
            }

            writer.WriteEndObject();
        }

        private GeoJsonObjectType ToGeoJsonObject(IGeometry geom)
        {
            if (geom is IPoint)
                return GeoJsonObjectType.Point;
            if (geom is ILineString)
                return GeoJsonObjectType.LineString;
            if (geom is IPolygon)
                return GeoJsonObjectType.Polygon;
            if (geom is IMultiPoint)
                return GeoJsonObjectType.MultiPoint;
            if (geom is IMultiLineString)
                return GeoJsonObjectType.MultiLineString;
            if (geom is IMultiPolygon)
                return GeoJsonObjectType.MultiPolygon;
            if (geom is IGeometryCollection)
                return GeoJsonObjectType.GeometryCollection;
            throw new ArgumentException("geom");
        }

        private static GeoJsonObjectType GetType(JsonReader reader)
        {
            var res = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), (string)reader.Value, true);
            reader.Read();
            return res;
        }

        private static List<object> ReadCoordinates(JsonReader reader)
        {
            var coords = new List<object>(3);
            bool startArray = reader.TokenType == JsonToken.StartArray;
            reader.Read();

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
                        reader.Read();
                        break;
                    case JsonToken.Null:
                        coords.Add(Coordinate.NullOrdinate);
                        reader.Read();
                        break;
                    default:
                        reader.Read();
                        break;
                }
                /*
                if (reader.TokenType == JsonToken.StartArray)
                {
                    coords.Add(ReadCoordinates(reader));
                }
                else if (reader.TokenType == JsonToken.Integer ||
                         reader.TokenType == JsonToken.Float ||
                         reader.TokenType == JsonToken.Null)
                {
                    coords.Add(reader.Value);
                    reader.Read();
                }
                else
                {
                    reader.Read();
                }*/
            }

            if (startArray)
            {
                Debug.Assert(reader.TokenType == JsonToken.EndArray);
                reader.Read();
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
                    reader.Read();
                    break;
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    geometries.Add(ParseGeometry(reader, serializer));
                }
            }
            return geometries;
        }

        private Coordinate GetPointCoordinate(IList list)
        {
            var c = new Coordinate();
            if (double.IsNaN(Convert.ToDouble(list[0])) && double.IsNaN(Convert.ToDouble(list[1])))
                return null;

            c.X = _factory.PrecisionModel.MakePrecise(Convert.ToDouble(list[0]));
            c.Y = _factory.PrecisionModel.MakePrecise(Convert.ToDouble(list[1]));

            if (list.Count > 2 && _dimension > 2)
                c.Z = Convert.ToDouble(list[2]);
            return c;
        }

        private Coordinate[] GetLineStringCoordinates(IEnumerable list)
        {
            var coordinates = new List<Coordinate>();
            foreach (List<object> coord in list)
            {
                var c = GetPointCoordinate(coord);
                if (c != null) coordinates.Add(c);
            }
            return coordinates.ToArray();
        }

        private List<Coordinate[]> GetPolygonCoordinates(IEnumerable list)
        {
            var coordinates = new List<Coordinate[]>();
            foreach (List<object> coord in list)
            {
                coordinates.Add(GetLineStringCoordinates(coord));
            }
            return coordinates;
        }

        private IEnumerable<List<Coordinate[]>> GetMultiPolygonCoordinates(IEnumerable list)
        {
            var coordinates = new List<List<Coordinate[]>>();
            foreach (List<object> coord in list)
            {
                coordinates.Add(GetPolygonCoordinates(coord));
            }
            return coordinates;
        }

        private static IGeometry[] GetGeometries(IEnumerable list)
        {
            var geometries = new List<IGeometry>();
            foreach (IGeometry geom in list)
            {
                geometries.Add(geom);
            }
            return geometries.ToArray();
        }

        private IGeometry ParseGeometry(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonReaderException("Expected Start object '{' Token");

            // advance
            bool read = reader.Read();

            Utility.SkipComments(reader);

            GeoJsonObjectType? geometryType = null;
            List<object> coords = null;
            while (reader.TokenType == JsonToken.PropertyName)
            {
                //read the tokens, type may come before coordinates or geometries as pr spec
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string prop = (string)reader.Value;
                    switch (prop)
                    {
                        case "type":
                            if (geometryType == null)
                            {
                                reader.Read();
                                geometryType = GetType(reader);
                            }
                            break;
                        case "geometries":
                            //only geom collection has "geometries"
                            reader.Read();  //read past start array tag                        
                            coords = ParseGeomCollection(reader, serializer);
                            break;
                        case "coordinates":
                            reader.Read(); //read past start array tag
                            coords = ReadCoordinates(reader);
                            break;
                        case "bbox":
                            // Read, but can't do anything with it, assigning Envelopes is impossible without reflection
                            /*var bbox = */
                            serializer.Deserialize<Envelope>(reader);
                            break;

                        default:
                            reader.Read();
                            /*var item = */
                            serializer.Deserialize(reader);
                            reader.Read();
                            break;

                    }
                }
                Utility.SkipComments(reader);

            }

            if (read && reader.TokenType != JsonToken.EndObject)
                throw new ArgumentException("Expected token '}' not found.");

            if (coords == null || geometryType == null)
            {
                return null;
            }

            switch (geometryType)
            {
                case GeoJsonObjectType.Point:
                    return _factory.CreatePoint(GetPointCoordinate(coords));
                case GeoJsonObjectType.LineString:
                    return _factory.CreateLineString(GetLineStringCoordinates(coords));
                case GeoJsonObjectType.Polygon:
                    return CreatePolygon(GetPolygonCoordinates(coords));
                case GeoJsonObjectType.MultiPoint:
                    return _factory.CreateMultiPointFromCoords(GetLineStringCoordinates(coords));
                case GeoJsonObjectType.MultiLineString:
                    var strings = new List<ILineString>();
                    foreach (var multiLineStringCoordinate in GetPolygonCoordinates(coords))
                    {
                        strings.Add(_factory.CreateLineString(multiLineStringCoordinate));
                    }
                    return _factory.CreateMultiLineString(strings.ToArray());
                case GeoJsonObjectType.MultiPolygon:
                    var polygons = new List<IPolygon>();
                    foreach (var multiPolygonCoordinate in GetMultiPolygonCoordinates(coords))
                    {
                        polygons.Add(CreatePolygon(multiPolygonCoordinate));
                    }
                    return _factory.CreateMultiPolygon(polygons.ToArray());
                case GeoJsonObjectType.GeometryCollection:
                    return _factory.CreateGeometryCollection(GetGeometries(coords));
            }
            return null;
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

        private static List<Coordinate[]> PolygonCoordinates(IPolygon polygon)
        {
            var res = new List<Coordinate[]>();
            res.Add(polygon.Shell.Coordinates);
            foreach (var interiorRing in polygon.InteriorRings)
                res.Add(interiorRing.Coordinates);
            return res;
        }

        private IPolygon CreatePolygon(IList<Coordinate[]> coordinatess)
        {
            var shell = _factory.CreateLinearRing(coordinatess[0]);
            var rings = new List<ILinearRing>();
            for (int i = 1; i < coordinatess.Count; i++)
                rings.Add(_factory.CreateLinearRing(coordinatess[i]));
            return _factory.CreatePolygon(shell, rings.ToArray());
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IGeometry).IsAssignableFrom(objectType);
        }
    }
}
