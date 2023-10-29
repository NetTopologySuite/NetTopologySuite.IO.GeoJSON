using NetTopologySuite.Geometries;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetTopologySuite.IO.Converters
{
    internal class StjEnvelopeConverter : JsonConverter<Envelope>
    {
        private readonly PrecisionModel _precisionModel;

        public StjEnvelopeConverter(PrecisionModel precisionModel)
        {
            _precisionModel = precisionModel;
        }

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

                double minX = reader.GetDouble(_precisionModel);
                reader.Read();
                double minY = reader.GetDouble(_precisionModel);
                reader.Read();
                double maxX = reader.GetDouble(_precisionModel);
                reader.Read();
                double maxY = reader.GetDouble(_precisionModel);
                reader.Read();

                if (reader.TokenType == JsonTokenType.Number)
                {
                    maxX = maxY;
                    maxY = reader.GetDouble(_precisionModel);
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
            writer.WriteNumberValue(value.MinX, _precisionModel);
            writer.WriteNumberValue(value.MinY, _precisionModel);
            writer.WriteNumberValue(value.MaxX, _precisionModel);
            writer.WriteNumberValue(value.MaxY, _precisionModel);
            writer.WriteEndArray();
        }
    }
}
