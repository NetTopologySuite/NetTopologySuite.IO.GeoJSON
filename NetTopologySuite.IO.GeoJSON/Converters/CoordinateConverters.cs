using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Convertes a <see cref="Coordinate"/> to and from JSON
    /// </summary>
    public class CoordinateConverter : JsonConverter
    {
        private readonly IPrecisionModel _precisionModel;
        private readonly int _dimension;

        /// <summary>
        /// Creates an instance of this class using a floating precision model and <see cref="GeoJsonSerializer.DefaultDimension"/> output dimensions
        /// </summary>
        internal CoordinateConverter()
            : this(GeometryFactory.Floating.PrecisionModel)
        { }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="precisionModel">The precision model to use for writing</param>
        /// <param name="dimension">The number of dimensions</param>
        internal CoordinateConverter(IPrecisionModel precisionModel, int dimension = GeoJsonSerializer.DefaultDimension)
        {
            _precisionModel = precisionModel;
            _dimension = dimension;
        }

        /// <summary>
        /// Writes a coordinate, a coordinate sequence or an enumeration of coordinates to JSON
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The coordinate</param>
        /// <param name="serializer">The serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteToken(JsonToken.Null);
                return;
            }

            List<List<Coordinate[]>> coordinatesss = value as List<List<Coordinate[]>>;
            if (coordinatesss != null)
            {
                WriteJsonCoordinatesEnumerable2(writer, coordinatesss, serializer);
                return;
            }

            List<Coordinate[]> coordinatess = value as List<Coordinate[]>;
            if (coordinatess != null)
            {
                WriteJsonCoordinatesEnumerable(writer, coordinatess, serializer);
                return;
            }

            IEnumerable<Coordinate> coordinates = value as IEnumerable<Coordinate>;
            if (coordinates != null)
            {
                WriteJsonCoordinates(writer, coordinates, serializer);
                return;
            }

            Coordinate coordinate = value as Coordinate;
            if (coordinate != null)
                WriteJsonCoordinate(writer, coordinate, serializer);

        }

        /// <summary>
        /// Writes a single coordinate to JSON
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="coordinate">The coordinate</param>
        /// <param name="serializer">The serializer</param>
        protected void WriteJsonCoordinate(JsonWriter writer, Coordinate coordinate, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            double value = _precisionModel.MakePrecise(coordinate.X);
            writer.WriteValue(value);
            value = _precisionModel.MakePrecise(coordinate.Y);
            writer.WriteValue(value);

            if (_dimension > 2 && !double.IsNaN(coordinate.Z))
                writer.WriteValue(coordinate.Z);

            writer.WriteEndArray();
        }

        private void WriteJsonCoordinates(JsonWriter writer, IEnumerable<Coordinate> coordinates, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (Coordinate coordinate in coordinates)
                WriteJsonCoordinate(writer, coordinate, serializer);
            writer.WriteEndArray();
        }

        private void WriteJsonCoordinatesEnumerable(JsonWriter writer, IEnumerable<Coordinate[]> coordinates, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (Coordinate[] coordinate in coordinates)
                WriteJsonCoordinates(writer, coordinate, serializer);
            writer.WriteEndArray();
        }

        private void WriteJsonCoordinatesEnumerable2(JsonWriter writer, List<List<Coordinate[]>> coordinates, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            foreach (List<Coordinate[]> coordinate in coordinates)
                WriteJsonCoordinatesEnumerable(writer, coordinate, serializer);
            writer.WriteEndArray();
        }

        /// <summary>
        /// Reads a coordinate, a coordinate sequence or an enumeration of coordinates from JSON
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();

            Debug.Assert(reader.TokenType == JsonToken.PropertyName);
            Debug.Assert((string)reader.Value == "coordinates");

            object result;
            if (objectType == typeof(Coordinate))
                result = ReadJsonCoordinate(reader);
            else if (typeof(IEnumerable<Coordinate>).IsAssignableFrom(objectType))
                result = ReadJsonCoordinates(reader);
            else if (typeof(List<Coordinate[]>).IsAssignableFrom(objectType))
                result = ReadJsonCoordinatesEnumerable(reader);
            else if (typeof(List<List<Coordinate[]>>).IsAssignableFrom(objectType))
                result = ReadJsonCoordinatesEnumerable2(reader);
            else throw new ArgumentException("unmanaged type: " + objectType);
            reader.Read();
            return result;

        }

        private Coordinate ReadJsonCoordinate(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray)
                return null;

            var c = new Coordinate();

            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer);
            c.X = _precisionModel.MakePrecise(Convert.ToDouble(reader.Value));

            reader.Read();
            Debug.Assert(reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer);
            c.Y = _precisionModel.MakePrecise(Convert.ToDouble(reader.Value));

            reader.Read();
            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
            {
                double value = Convert.ToDouble(reader.Value);
                if (_dimension > 2)
                    c.Z = value;
                reader.Read();
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);
            return c;
        }

        private Coordinate[] ReadJsonCoordinates(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray) return null;

            List<Coordinate> coordinates = new List<Coordinate>();
            while (true)
            {
                Coordinate c = ReadJsonCoordinate(reader);
                if (c == null) break;
                coordinates.Add(c);
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);
            return coordinates.ToArray();
        }

        private List<Coordinate[]> ReadJsonCoordinatesEnumerable(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray) return null;

            List<Coordinate[]> coordinates = new List<Coordinate[]>();
            while (true)
            {
                Coordinate[] res = ReadJsonCoordinates(reader);
                if (res == null) break;
                coordinates.Add(res);
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);
            return coordinates;
        }

        private List<List<Coordinate[]>> ReadJsonCoordinatesEnumerable2(JsonReader reader)
        {
            reader.Read();
            if (reader.TokenType != JsonToken.StartArray) return null;
            List<List<Coordinate[]>> coordinates = new List<List<Coordinate[]>>();

            while (true)
            {
                List<Coordinate[]> res = ReadJsonCoordinatesEnumerable(reader);
                if (res == null) break;
                coordinates.Add(res);
            }
            Debug.Assert(reader.TokenType == JsonToken.EndArray);
            return coordinates;
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Coordinate) ||
                   objectType == typeof(Coordinate[]) ||
                   objectType == typeof(List<Coordinate[]>) ||
                   objectType == typeof(List<List<Coordinate[]>>) ||
                   typeof(IEnumerable<Coordinate>).IsAssignableFrom(objectType) ||
                   typeof(IEnumerable<IEnumerable<Coordinate>>).IsAssignableFrom(objectType) ||
                   typeof(IEnumerable<IEnumerable<IEnumerable<Coordinate>>>).IsAssignableFrom(objectType);
        }
    }
}
