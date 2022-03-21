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
        private static readonly Geometry Pnt = GF.CreatePoint(
            new Coordinate(-80, 40));
        private static readonly Geometry Coll = GF.CreateGeometryCollection(
            new[] { Poly, Pnt });
        private static readonly Geometry CollSamePoints = GF.CreateGeometryCollection(
            new[] { Pnt, Pnt.Copy(), Pnt.Copy(), Pnt.Copy(), Pnt.Copy() });
        private static readonly Geometry Empty = GF.CreatePoint();

        private static void DoBBOXTest(Geometry g, bool writeBBOX, bool ignoreNull)
        {
            Assert.That(g, Is.Not.Null);
            var options = new JsonSerializerOptions()
            {
                IgnoreNullValues = ignoreNull,
                Converters = { new GeoJsonConverterFactory(GF, writeBBOX) }
            };
            string geomJson = JsonSerializer.Serialize(g, options);
            Console.WriteLine($"GEOM: {geomJson}");
            TestJsonTextForBBOX(geomJson, g, writeBBOX, ignoreNull);

            var feature = new Feature(g, new AttributesTable { { "id", 1 }, { "test", "2" } })
            {
                BoundingBox = g.EnvelopeInternal
            };
            string featureJson = JsonSerializer.Serialize(feature, options);
            Console.WriteLine($"FEAT: {featureJson}");
            TestJsonTextForBBOX(featureJson, g, writeBBOX, ignoreNull);

            var featureColl = new FeatureCollection { feature };
            featureColl.BoundingBox = feature.BoundingBox;
            string featureCollJson = JsonSerializer.Serialize(featureColl, options);
            Console.WriteLine($"COLL: {featureCollJson}");
            TestJsonTextForBBOX(featureCollJson, g, writeBBOX, ignoreNull);
        }

        private static void TestJsonTextForBBOX(string json, Geometry g, bool writeBBOX, bool ignoreNull)
        {
            if (!writeBBOX)
            {
                Assert.AreEqual(false, json.Contains("bbox", StrCmp));
                Assert.That(json.IndexOf("null", StrCmp), Is.EqualTo(-1));
                return;
            }

            if (ignoreNull)
            {
                if (g.IsEmpty)
                {
                    Assert.AreEqual(false, json.Contains("bbox", StrCmp));
                    Assert.That(json.IndexOf("null", StrCmp), Is.EqualTo(-1));
                }
                else
                {
                    Assert.AreEqual(true, json.Contains("bbox", StrCmp));
                    Assert.That(json.IndexOf("null", StrCmp), Is.EqualTo(-1));
                }
                return;
            }

            if (g.IsEmpty)
            {
                Assert.AreEqual(true, json.Contains("bbox", StrCmp));
                Assert.That(json.IndexOf("null", StrCmp), Is.Not.EqualTo(-1));
            }
            else
            {
                Assert.AreEqual(true, json.Contains("bbox", StrCmp));
                Assert.That(json.IndexOf("null", StrCmp), Is.EqualTo(-1));
            }
        }

        [Test]
        public void BBOXForPolygonShouldBeWritten()
        {
            DoBBOXTest(g: Poly, writeBBOX: true, ignoreNull: false);
            DoBBOXTest(g: Poly, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void BBOXForPolygonShouldNotBeWritten()
        {
            DoBBOXTest(g: Poly, writeBBOX: false, ignoreNull: false);
            DoBBOXTest(g: Poly, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForPointShouldNotBeWrittenEvenIfBBoxFlagIsTrue()
        {
            DoBBOXTest(g: Pnt, writeBBOX: true, ignoreNull: false);
            DoBBOXTest(g: Pnt, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void BBOXForPointShouldNotBeWritten()
        {
            DoBBOXTest(g: Pnt, writeBBOX: false, ignoreNull: false);
            DoBBOXTest(g: Pnt, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForGeomCollShouldBeWritten()
        {
            DoBBOXTest(g: Coll, writeBBOX: true, ignoreNull: false);
            DoBBOXTest(g: Coll, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void BBOXForGeomCollShouldNotBeWritten()
        {
            DoBBOXTest(g: Coll, writeBBOX: false, ignoreNull: false);
            DoBBOXTest(g: Coll, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForGeomCollMadeOfSamePointsShouldBeWritten()
        {
            DoBBOXTest(g: CollSamePoints, writeBBOX: true, ignoreNull: false);
            DoBBOXTest(g: CollSamePoints, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void NullBBOXForGeomCollMadeOfSamePointsShouldNotBeWritten()
        {
            DoBBOXTest(g: CollSamePoints, writeBBOX: false, ignoreNull: false);
            DoBBOXTest(g: CollSamePoints, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForEmptyGeomShouldBeWritten()
        {
            DoBBOXTest(g: Empty, writeBBOX: true, ignoreNull: false);
        }

        [Test]
        public void BBOXForEmptyGeomShouldNotBeWritten()
        {
            DoBBOXTest(g: Empty, writeBBOX: false, ignoreNull: false);
        }

        [Test]
        public void NullBBOXForEmptyGeomShouldBeIgnored()
        {
            DoBBOXTest(g: Empty, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void NullBBOXForEmptyGeomShouldBeNotIgnored()
        {
            DoBBOXTest(g: Empty, writeBBOX: true, ignoreNull: false);
        }
    }
}
