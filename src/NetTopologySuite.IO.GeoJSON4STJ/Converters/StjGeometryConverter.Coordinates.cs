﻿using System.Text.Json;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    internal partial class StjGeometryConverter
    {
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
                    sequence = sequence.Reversed();
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
