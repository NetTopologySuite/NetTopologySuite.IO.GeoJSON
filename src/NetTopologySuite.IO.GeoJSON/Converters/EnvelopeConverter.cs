using System;
using System.Diagnostics;
using System.Globalization;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NetTopologySuite.IO.Converters
{
    /// <summary>
    /// Converts <see cref="Envelope"/>s to and from JSON
    /// </summary>
    public class EnvelopeConverter : JsonConverter
    {
        private readonly PrecisionModel _precisionModel;

        /// <summary>
        /// Creates an instance of this class that is using a <see cref="PrecisionModels.Floating"/> precision model.
        /// </summary>
        public EnvelopeConverter()
            :this(new PrecisionModel(PrecisionModels.Floating))
        {
        }

        /// <summary>
        /// Creates an instance of this class using the provided precision model
        /// </summary>
        /// <param name="precisionModel">A precision model</param>
        internal EnvelopeConverter(PrecisionModel precisionModel)
        {
            _precisionModel = precisionModel;
        }

        /// <summary>
        /// Writes an <see cref="Envelope"/> to JSON
        /// </summary>
        /// <param name="writer">The writer</param>
        /// <param name="value">The envelope</param>
        /// <param name="serializer">The serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is Envelope envelope))
            {
                writer.WriteToken(null);
                return;
            }

            writer.WriteStartArray();
            writer.WriteValue(_precisionModel.MakePrecise(envelope.MinX));
            writer.WriteValue(_precisionModel.MakePrecise(envelope.MinY));
            writer.WriteValue(_precisionModel.MakePrecise(envelope.MaxX));
            writer.WriteValue(_precisionModel.MakePrecise(envelope.MaxY));
            writer.WriteEndArray();
        }

        /// <summary>
        /// Reads an <see cref="Envelope"/> from JSON
        /// </summary>
        /// <param name="reader">The reader</param>
        /// <param name="objectType">The object type</param>
        /// <param name="existingValue">The existing value</param>
        /// <param name="serializer">The serializer</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.Assert(reader.TokenType == JsonToken.PropertyName);
            Debug.Assert((string)reader.Value == "bbox");
            reader.Read(); // move to array start

            if (reader.TokenType != JsonToken.Null)
            {
                var envelope = serializer.Deserialize<JArray>(reader);
                Debug.Assert(envelope.Count == 4);

                double minX = _precisionModel.MakePrecise(double.Parse((string)envelope[0], NumberFormatInfo.InvariantInfo));
                double minY = _precisionModel.MakePrecise(double.Parse((string)envelope[1], NumberFormatInfo.InvariantInfo));
                double maxX = _precisionModel.MakePrecise(double.Parse((string)envelope[2], NumberFormatInfo.InvariantInfo));
                double maxY = _precisionModel.MakePrecise(double.Parse((string)envelope[3], NumberFormatInfo.InvariantInfo));

                Debug.Assert(minX <= maxX);
                Debug.Assert(minY <= maxY);

                reader.Read(); // move away from array end
                return new Envelope(minX, maxX, minY, maxY);
            }

            reader.Read(); // move away from array end
            return null;
        }

        /// <summary>
        /// Predicate function to check if an instance of <paramref name="objectType"/> can be converted using this converter.
        /// </summary>
        /// <param name="objectType">The type of the object to convert</param>
        /// <returns><value>true</value> if the conversion is possible, otherwise <value>false</value></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Envelope);
        }
    }
}
