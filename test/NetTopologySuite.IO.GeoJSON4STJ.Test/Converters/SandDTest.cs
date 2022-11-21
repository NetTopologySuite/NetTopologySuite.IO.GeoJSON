using System;
using System.IO;
using System.Text;
using System.Text.Json;
using NetTopologySuite.IO.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    public abstract class SandDTest<T>
    {
        protected GeoJsonConverterFactory GeoJsonConverterFactory { get; }
            = new GeoJsonConverterFactory();

        protected JsonSerializerOptions DefaultOptions
        {
            get
            {
                var res = new JsonSerializerOptions
                    {ReadCommentHandling = JsonCommentHandling.Skip};
                res.Converters.Add(GeoJsonConverterFactory);
                return res;
            }
        }

        protected void Serialize(Stream stream, T value, JsonSerializerOptions options)
        {
            using (var writer = new Utf8JsonWriter(stream))
                JsonSerializer.Serialize(writer, value, options);
        }

        protected string ToJsonString(T value, JsonSerializerOptions options = null)
        {
            if (options == null)
                options = DefaultOptions;

            using (var ms = new MemoryStream())
            {
                Serialize(ms, value, options);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        protected T Deserialize(string geoJson, JsonSerializerOptions options)
        {
            using(var ms = new MemoryStream(Encoding.UTF8.GetBytes(geoJson)))
                return Deserialize(ms, options);
        }

        protected T Deserialize(MemoryStream stream, JsonSerializerOptions options)
        {
            var b = new ReadOnlySpan<byte>(stream.ToArray());
            var r = new Utf8JsonReader(b);

            // we are at None
            r.Read();
            var res = JsonSerializer.Deserialize<T>(ref r, options);

            return res;
        }
        protected T FromJsonString(string json, JsonSerializerOptions options = null)
        {
            return Deserialize(json, options);
        }
    }
}
