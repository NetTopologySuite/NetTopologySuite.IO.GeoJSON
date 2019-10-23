using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace NetTopologySuite.IO.Converters
{
    public partial class StjGeometryConverter
    {
        private MemoryStream ReadCoordinateData(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            reader.ReadToken(JsonTokenType.StartArray);
            var res = new MemoryStream();
            res.Write(System.Text.Encoding.UTF8.GetBytes("["), 0, 1);
            int openBrackets = 1;

            bool wasCloseBracket = false;
            bool addComma = false;
            byte bytComma = System.Text.Encoding.UTF8.GetBytes(",")[0];
            while (openBrackets > 0)
            {
                // add a comma to separate arrays
                if (reader.TokenType == JsonTokenType.StartArray)
                {
                    if (wasCloseBracket)
                        res.WriteByte(bytComma);
                    addComma = false;
                    openBrackets++;
                }

                if (reader.TokenType == JsonTokenType.Number && addComma)
                    res.WriteByte(bytComma);

                byte[] seq = reader.HasValueSequence
                    ? reader.ValueSequence.ToArray()
                    : reader.ValueSpan.ToArray();
                res.Write(seq, 0, seq.Length);

                addComma = reader.TokenType == JsonTokenType.Number;

                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    openBrackets--;
                    addComma = false;
                    wasCloseBracket = true;
                }
                else
                {
                    wasCloseBracket = false;
                }

                if (!reader.Read())
                    throw new JsonException();
            }

            //reader.ReadToken(JsonTokenType.EndArray);
            //res.Write(System.Text.Encoding.UTF8.GetBytes("]"), 0, 1);

            Console.WriteLine(System.Text.Encoding.UTF8.GetString(res.ToArray()));
            return res;
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

        private void WriteCoordinateSequence(Utf8JsonWriter writer, CoordinateSequence sequence, JsonSerializerOptions options, bool multiple = true)
        {
            //writer.WritePropertyName("coordinates");
            if (sequence == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (multiple)
                writer.WriteStartArray();

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
