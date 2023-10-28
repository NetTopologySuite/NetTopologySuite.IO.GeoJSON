using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetTopologySuite.IO.Converters
{
    internal class StjEnvelopeConverter : JsonConverter<Envelope>
    {
        public override Envelope Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Envelope res = null;

            if (reader.TokenType == JsonTokenType.Null)
            {
                // #57: callers expect us to have read past the last token
                reader.Read();
            }
            else
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

            //reader.Read(); // move away from array end
            return res;
        }

        public override void Write(Utf8JsonWriter writer, Envelope value, JsonSerializerOptions options)
        {
            // if we don't want to write "null" bounding boxes, bail out.
            if (value?.IsNull != false)
            {
                writer.WriteNullValue();

                return;
            }

            writer.WriteStartArray();
            writer.WriteNumberValue(value.MinX);
            writer.WriteNumberValue(value.MinY);
            writer.WriteNumberValue(value.MaxX);
            writer.WriteNumberValue(value.MaxY);
            writer.WriteEndArray();
        }
    }
}
