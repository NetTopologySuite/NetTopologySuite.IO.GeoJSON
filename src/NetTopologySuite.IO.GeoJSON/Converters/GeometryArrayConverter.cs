using System;
using System.Collections.Generic;
using System.Linq;

using NetTopologySuite.Geometries;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts an array of <see cref="Geometry"/>s to and from JSON
    /// </summary>
    public class GeometryArrayConverter : JsonConverter
    {
        private readonly GeometryFactory _factory;
        private readonly int _dimension;

        /// <summary>
        /// Creates an instance of this class using <see cref="GeoJsonSerializer.Wgs84Factory"/>
        /// </summary>
        public GeometryArrayConverter() : this(GeoJsonSerializer.Wgs84Factory) { }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/>
        /// </summary>
        /// <param name="factory">The factory</param>
        public GeometryArrayConverter(GeometryFactory factory)
            : this(factory, 2)
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="GeometryFactory"/>
        /// </summary>
        /// <param name="factory">The factory</param>
        /// <param name="dimension">The number of dimensions to handle.  Must be 2 or 3.</param>
        public GeometryArrayConverter(GeometryFactory factory, int dimension)
        {
            if (dimension != 2 && dimension != 3)
            {
                throw new ArgumentException("Must be either 2 or 3", nameof(dimension));
            }

            _factory = factory;
            _dimension = dimension;
        }

        /// <summary>
        /// Writes an array of <see cref="Geometry"/>s to JSON
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The geometry</param>
        /// <param name="serializer">The serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //moved to GeometryConverter:
            //writer.WritePropertyName("geometries");
            WriteGeometries(writer, (IEnumerable<Geometry>)value, serializer);
        }

        private static void WriteGeometries(JsonWriter writer, IEnumerable<Geometry> geometries, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (var geometry in geometries)
            {
                serializer.Serialize(writer, geometry);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// Reads an array of <see cref="Geometry"/>s from JSON
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="objectType">The object type</param>
        /// <param name="existingValue">The existing value</param>
        /// <param name="serializer">The serializer</param>
        /// <returns>The geometry array read</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new Exception();
            }

            reader.Read();

            var geoms = new List<Geometry>();
            while (reader.TokenType != JsonToken.EndArray)
            {
                var obj = (JObject)serializer.Deserialize(reader);
                var geometryType = (GeoJsonObjectType)Enum.Parse(typeof(GeoJsonObjectType), obj.Value<string>("type"), true);

                switch (geometryType)
                {
                    case GeoJsonObjectType.Point:
                        geoms.Add(_factory.CreatePoint(ToCoordinate(obj.Value<JArray>("coordinates"), reader.Culture)));
                        break;

                    case GeoJsonObjectType.LineString:
                        geoms.Add(_factory.CreateLineString(ToCoordinates(obj.Value<JArray>("coordinates"), reader.Culture)));
                        break;

                    case GeoJsonObjectType.Polygon:
                        geoms.Add(CreatePolygon(ToListOfCoordinates(obj.Value<JArray>("coordinates"), reader.Culture)));
                        break;

                    case GeoJsonObjectType.MultiPoint:
                        geoms.Add(_factory.CreateMultiPointFromCoords(ToCoordinates(obj.Value<JArray>("coordinates"), reader.Culture)));
                        break;

                    case GeoJsonObjectType.MultiLineString:
                        geoms.Add(CreateMultiLineString(ToListOfCoordinates(obj.Value<JArray>("coordinates"), reader.Culture)));
                        break;

                    case GeoJsonObjectType.MultiPolygon:
                        geoms.Add(CreateMultiPolygon(ToListOfListOfCoordinates(obj.Value<JArray>("coordinates"), reader.Culture)));
                        break;

                    case GeoJsonObjectType.GeometryCollection:
                        throw new NotSupportedException();
                }

                reader.Read();
            }

            return geoms;
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IEnumerable<Geometry>).IsAssignableFrom(objectType);
        }

        private MultiLineString CreateMultiLineString(IEnumerable<IEnumerable<Coordinate>> coordinates)
        {
            var strings = new List<LineString>();
            foreach (var seq in coordinates)
            {
                strings.Add(_factory.CreateLineString(seq as Coordinate[] ?? seq.ToArray()));
            }

            return _factory.CreateMultiLineString(strings.ToArray());
        }

        private Polygon CreatePolygon(IEnumerable<IEnumerable<Coordinate>> coordinates)
        {
            var allRings = new List<LinearRing>();
            foreach (var seq in coordinates)
            {
                allRings.Add(_factory.CreateLinearRing(seq as Coordinate[] ?? seq.ToArray()));
            }

            var shell = allRings[0];
            var holes = allRings.Skip(1).ToArray();
            return _factory.CreatePolygon(shell, holes);
        }

        private MultiPolygon CreateMultiPolygon(IEnumerable<IEnumerable<IEnumerable<Coordinate>>> coordinates)
        {
            var polygons = new List<Polygon>();
            foreach (var seq in coordinates)
            {
                polygons.Add(CreatePolygon(seq));
            }

            return _factory.CreateMultiPolygon(polygons.ToArray());
        }

        private Coordinate ToCoordinate(JArray array, IFormatProvider formatProvider)
        {
            var c = Coordinates.Create(_dimension);
            object[] jarray = array.Cast<JValue>().Select(i => i.Value).ToArray();
            c.X = _factory.PrecisionModel.MakePrecise(Convert.ToDouble(jarray[0], formatProvider));
            c.Y = _factory.PrecisionModel.MakePrecise(Convert.ToDouble(jarray[1], formatProvider));

            if (array.Count > 2 && _dimension > 2)
            {
                c.Z = Convert.ToDouble(array[2], formatProvider);
            }

            return c;
        }

        private Coordinate[] ToCoordinates(JArray array, IFormatProvider formatProvider)
        {
            var c = new Coordinate[array.Count];
            for (int i = 0; i < array.Count; i++)
            {
                c[i] = ToCoordinate((JArray)array[i], formatProvider);
            }

            return c;
        }

        private IEnumerable<Coordinate[]> ToListOfCoordinates(JArray array, IFormatProvider formatProvider)
        {
            for (int i = 0; i < array.Count; i++)
            {
                yield return ToCoordinates((JArray)array[i], formatProvider);
            }
        }

        private IEnumerable<IEnumerable<Coordinate[]>> ToListOfListOfCoordinates(JArray array, IFormatProvider formatProvider)
        {
            for (int i = 0; i < array.Count; i++)
            {
                yield return ToListOfCoordinates((JArray)array[i], formatProvider);
            }
        }
    }
}
