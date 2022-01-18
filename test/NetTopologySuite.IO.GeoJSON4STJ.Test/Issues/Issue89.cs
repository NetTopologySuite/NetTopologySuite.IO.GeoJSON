using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GeoJSON4STJ.Test.Converters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(89)]
    public sealed class Issue89 : SandDTest<IEnumerable<Geometry>>
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
            string json;
            using (var ms = new MemoryStream())
            {
                Serialize(ms, geoms, DefaultOptions);
                json = Encoding.UTF8.GetString(ms.ToArray());
                Assert.That(string.IsNullOrWhiteSpace(json), Is.False);
                Console.WriteLine(json);
            }

            object tempObj = Deserialize(json, DefaultOptions);
            Assert.That(tempObj, Is.Not.Null);
            Assert.That(tempObj, Is.InstanceOf<IEnumerable<Geometry>>());
            var serializedData = (IEnumerable<Geometry>)tempObj;
            Assert.That(geoms.ElementAt(0).EqualsExact(serializedData.ElementAt(0)), Is.True);
            Assert.That(geoms.ElementAt(1).EqualsExact(serializedData.ElementAt(1)), Is.True);
        }
    }
}
