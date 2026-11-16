using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.IO.GeoJSON4STJ.Test.Converters;
using NUnit.Framework;
using System.Text.Json;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    internal class Issue135 : SandDTest<Geometry>
    {
        [Test, GeoJsonIssueNumber(135)]
        public void TestOutputPrecision()
        {
            var coords = new[]
            {
                new Coordinate(0.001, 0.001),
                new Coordinate(10.1, 0.002),
                new Coordinate(10, 10.1),
                new Coordinate(0.05, 9.999),
                new Coordinate(0.001, 0.001)
            };
            var factory = new GeometryFactory(new PrecisionModel(10), 4326);
            var polygon = factory.CreatePolygon(coords);

            string json = JsonSerializer.Serialize(polygon, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                Converters =
                {
                    new GeoJsonConverterFactory(factory)
                }
            });

            Assert.That(json, Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[10.1,0],[10,10.1],[0.1,10],[0,0]]]}"));
        }

        private static readonly GeometryFactory Fixed10 = new GeometryFactory(new PrecisionModel(10), 4326);

        private static JsonSerializerOptions Options(GeometryFactory factory, bool writeBBox = false)
        {
            var res = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
            res.Converters.Add(new GeoJsonConverterFactory(factory, writeBBox));
            return res;
        }

        [TestCase("POINT (1.26 2.34)", "{\"type\":\"Point\",\"coordinates\":[1.3,2.3]}")]
        [TestCase("POINT Z (1.26 2.34 3.47)", "{\"type\":\"Point\",\"coordinates\":[1.3,2.3,3.5]}")]
        [TestCase("LINESTRING (0.04 0.06, 1.01 1.99)", "{\"type\":\"LineString\",\"coordinates\":[[0,0.1],[1,2]]}")]
        [TestCase("MULTIPOINT ((0.04 0.06), (1.01 1.99))", "{\"type\":\"MultiPoint\",\"coordinates\":[[0,0.1],[1,2]]}")]
        [TestCase("MULTILINESTRING ((0.04 0.06, 1.01 1.99))", "{\"type\":\"MultiLineString\",\"coordinates\":[[[0,0.1],[1,2]]]}")]
        [TestCase("GEOMETRYCOLLECTION (POINT (1.26 2.34))", "{\"type\":\"GeometryCollection\",\"geometries\":[{\"type\":\"Point\",\"coordinates\":[1.3,2.3]}]}")]
        [GeoJsonIssueNumber(135)]
        public void TestOutputPrecisionForAllGeometryTypes(string wkt, string expectedJson)
        {
            var geometry = new WKTReader().Read(wkt);
            Assert.That(JsonSerializer.Serialize<Geometry>(geometry, Options(Fixed10)), Is.EqualTo(expectedJson));
        }

        [Test, GeoJsonIssueNumber(135)]
        public void TestFloatingPrecisionModelDoesNotRound()
        {
            var factory = new GeometryFactory(new PrecisionModel(PrecisionModels.Floating), 4326);
            var point = factory.CreatePoint(new CoordinateZ(0.123456789012345, -98.7654321098765, 1.000000000001));

            Assert.That(JsonSerializer.Serialize<Geometry>(point, Options(factory)),
                Is.EqualTo("{\"type\":\"Point\",\"coordinates\":[0.123456789012345,-98.7654321098765,1.000000000001]}"));
        }

        [Test, GeoJsonIssueNumber(135)]
        public void TestOutputPrecisionForBBox()
        {
            var line = Fixed10.CreateLineString(new[] { new Coordinate(0.04, 0.06), new Coordinate(10.01, 9.99) });

            string json = JsonSerializer.Serialize<Geometry>(line, Options(Fixed10, writeBBox: true));

            Assert.That(json, Does.Contain("\"bbox\":[0,0.1,10,10]"));
        }

        [TestCase("[0.01, 0.06, 10.04, 10.06]")]
        [TestCase("[0.01, 0.06, -5.55, 10.04, 10.06, 5.55]")]
        [GeoJsonIssueNumber(135)]
        public void TestInputPrecisionForBBox(string bbox)
        {
            string json = $"{{\"type\":\"Feature\",\"bbox\":{bbox},\"geometry\":null,\"properties\":{{}}}}";

            var feature = JsonSerializer.Deserialize<IFeature>(json, Options(Fixed10));

            Assert.That(feature.BoundingBox, Is.EqualTo(new Envelope(0, 10, 0.1, 10.1)));
        }

        [Test, GeoJsonIssueNumber(135)]
        public void TestFeatureWithBBoxCanStillBeRead()
        {
            string json = "{\"type\":\"Feature\",\"bbox\":[0.01,0.06,10.04,10.06],\"geometry\":null,\"properties\":{\"a\":1}}";

            var feature = JsonSerializer.Deserialize<IFeature>(json, Options(GeometryFactory.Default));

            Assert.That(feature.BoundingBox, Is.EqualTo(new Envelope(0.01, 10.04, 0.06, 10.06)));
            Assert.That(feature.Attributes["a"], Is.EqualTo(1));
        }

        [Test, GeoJsonIssueNumber(135)]
        public void TestEnvelopeRoundTripsOnItsOwn()
        {
            var options = Options(Fixed10);
            string json = JsonSerializer.Serialize(new Envelope(0.04, 10.01, 0.06, 9.99), options);

            Assert.That(json, Is.EqualTo("[0,0.1,10,10]"));
            Assert.That(JsonSerializer.Deserialize<Envelope>(json, options), Is.EqualTo(new Envelope(0, 10, 0.1, 10)));
        }
    }
}
