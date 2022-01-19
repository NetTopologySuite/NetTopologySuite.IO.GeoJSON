using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GeoJSON4STJ.Test.Converters;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(89)]
    public sealed class Issue89 : SandDTest<IEnumerable<Geometry>>
    {
        private IEnumerable<T> DoTest<T>(IEnumerable<T> geoms)
            where T : Geometry
        {
            using var ms = new MemoryStream();
            Serialize(ms, geoms, DefaultOptions);
            string json = Encoding.UTF8.GetString(ms.ToArray());
            return Deserialize(json, DefaultOptions)
                /*
                 * needed to deserialize to the actual geometry type!
                 * see: https://github.com/NetTopologySuite/NetTopologySuite.IO.GeoJSON/pull/90#discussion_r787875845
                 */
                .Cast<T>();
        }

        // NOTE: from here we use the same test code of GithubIssue99 in GeoJSON4.Test!

        private static Polygon[] CreateTestData()
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

        [Test]
        public void TestPolygonsDeserializationAsGeometries()
        {
            IEnumerable<Geometry> geoms = CreateTestData();
            var serializedData = DoTest<Geometry>(geoms);
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is Polygon), Is.True);
            Assert.That(serializedData.ElementAt(0).EqualsExact(geoms.ElementAt(0)), Is.True);
            Assert.That(serializedData.ElementAt(1).EqualsExact(geoms.ElementAt(1)), Is.True);
        }

        [Test]
        public void TestPolygonsDeserialization()
        {
            IEnumerable<Polygon> geoms = CreateTestData();
            var serializedData = DoTest<Polygon>(geoms);
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is Polygon), Is.True);
            Assert.That(serializedData.ElementAt(0).EqualsExact(geoms.ElementAt(0)), Is.True);
            Assert.That(serializedData.ElementAt(1).EqualsExact(geoms.ElementAt(1)), Is.True);
        }

        [Test]
        public void TestLineStringsDeserialization()
        {
            IEnumerable<LineString> geoms = CreateTestData()
                .Cast<Polygon>()
                .Select(p => p.Shell)
                .Cast<LineString>()
                .ToList();
            var serializedData = DoTest<LineString>(geoms);
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is LineString), Is.True);
            Assert.That(serializedData.ElementAt(0).EqualsExact(geoms.ElementAt(0)), Is.True);
            Assert.That(serializedData.ElementAt(1).EqualsExact(geoms.ElementAt(1)), Is.True);
        }

        [Test]
        public void TestPointsDeserialization()
        {
            IEnumerable<Point> geoms = CreateTestData()
                .Cast<Polygon>()
                .Select(p => p.Shell)
                .Cast<LineString>()
                .SelectMany(s => s.Coordinates.Select(c => s.Factory.CreatePoint(c)))
                .ToList();
            var serializedData = DoTest<Point>(geoms);
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
            var serializedData = DoTest<Polygon>(new[] { poly });
            Assert.That(serializedData, Is.Not.Null);
            Assert.That(serializedData.All(p => p is Polygon), Is.True);
            Assert.That(serializedData.ElementAt(0).EqualsExact(poly), Is.True);
        }
    }
}
