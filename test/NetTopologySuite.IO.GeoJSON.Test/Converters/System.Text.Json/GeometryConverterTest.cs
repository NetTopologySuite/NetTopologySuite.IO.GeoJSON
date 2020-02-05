using System;
using System.IO;
using System.Text;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Converters.System.Text.Json
{
    public class GeometryConverterTest : SandDTest<Geometry>
    {
        [Test]
        public void TestCanConvert()
        {
            var c = new StjGeometryConverter();
            Assert.That(c.CanConvert(typeof(Envelope)), Is.False);
            Assert.That(c.CanConvert(typeof(Geometry)), Is.True);
            Assert.That(c.CanConvert(typeof(Point)), Is.True);
            Assert.That(c.CanConvert(typeof(LineString)), Is.True);
            Assert.That(c.CanConvert(typeof(Polygon)), Is.True);
            Assert.That(c.CanConvert(typeof(MultiPoint)), Is.True);
            Assert.That(c.CanConvert(typeof(MultiLineString)), Is.True);
            Assert.That(c.CanConvert(typeof(MultiPolygon)), Is.True);
            Assert.That(c.CanConvert(typeof(GeometryCollection)), Is.True);
        }

        [Test]
        public void TestReadPoint2D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""Point"", ""coordinates"": [102.0, 0.5] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
        }

        [Test]
        public void TestReadPoint3D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""Point"", ""coordinates"": [102.0, 0.5, 6.2] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));
            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(Point)));
            Assert.That(geom.Coordinate, Is.InstanceOf(typeof(CoordinateZ)));
        }

        [Test]
        public void TestReadLineString2D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""LineString"", ""coordinates"": [[102.0, 0.5],[112.7, 2.1]] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(LineString)));
        }

        [Test]
        public void TestReadLineString3D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""LineString"", ""coordinates"": [[102.0, 0.5, 2.45],[112.7, 2.1, 2.34]] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(LineString)));
            Assert.That(geom.Coordinate, Is.InstanceOf(typeof(CoordinateZ)));
        }

        [Test]
        public void TestReadPolygon2D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""Polygon"", ""coordinates"": [[[0, 0],[10, 0],[10, 10],[0, 0]],[[1, 1],[9, 9],[1, 9],[1, 1]]] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(Polygon)));
            Assert.That(((Polygon)geom).NumInteriorRings, Is.EqualTo(1));
        }

        [Test]
        public void TestReadMultiPoint2D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""MultiPoint"", ""coordinates"": [[102.0, 0.5],[112.7, 2.1],[102.0, 1.5],[112.7, 3.1]] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(MultiPoint)));
            Assert.That(geom.NumGeometries, Is.EqualTo(4));
        }

        [Test]
        public void TestReadMultiLineString2D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""MultiLineString"", ""coordinates"": [[[102.0, 0.5],[112.7, 2.1]],[[102.0, 1.5],[112.7, 3.1]]] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(MultiLineString)));
            Assert.That(geom.NumGeometries, Is.EqualTo(2));
        }

        [Test]
        public void TestReadMultiPolygon2D()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""MultiPolygon"", ""coordinates"": [
[[[0, 0],[10, 0],[10, 10],[0, 0]],[[1, 1],[9, 9],[1, 9],[1, 1]]],
[[[20, 20],[30, 20],[30, 30],[20, 20]],[[21, 21],[29, 29],[21, 29],[21, 21]]]
] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(MultiPolygon)));
            Assert.That(geom.NumGeometries, Is.EqualTo(2));
        }

        [Test]
        public void TestReadGeometryCollection()
        {
            var c = new StjGeometryConverter();
            string geoJson = @"{ ""type"" : ""GeometryCollection"", ""geometries"": [
{ ""type"" : ""Polygon"", ""coordinates"": [[[0, 0],[10, 0],[10, 10],[0, 0]],[[1, 1],[9, 9],[1, 9],[1, 1]]] },
{ ""type"" : ""LineString"", ""coordinates"": [[102.0, 0.5],[112.7, 2.1]] },
{ ""type"" : ""Point"", ""coordinates"": [102.0, 0.5] }
] }";
            var utf8 = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
            var rdr = new Utf8JsonReader(utf8);
            // nothing read
            rdr.Read();
            var geom = c.Read(ref rdr, typeof(Geometry), new JsonSerializerOptions());
            Assert.That(rdr.BytesConsumed, Is.EqualTo(utf8.Length));

            Assert.That(geom != null);
            Assert.That(geom, Is.InstanceOf(typeof(GeometryCollection)));
            Assert.That(geom.NumGeometries, Is.EqualTo(3));
        }


        [TestCase("POINT (1 2)")]
        [TestCase("POINT Z (1 2 3)")]
        [TestCase("LINESTRING (1 2, 2 2)")]
        [TestCase("LINESTRING Z (1 2 0, 2 2 0)")]
        [TestCase("POLYGON ((0 0, 10 10, 0 10, 0 0))")]
        [TestCase("POLYGON Z ((0 0 1, 10 10 1, 0 10 1, 0 0 1))")]
        [TestCase("POLYGON ((0 0, 10 10, 0 10, 0 0), (1 2, 1 9, 8 9, 1 2))")]
        [TestCase("POLYGON Z ((0 0 1, 10 10 1, 0 10 1, 0 0 1), (1 2.4 1, 1 9 1, 7.6 9 1, 1 2.4 1))")]

        public void TestWriteReadWkt(string wkt)
        {
            var wktReader = new WKTReader(StjGeometryConverter.DefaultGeometryFactory);
            var geomS = wktReader.Read(wkt);

            var c = new StjGeometryConverter();
            var ms = new MemoryStream();
            Serialize(c, ms, geomS, new JsonSerializerOptions());
            var geomD = Deserialize(c, ms, new JsonSerializerOptions());

            Assert.That(geomS.EqualsTopologically(geomD));
        }
    }
}
