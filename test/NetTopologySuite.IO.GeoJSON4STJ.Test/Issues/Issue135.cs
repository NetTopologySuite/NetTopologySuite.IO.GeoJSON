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
    }
}
