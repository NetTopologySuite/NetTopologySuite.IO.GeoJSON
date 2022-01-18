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
        private static IEnumerable<Geometry> CreateTestData()
        {
            var fac = GeometryFactory.Default;
            var p1 = fac.CreatePolygon(
                new LinearRing(new[]
                {
                    new Coordinate(-100, 45),
                    new Coordinate(-98, 45),
                    new Coordinate(-99, 46),
                    new Coordinate(-100, 45),
                }));
            var p2 = fac.CreatePolygon(
                new LinearRing(new[]
                {
                    new Coordinate(-101, 46),
                    new Coordinate(-99, 46),
                    new Coordinate(-100, 47),
                    new Coordinate(-101, 46),
                }));
            return new[] { p1, p2 };
        }

        [Test]
        public void TestEnumerableDeserialization()
        {
            var geoms = CreateTestData();

            var serializer = GeoJsonSerializer.CreateDefault();
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var jtw = new JsonTextWriter(sw))
            {
                serializer.Serialize(jtw, geoms);
            }
            string json = sb.ToString();
            Assert.That(string.IsNullOrWhiteSpace(json), Is.False);

            object tempObj;
            using var sr = new StringReader(json);
            using var jtr = new JsonTextReader(sr);
            {
                tempObj = serializer.Deserialize<IEnumerable<Geometry>>(jtr);
            }
            Assert.That(tempObj, Is.Not.Null);
            Assert.That(tempObj, Is.InstanceOf<IEnumerable<Geometry>>());
            var serializedData = (IEnumerable<Geometry>)tempObj;
            Assert.That(geoms.ElementAt(0).EqualsExact(serializedData.ElementAt(0)), Is.True);
            Assert.That(geoms.ElementAt(1).EqualsExact(serializedData.ElementAt(1)), Is.True);
        }
    }
}
