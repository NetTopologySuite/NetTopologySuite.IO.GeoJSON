using System;
using System.IO;
using System.Text;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(98)]
    public sealed class Issue98
    {
        private GeoJsonConverterFactory GeoJsonConverterFactory { get; } = new GeoJsonConverterFactory();

        private JsonSerializerOptions DefaultOptions
        {
            get
            {
                var res = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                res.Converters.Add(GeoJsonConverterFactory);
                return res;
            }
        }

        private string Serialize(Geometry geom)
        {
            using var ms = new MemoryStream();
            using (var writer = new Utf8JsonWriter(ms))
                JsonSerializer.Serialize(writer, geom, DefaultOptions);
            return Encoding.UTF8.GetString(ms.ToArray());
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
            var settings = new JsonSerializerOptions()
            {
                Converters = { new GeoJsonConverterFactory(fac) }
            };
            string json = JsonSerializer.Serialize(fac.CreatePoint(), settings);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyPointCustomSettingsNullsIgnored()
        {
            var fac = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
            var settings = new JsonSerializerOptions()
            {
                Converters = { new GeoJsonConverterFactory(fac) },
                IgnoreNullValues = true
            };
            string json = JsonSerializer.Serialize(fac.CreatePoint(), settings);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[]}", json);
        }

        [Test]
        public void TestSerializeEmptyPointCustomSettingsNullsIncluded()
        {
            var fac = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
            var settings = new JsonSerializerOptions()
            {
                Converters = { new GeoJsonConverterFactory(fac) },
                IgnoreNullValues = false
            };
            string json = JsonSerializer.Serialize(fac.CreatePoint(), settings);
            Console.WriteLine(json);
            Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[]}", json);
        }
    }
}
