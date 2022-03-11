using System;
using System.Text.Json;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(100)]
    public sealed class Issue100
    {
        private const StringComparison StrCmp = StringComparison.InvariantCultureIgnoreCase;

        private static readonly GeometryFactory GF = GeometryFactory.Default;
        private static readonly Geometry Poly = GF.CreatePolygon(
            new Coordinate[]
            {
                    new Coordinate(-89.863283,47.963199),
                    new Coordinate(-89.862819,47.963009),
                    new Coordinate(-89.86361,47.961897),
                    new Coordinate(-89.863596,47.963326),
                    new Coordinate(-89.863283,47.963199)
            });
        private static readonly Geometry Pt = GF.CreatePoint(
            new Coordinate(-80, 40));
        private static readonly Geometry Coll = GF.CreateGeometryCollection(
            new[] { Poly, Pt });
        private static readonly Geometry CollSamePoints = GF.CreateGeometryCollection(
            new[] { Pt, Pt.Copy(), Pt.Copy(), Pt.Copy(), Pt.Copy() });

        private static void DoBBOXTest(Geometry geom, bool writeBBOX)
        {
            Assert.That(geom, Is.Not.Null);
            var options = new JsonSerializerOptions()
            {
                IgnoreNullValues = false,
                Converters = { new GeoJsonConverterFactory(GF, writeBBOX) }
            };
            string geomJson = JsonSerializer.Serialize(geom, options);
            Console.WriteLine($"GEOM: {geomJson}");
            Assert.AreEqual(writeBBOX, geomJson.Contains("bbox", StrCmp));

            var feature = new Feature(geom, new AttributesTable { { "id", 1 }, { "test", "2" } });
            string featureJson = JsonSerializer.Serialize(feature, options);
            Console.WriteLine($"FEAT: {featureJson}");
            Assert.AreEqual(writeBBOX, featureJson.Contains("bbox", StrCmp));
            if (writeBBOX)
                Assert.That(featureJson.IndexOf("null", StrCmp), Is.EqualTo(-1));

            var featureColl = new FeatureCollection { feature };
            string featureCollJson = JsonSerializer.Serialize(featureColl, options);
            Console.WriteLine($"COLL: {featureCollJson}");
            Assert.AreEqual(writeBBOX, featureCollJson.Contains("bbox", StrCmp));
            if (writeBBOX)
                Assert.That(featureCollJson.IndexOf("null", StrCmp), Is.EqualTo(-1));
        }

        [Test]
        public void BBOXForPolygonShouldBeWritten()
        {
            DoBBOXTest(Poly, true);
        }

        [Test]
        public void BBOXForPolygonShouldNotBeWritten()
        {
            DoBBOXTest(Poly, false);
        }

        [Test]
        public void BBOXForPointShouldNotBeWrittenEvenIfBBoxFlagIsTrue()
        {
            DoBBOXTest(Pt, true);
        }

        [Test]
        public void BBOXForPointShouldNotBeWritten()
        {
            DoBBOXTest(Pt, false);
        }

        [Test]
        public void BBOXForGeomCollShouldBeWritten()
        {
            DoBBOXTest(Coll, true);
        }

        [Test]
        public void BBOXForGeomCollShouldNotBeWritten()
        {
            DoBBOXTest(Coll, false);
        }

        [Test]
        public void BBOXForGeomCollMadeOfSamePointsShouldBeWritten()
        {
            DoBBOXTest(CollSamePoints, true);
        }

        [Test]
        public void BBOXForGeomCollMadeOfSamePointsShouldNotBeWritten()
        {
            DoBBOXTest(CollSamePoints, false);
        }
    }
}
