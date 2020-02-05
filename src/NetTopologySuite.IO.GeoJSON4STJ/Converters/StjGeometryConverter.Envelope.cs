using System.Diagnostics;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    public partial class StjGeometryConverter
    {
        internal Envelope ReadBBox(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            Envelope res = null;

            if (reader.TokenType != JsonTokenType.Null)
            {
                reader.ReadToken(JsonTokenType.StartArray);

                double minX = reader.GetDouble();
                reader.Read();
                double minY = reader.GetDouble();
                reader.Read();
                double maxX = reader.GetDouble();
                reader.Read();
                double maxY = reader.GetDouble();
                reader.Read();

                if (reader.TokenType == JsonTokenType.Number)
                {
                    maxX = maxY;
                    maxY = reader.GetDouble();
                    reader.Read();
                    reader.Read();
                }

                reader.ReadToken(JsonTokenType.EndArray);

                res = new Envelope(minX, maxX, minY, maxY);
            }

            reader.Read(); // move away from array end
            return res;
        }

        internal void WriteBBox(Utf8JsonWriter writer, Envelope value, JsonSerializerOptions options)
        {
            writer.WritePropertyName("bbox");

            if (!(value is Envelope envelope))
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            writer.WriteNumberValue(envelope.MinX);
            writer.WriteNumberValue(envelope.MinY);
            writer.WriteNumberValue(envelope.MaxX);
            writer.WriteNumberValue(envelope.MaxY);
            writer.WriteEndArray();
        }
    }
}
