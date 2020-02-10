using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    public partial class StjGeometryConverter
    {
        private static int GetCoordinateDataSize(Utf8JsonReader reader)
        {
            long start = reader.TokenStartIndex;
            reader.Skip();
            return (int) (reader.TokenStartIndex - start);
        }

        /// <summary>
        /// Reads the coordinate data
        /// </summary>
        /// <param name="reader">the reader</param>
        /// <param name="pool">A pool to rent buffers from.</param>
        /// <returns>A <b>rented</b> buffer. Make sure to return it!</returns>
        private static byte[] ReadCoordinateData(ref Utf8JsonReader reader, ArrayPool<byte> pool)
        {
            byte[] buffer = pool.Rent(GetCoordinateDataSize(reader) + 1);
            var span = new Span<byte>(buffer);
            int position = 0;

            bool wasNumber = false;
            bool wasEndArray = false;
            int depth = reader.CurrentDepth;
            do
            {
                if (wasNumber && reader.TokenType == JsonTokenType.Number)
                    buffer[position++] = 44; //System.Text.Encoding.UTF8.GetBytes(",")[0];
                else if (wasEndArray && reader.TokenType == JsonTokenType.StartArray)
                    buffer[position++] = 44; //System.Text.Encoding.UTF8.GetBytes(",")[0];

                if (reader.HasValueSequence)
                {
                    reader.ValueSequence.CopyTo(span.Slice(position, (int)reader.ValueSequence.Length));
                    position += (int)reader.ValueSequence.Length;
                }
                else
                {
                    reader.ValueSpan.CopyTo(span.Slice(position, reader.ValueSpan.Length));
                    position += reader.ValueSpan.Length;
                }

                wasNumber = reader.TokenType == JsonTokenType.Number;
                wasEndArray = reader.TokenType == JsonTokenType.EndArray;

                reader.Read();

            } while (depth < reader.CurrentDepth);

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                buffer[position++] = 93; //System.Text.Encoding.UTF8.GetBytes("]")[0];
                reader.Read();
            }

            // fill tail spaces
            while (position < buffer.Length)
                buffer[position++] = 32;

            return span.ToArray();

        }

        private Coordinate ReadCoordinate(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.ReadToken(JsonTokenType.StartArray);

            Coordinate res;

            double lon = _geometryFactory.PrecisionModel.MakePrecise(reader.GetDouble());
            reader.Read();
            double lat = _geometryFactory.PrecisionModel.MakePrecise(reader.GetDouble());
            reader.Read();

            if (reader.TokenType == JsonTokenType.Number)
            {
                res = new CoordinateZ(lon, lat, reader.GetDouble());
                reader.Read();
            }
            else
            {
                res = new Coordinate(lon, lat);
            }

            // Skip other ordinates that we are not forced to handle
            while (reader.TokenType == JsonTokenType.Number)
                reader.Read();

            reader.ReadToken(JsonTokenType.EndArray);

            return res;
        }

        private CoordinateSequence ReadCoordinateSequence(ref Utf8JsonReader reader, JsonSerializerOptions options,
            bool justOne)
        {
            if (justOne)
                return _geometryFactory.CoordinateSequenceFactory.Create(new[] {ReadCoordinate(ref reader, options)});

            reader.ReadToken(JsonTokenType.StartArray);

            var coordinates = new List<Coordinate>();
            while (reader.TokenType != JsonTokenType.EndArray)
                coordinates.Add(ReadCoordinate(ref reader, options));

            reader.ReadToken(JsonTokenType.EndArray);

            return _geometryFactory.CoordinateSequenceFactory.Create(coordinates.ToArray());
        }

        private CoordinateSequence[] ReadCoordinateSequences1(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {

            reader.ReadToken(JsonTokenType.StartArray);

            var coordinates = new List<CoordinateSequence>();
            while (reader.TokenType != JsonTokenType.EndArray)
                coordinates.Add(ReadCoordinateSequence(ref reader, options, false));

            reader.ReadToken(JsonTokenType.EndArray);

            return coordinates.ToArray();
        }

        private CoordinateSequence[][] ReadCoordinateSequences2(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {

            reader.ReadToken(JsonTokenType.StartArray);

            var coordinates = new List<CoordinateSequence[]>();
            while (reader.TokenType != JsonTokenType.EndArray)
                coordinates.Add(ReadCoordinateSequences1(ref reader, options));

            reader.ReadToken(JsonTokenType.EndArray);

            return coordinates.ToArray();
        }

        private void WriteCoordinateSequence(Utf8JsonWriter writer, CoordinateSequence sequence, JsonSerializerOptions options, bool multiple = true, OrientationIndex orientation = OrientationIndex.None)
        {
            //writer.WritePropertyName("coordinates");
            if (sequence == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (multiple)
            { 
                writer.WriteStartArray();
                if (orientation == OrientationIndex.Clockwise && Orientation.IsCCW(sequence) ||
                    orientation == OrientationIndex.CounterClockwise && !Orientation.IsCCW(sequence))
                {
                    CoordinateSequences.Reverse(sequence);
                }
            }

            bool hasZ = sequence.HasZ;
            for (int i = 0; i < sequence.Count; i++)
            {
                writer.WriteStartArray();
                writer.WriteNumberValue(sequence.GetX(i));
                writer.WriteNumberValue(sequence.GetY(i));

                if (hasZ)
                {
                    double z = sequence.GetZ(i);
                    if (!double.IsNaN(z))
                        writer.WriteNumberValue(sequence.GetZ(i));
                }
                writer.WriteEndArray();

                if (!multiple) break;
            }

            if (multiple)
                writer.WriteEndArray();
        }
    }
}
