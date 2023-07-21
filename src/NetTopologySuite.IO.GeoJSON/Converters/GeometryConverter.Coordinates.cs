using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Converters
{
    partial class GeometryConverter
    {
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

        private void WriteCoordinates(JsonWriter writer, CoordinateSequence sequence,
            bool multiple = true, OrientationIndex orientation = OrientationIndex.None)
        {
            //writer.WritePropertyName("coordinates");
            if (sequence == null || sequence.Count == 0)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
                return;
            }

            if (multiple)
            {
                if (orientation == OrientationIndex.Clockwise && Orientation.IsCCW(sequence) ||
                    orientation == OrientationIndex.CounterClockwise && !Orientation.IsCCW(sequence))
                {
                    sequence = sequence.Reversed();
                }
                writer.WriteStartArray();
            }

            bool hasZ = sequence.HasZ && _dimension > 2;
            for (int i = 0; i < sequence.Count; i++)
            {
                writer.WriteStartArray();
                double value = _factory.PrecisionModel.MakePrecise(sequence.GetX(i));
                writer.WriteValue(value);
                value = _factory.PrecisionModel.MakePrecise(sequence.GetY(i));
                writer.WriteValue(value);

                if (hasZ)
                {
                    double z = sequence.GetZ(i);
                    if (!double.IsNaN(z))
                        writer.WriteValue(sequence.GetZ(i));
                }
                writer.WriteEndArray();

                if (!multiple) break;
            }

            if (multiple)
                writer.WriteEndArray();
        }

    }
}
