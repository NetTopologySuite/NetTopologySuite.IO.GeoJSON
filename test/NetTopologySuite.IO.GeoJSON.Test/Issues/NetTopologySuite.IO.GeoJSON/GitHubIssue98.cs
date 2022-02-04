using System;
using System.IO;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(98)]
    public sealed class GitHubIssue98
    {
        private static string Serialize(Geometry geom)
        {
            var serializer = GeoJsonSerializer.CreateDefault();
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var jtw = new JsonTextWriter(sw);
            serializer.Serialize(jtw, geom);
            return sb.ToString();
        }

        [Test]
        public void TestSerializeEmptyPoint()
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreatePoint();
            string json = Serialize(geom);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyLineString()
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreateLineString();
            string json = Serialize(geom);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""LineString"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyPolygon()
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreatePolygon();
            string json = Serialize(geom);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Polygon"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyMultiPoint()
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreateMultiPoint();
            string json = Serialize(geom);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""MultiPoint"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyMultiLineString()
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreateMultiLineString();
            string json = Serialize(geom);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""MultiLineString"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyMultiPolygon()
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreateMultiPolygon();
            string json = Serialize(geom);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""MultiPolygon"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyGeometryCollection()
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreateGeometryCollection();
            string json = Serialize(geom);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""GeometryCollection"",""geometries"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyPointCustomSettingsNullsDefault()
        {
            var fac = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
            var settings = new JsonSerializerSettings()
            {
                Converters = { new GeometryConverter(fac) }
            };
            string json = JsonConvert.SerializeObject(fac.CreatePoint(), settings);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyPointCustomSettingsNullsIgnored()
        {
            var fac = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
            var settings = new JsonSerializerSettings()
            {
                Converters = { new GeometryConverter(fac) },
                NullValueHandling = NullValueHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(fac.CreatePoint(), settings);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyPointCustomSettingsNullsIncluded()
        {
            var fac = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
            var settings = new JsonSerializerSettings()
            {
                Converters = { new GeometryConverter(fac) },
                NullValueHandling = NullValueHandling.Include
            };
            string json = JsonConvert.SerializeObject(fac.CreatePoint(), settings);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[]}", json);
        }
    }
}
