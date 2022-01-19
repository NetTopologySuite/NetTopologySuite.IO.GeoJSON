using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(89)]
    public sealed class GitHubIssue89
    {
        private static Geometry[] CreateTestData()
        {
            var fac = GeometryFactory.Default;
            var p1 = fac.CreatePolygon(
                fac.CreateLinearRing(new[]
                {
                    new Coordinate(-100, 45),
                    new Coordinate(-98, 45),
                    new Coordinate(-99, 46),
                    new Coordinate(-100, 45),
                }));
            var p2 = fac.CreatePolygon(
                fac.CreateLinearRing(new[]
                {
                    new Coordinate(-101, 46),
                    new Coordinate(-99, 46),
                    new Coordinate(-100, 47),
                    new Coordinate(-101, 46),
                }));
            return new[] { p1, p2 };
        }

        private static IEnumerable<Geometry> DoTest(IEnumerable<Geometry> geoms)
        {
            var serializer = GeoJsonSerializer.CreateDefault();
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var jtw = new JsonTextWriter(sw))
            {
                serializer.Serialize(jtw, geoms);
            }
            string json = sb.ToString();
            using var sr = new StringReader(json);
            using var jtr = new JsonTextReader(sr);
            {
                return serializer.Deserialize<IEnumerable<Geometry>>(jtr);
            }
        }

        [Test]
        public void TestPolygonsDeserialization()
        {
            IEnumerable<Geometry> geoms = CreateTestData();
            var serializedData = DoTest(geoms);
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is Polygon), Is.True);
            Assert.That(serializedData.ElementAt(0).EqualsExact(geoms.ElementAt(0)), Is.True);
            Assert.That(serializedData.ElementAt(1).EqualsExact(geoms.ElementAt(1)), Is.True);
        }

        [Test]
        public void TestLineStringsDeserialization()
        {
            IEnumerable<Geometry> geoms = CreateTestData()
                .Cast<Polygon>()
                .Select(p => p.Shell)
                .Cast<LineString>()
                .ToList();
            var serializedData = DoTest(geoms);
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is LineString), Is.True);
            Assert.That(serializedData.ElementAt(0).EqualsExact(geoms.ElementAt(0)), Is.True);
            Assert.That(serializedData.ElementAt(1).EqualsExact(geoms.ElementAt(1)), Is.True);
        }

        [Test]
        public void TestPointsDeserialization()
        {
            IEnumerable<Geometry> geoms = CreateTestData()
                .Cast<Polygon>()
                .Select(p => p.Shell)
                .Cast<LineString>()
                .SelectMany(s => s.Coordinates.Select(c => s.Factory.CreatePoint(c)))
                .ToList();
            var serializedData = DoTest(geoms);
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is Point), Is.True);
            Assert.That(serializedData.ElementAt(0).Coordinates[0]
                .Equals(geoms.ElementAt(0).Coordinates[0]), Is.True);
            Assert.That(serializedData.ElementAt(1).Coordinates[0]
                .Equals(geoms.ElementAt(1).Coordinates[0]), Is.True);
        }

        [Test]
        public void TestPolygonDeserialization()
        {
            var fac = GeometryFactory.Default;
            var shell = fac.CreateLinearRing(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 0),
                    new Coordinate(10, 10),
                    new Coordinate(0, 10),
                    new Coordinate(0, 0)
                });
            var hole = fac.CreateLinearRing(new[]
                {
                    new Coordinate(2, 2),
                    new Coordinate(2, 4),
                    new Coordinate(4, 4),
                    new Coordinate(4, 2),
                    new Coordinate(2, 2)
                });
            var poly = fac.CreatePolygon(shell, new[] { hole });
            Assert.That(poly.IsValid, Is.True);
            var serializedData = DoTest(new[] { poly });
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is Polygon), Is.True);
            Assert.That(serializedData.ElementAt(0).EqualsExact(poly), Is.True);
        }
    }
}
