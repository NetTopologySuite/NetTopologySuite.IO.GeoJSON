using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(145)]
    public sealed class Issue145
    {
        private static JsonSerializerOptions Options
        {
            get
            {
                var res = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                res.Converters.Add(new GeoJsonConverterFactory());
                return res;
            }
        }

        private static Geometry Read(string json)
            => JsonSerializer.Deserialize<Geometry>(json, Options);

        [TestCase("MultiPolygon", "[[]]", "MULTIPOLYGON (EMPTY)")]
        [TestCase("MultiPolygon", "[[[]]]", "MULTIPOLYGON (EMPTY)")]
        [TestCase("MultiPolygon", "[[], [[[0,0],[1,0],[1,1],[0,0]]]]", "MULTIPOLYGON (EMPTY, ((0 0, 1 0, 1 1, 0 0)))")]
        [TestCase("MultiPolygon", "[[[[0,0],[1,0],[1,1],[0,0]]], []]", "MULTIPOLYGON (((0 0, 1 0, 1 1, 0 0)), EMPTY)")]
        [TestCase("Polygon", "[[]]", "POLYGON EMPTY")]
        [TestCase("Polygon", "[[[0,0],[9,0],[9,9],[0,0]], []]", "POLYGON ((0 0, 9 0, 9 9, 0 0), EMPTY)")]
        [TestCase("MultiLineString", "[[]]", "MULTILINESTRING (EMPTY)")]
        [TestCase("MultiLineString", "[[], [[0,0],[1,1]]]", "MULTILINESTRING (EMPTY, (0 0, 1 1))")]
        [TestCase("MultiLineString", "[[[0,0],[1,1]], []]", "MULTILINESTRING ((0 0, 1 1), EMPTY)")]
        public void EmptyArraysAreReadAtTheDepthTheTypeExpects(string type, string coordinates, string expectedWkt)
        {
            var expected = new WKTReader().Read(expectedWkt);

            var typeFirst = Read($"{{\"type\": \"{type}\", \"coordinates\": {coordinates}}}");
            var coordinatesFirst = Read($"{{\"coordinates\": {coordinates}, \"type\": \"{type}\"}}");

            Assert.That(typeFirst.EqualsExact(expected), $"type first: {typeFirst}");
            Assert.That(coordinatesFirst.EqualsExact(expected), $"coordinates first: {coordinatesFirst}");
        }

        [Test]
        public void EmptyPositionsInAMultiPointAreEmptyPoints()
        {
            // WKTReader can't parse "MULTIPOINT (EMPTY, (1 2))", so check the parts directly
            foreach (string json in new[]
                     {
                         "{\"type\": \"MultiPoint\", \"coordinates\": [[], [1,2]]}",
                         "{\"coordinates\": [[], [1,2]], \"type\": \"MultiPoint\"}",
                     })
            {
                var geom = (MultiPoint)Read(json);
                Assert.That(geom.NumGeometries, Is.EqualTo(2));
                Assert.That(geom.GetGeometryN(0).IsEmpty);
                Assert.That(geom.GetGeometryN(1).EqualsExact(new WKTReader().Read("POINT (1 2)")));
            }
        }

        [TestCase("Point", "[1,2]", "POINT (1 2)")]
        [TestCase("LineString", "[[1,2],[3,4]]", "LINESTRING (1 2, 3 4)")]
        [TestCase("MultiPoint", "[[1,2],[3,4]]", "MULTIPOINT ((1 2), (3 4))")]
        [TestCase("Polygon", "[[[0,0],[1,0],[1,1],[0,0]]]", "POLYGON ((0 0, 1 0, 1 1, 0 0))")]
        [TestCase("MultiLineString", "[[[1,2],[3,4]]]", "MULTILINESTRING ((1 2, 3 4))")]
        [TestCase("MultiPolygon", "[[[[0,0],[1,0],[1,1],[0,0]]]]", "MULTIPOLYGON (((0 0, 1 0, 1 1, 0 0)))")]
        public void CoordinatesBeforeTypeAreReadTheSame(string type, string coordinates, string expectedWkt)
        {
            var expected = new WKTReader().Read(expectedWkt);
            var geom = Read($"{{\"coordinates\": {coordinates}, \"bbox\": null, \"type\": \"{type}\"}}");
            Assert.That(geom.EqualsExact(expected), geom.ToString());
        }

        [TestCase("{\"coordinates\": [[1,2],[3,4,5]], \"type\": \"LineString\"}")]
        [TestCase("{\"type\": \"LineString\", \"coordinates\": [[1,2],[3,4,5]]}")]
        public void MixedXYAndXYZPositionsGetNullOrdinateForMissingZ(string json)
        {
            var geom = (LineString)Read(json);
            Assert.That(geom.CoordinateSequence.Dimension, Is.EqualTo(3));
            Assert.That(geom.CoordinateSequence.GetZ(0), Is.NaN);
            Assert.That(geom.CoordinateSequence.GetZ(1), Is.EqualTo(5));
        }

        [Test]
        public void CommentsInsideCoordinatesAreSkipped()
        {
            var expected = new WKTReader().Read("LINESTRING (1 2, 3 4)");
            const string coordinates = "[/* a */ [1, /* x */ 2 // y\n], [3, 4] /* b */]";
            Assert.That(Read($"{{\"coordinates\": {coordinates}, \"type\": \"LineString\"}}").EqualsExact(expected));
            Assert.That(Read($"{{\"type\": \"LineString\", \"coordinates\": {coordinates}}}").EqualsExact(expected));
        }

        [Test]
        public void ReadingFromAStreamStillWorks()
        {
            const string json = "{\"type\": \"FeatureCollection\", \"features\": [" +
                "{\"type\": \"Feature\", \"geometry\": {\"coordinates\": [[]], \"type\": \"MultiLineString\"}, \"properties\": {}}," +
                "{\"type\": \"Feature\", \"geometry\": {\"coordinates\": [[1,2],[3,4]], \"type\": \"LineString\"}, \"properties\": {}}]}";

            // a tiny buffer forces the reader to work across many partial reads
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var options = Options;
            options.DefaultBufferSize = 16;
            var fc = JsonSerializer.DeserializeAsync<NetTopologySuite.Features.FeatureCollection>(ms, options).AsTask().Result;

            Assert.That(fc.Count, Is.EqualTo(2));
            Assert.That(fc[0].Geometry, Is.InstanceOf<MultiLineString>());
            Assert.That(fc[1].Geometry, Is.InstanceOf<LineString>());
        }

        [TestCase("Point", "[[1,2]]")]
        [TestCase("Point", "[1]")]
        [TestCase("LineString", "[1,2]")]
        [TestCase("LineString", "[[]]")]
        [TestCase("Polygon", "[[1,2]]")]
        [TestCase("MultiPolygon", "[[[1,2]]]")]
        [TestCase("MultiPoint", "[[\"a\", 2]]")]
        [TestCase("Polygon", "[[[0,0],[1,0],[1,1],[0,0]], [1,2]]")]
        [TestCase("MultiLineString", "[[[1,2],[3,4]], [[1,2],[3]]]")]
        public void CoordinatesAtTheWrongDepthThrowJsonException(string type, string coordinates)
        {
            Assert.That(() => Read($"{{\"coordinates\": {coordinates}, \"type\": \"{type}\"}}"),
                Throws.InstanceOf<JsonException>().With.Message.Contains(type));

            Assert.That(() => Read($"{{\"type\": \"{type}\", \"coordinates\": {coordinates}}}"),
                Throws.InstanceOf<JsonException>().With.Message.Contains(type));
        }

        private static Geometry[] ReadManyFromPartialBuffers(string json)
        {
            using var stream = new SingleByteReadingMemoryStream();
            var bytes = Encoding.UTF8.GetBytes("[" + string.Join(",", Enumerable.Repeat(json, 500)) + "]");
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;
            return JsonSerializer.Deserialize<Geometry[]>(stream, Options);
        }

        [TestCase("{\"type\": \"LineString\", \"bbox\": [1,2,3,4], \"coordinates\": [[1,2],[3,4]]}")]
        [TestCase("{\"coordinates\": [[1,2],[3,4]], \"bbox\": [1,2,3,4], \"type\": \"LineString\"}")]
        [TestCase("{\"bbox\": [1,2,3,4], \"coordinates\": [[1,2],[3,4]], \"type\": \"LineString\"}")]
        public void SkippedMembersInPartialBuffersDoNotThrow(string json)
        {
            var expected = new WKTReader().Read("LINESTRING (1 2, 3 4)");
            var geoms = ReadManyFromPartialBuffers(json);
            Assert.That(geoms, Has.Length.EqualTo(500));
            Assert.That(geoms.All(g => g.EqualsExact(expected)));
        }

        [TestCase("\"abc\"")]
        [TestCase("{\"a\": [1, 2]}")]
        [TestCase("[[1,2], {\"a\": [1, 2]}]")]
        public void MalformedCoordinatesInPartialBuffersThrowTheUsualException(string coordinates)
        {
            Assert.That(() => ReadManyFromPartialBuffers($"{{\"type\": \"LineString\", \"coordinates\": {coordinates}}}"),
                Throws.InstanceOf<JsonException>().With.Message.Contains("LineString"));
            Assert.That(() => ReadManyFromPartialBuffers($"{{\"coordinates\": {coordinates}, \"type\": \"LineString\"}}"),
                Throws.InstanceOf<JsonException>().With.Message.Contains("LineString"));
        }
    }
}
