using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetTopologySuite.IO.GeoJSON.Test.Converters.System.Text.Json
{
    public abstract class SandDTest<T>
    {
        protected void Serialize(JsonConverter<T> converter, Stream stream, T value,
            JsonSerializerOptions options, bool valueIsObject = true)
        {
            using (var writer = new Utf8JsonWriter(stream))
            {
                if (!valueIsObject)
                    writer.WriteStartObject();
                converter.Write(writer, value, options);
                if (!valueIsObject)
                    writer.WriteEndObject();
            }
        }

        protected T Deserialize(JsonConverter<T> converter, MemoryStream stream,
            JsonSerializerOptions options, bool valueIsObject = true)
        {
            var b = new ReadOnlySpan<byte>(stream.ToArray());
            var r = new Utf8JsonReader(b);

            // we are at None
            r.Read();
            if (!valueIsObject)
                r.Read();
            var res = converter.Read(ref r, typeof(T), options);
            if (!valueIsObject)
                r.Read();

            return res;
        }
    }
}
