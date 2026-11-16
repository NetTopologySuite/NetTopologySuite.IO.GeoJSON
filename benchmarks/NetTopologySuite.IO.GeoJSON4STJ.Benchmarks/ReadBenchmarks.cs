using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;

namespace NetTopologySuite.IO.GeoJSON4STJ.Benchmarks
{
    [MemoryDiagnoser]
    public class ReadBenchmarks
    {
        public enum Shape
        {
            ManyPoints,          // 50,000 points
            ManyPolygons,        // 5,000 polygons, 100-vertex shell + 20-vertex hole
            OneHugeMultiPolygon, // 50 polygons x 4,000 vertices
        }

        private byte[] _json;
        private JsonSerializerOptions _options;

        [Params(Shape.ManyPoints, Shape.ManyPolygons, Shape.OneHugeMultiPolygon)]
        public Shape Data { get; set; }

        [Params(false, true)]
        public bool CoordinatesFirst { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            _json = Encoding.UTF8.GetBytes(Generate(Data, CoordinatesFirst));
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new GeoJsonConverterFactory());

            var fc = JsonSerializer.Deserialize<FeatureCollection>(_json, _options);
            if (fc == null || fc.Count == 0)
            {
                throw new InvalidOperationException("benchmark data did not round-trip");
            }
        }

        [Benchmark(Description = "Deserialize(byte[])")]
        public FeatureCollection FromBytes()
            => JsonSerializer.Deserialize<FeatureCollection>(_json, _options);

        [Benchmark(Description = "DeserializeAsync(Stream)")]
        public async Task<FeatureCollection> FromStream()
        {
            using var stream = new MemoryStream(_json, writable: false);
            return await JsonSerializer.DeserializeAsync<FeatureCollection>(stream, _options);
        }

        private static string Generate(Shape shape, bool coordinatesFirst)
        {
            var rnd = new Random(1234);
            var sb = new StringBuilder();
            sb.Append("{\"type\":\"FeatureCollection\",\"features\":[");

            void Feature(int id, string type, Action<StringBuilder> writeCoordinates)
            {
                if (id > 0) sb.Append(',');
                sb.Append("{\"type\":\"Feature\",\"properties\":{\"id\":").Append(id).Append("},\"geometry\":{");
                if (coordinatesFirst)
                {
                    sb.Append("\"coordinates\":");
                    writeCoordinates(sb);
                    sb.Append(",\"type\":\"").Append(type).Append('"');
                }
                else
                {
                    sb.Append("\"type\":\"").Append(type).Append("\",\"coordinates\":");
                    writeCoordinates(sb);
                }

                sb.Append("}}");
            }

            void Position(StringBuilder b, double x, double y)
                => b.Append('[').Append(x.ToString("F6", CultureInfo.InvariantCulture))
                    .Append(',').Append(y.ToString("F6", CultureInfo.InvariantCulture)).Append(']');

            void Ring(StringBuilder b, double cx, double cy, double r, int n)
            {
                b.Append('[');
                for (int i = 0; i <= n; i++)
                {
                    double a = 2 * Math.PI * (i % n) / n;
                    if (i > 0) b.Append(',');
                    Position(b, cx + r * Math.Cos(a), cy + r * Math.Sin(a));
                }

                b.Append(']');
            }

            switch (shape)
            {
                case Shape.ManyPoints:
                    for (int i = 0; i < 50_000; i++)
                    {
                        double x = rnd.NextDouble() * 360 - 180, y = rnd.NextDouble() * 180 - 90;
                        Feature(i, "Point", b => Position(b, x, y));
                    }

                    break;

                case Shape.ManyPolygons:
                    for (int i = 0; i < 5_000; i++)
                    {
                        double x = rnd.NextDouble() * 300 - 150, y = rnd.NextDouble() * 160 - 80;
                        Feature(i, "Polygon", b =>
                        {
                            b.Append('[');
                            Ring(b, x, y, 1.0, 100);
                            b.Append(',');
                            Ring(b, x, y, 0.2, 20);
                            b.Append(']');
                        });
                    }

                    break;

                case Shape.OneHugeMultiPolygon:
                    Feature(0, "MultiPolygon", b =>
                    {
                        b.Append('[');
                        for (int p = 0; p < 50; p++)
                        {
                            if (p > 0) b.Append(',');
                            b.Append('[');
                            Ring(b, p * 3.0, 0, 1.0, 4_000);
                            b.Append(']');
                        }

                        b.Append(']');
                    });
                    break;
            }

            sb.Append("]}");
            return sb.ToString();
        }
    }
}
