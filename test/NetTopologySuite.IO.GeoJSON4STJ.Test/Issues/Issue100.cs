using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        private static void DoBBoxTest(Geometry g, bool writeBBOX, bool ignoreNull)
        {
            if (ignoreNull)
            {
                Run(options => options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault);
                Run(options => options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

#pragma warning disable SYSLIB0020
                Run(options => options.IgnoreNullValues = true);
#pragma warning restore SYSLIB0020
            }
            else
            {
                Run(options => options.DefaultIgnoreCondition = JsonIgnoreCondition.Never);
            }

            void Run(Action<JsonSerializerOptions> setIgnoreNull)
            {
                Assert.That(g, Is.Not.Null);
                var options = new JsonSerializerOptions()
                {
                    Converters = { new GeoJsonConverterFactory(GF, writeBBOX) }
                };
                setIgnoreNull(options);

                string geomJson = JsonSerializer.Serialize(g, options);
                Console.WriteLine($"GEOM: {geomJson}");
                TestJsonWithValidBBox(geomJson, g, writeBBOX, ignoreNull);

                var feature = new Feature(g, new AttributesTable { { "id", 1 }, { "test", "2" } });
                string featureJson = JsonSerializer.Serialize(feature, options);
                Console.WriteLine($"FEAT: {featureJson}");
                TestJsonWithNullBBox(featureJson, g, writeBBOX, ignoreNull);
                feature.BoundingBox = g.EnvelopeInternal;
                string featureJsonBBox = JsonSerializer.Serialize(feature, options);
                Console.WriteLine($"FEAT+BBox: {featureJsonBBox}");
                TestJsonWithValidBBox(featureJsonBBox, g, writeBBOX, ignoreNull);

                var featureColl = new FeatureCollection { feature };
                string featureCollJson = JsonSerializer.Serialize(featureColl, options);
                Console.WriteLine($"COLL: {featureCollJson}");
                TestJsonWithNullBBox(featureCollJson, g, writeBBOX, ignoreNull);
                featureColl.BoundingBox = feature.BoundingBox;
                string featureCollJsonBBox = JsonSerializer.Serialize(featureColl, options);
                Console.WriteLine($"COLL+BBox: {featureCollJsonBBox}");
                TestJsonWithValidBBox(featureCollJsonBBox, g, writeBBOX, ignoreNull);
            }
        }

        // NOTE: feature (and feature coll) bbox is always NULL
        private static void TestJsonWithNullBBox(string json, Geometry g, bool writeBBOX, bool ignoreNull)
        {
            if (!writeBBOX)
            {
                // bbox never written
                Assert.AreEqual(false, json.Contains("bbox", StrCmp));
                Assert.That(json.IndexOf("null", StrCmp), Is.EqualTo(-1));
                return;
            }

            // null bbox written only if "ignoreNull" is false
            Assert.AreEqual(!ignoreNull, json.Contains("\"bbox\":null", StrCmp));
        }

        // NOTE: feature (and feature coll) bbox is NOT NULL when geom is NOT EMPTY
        private static void TestJsonWithValidBBox(string json, Geometry g, bool writeBBOX, bool ignoreNull)
        {
            if (!writeBBOX)
            {
                // bbox never written
                Assert.AreEqual(false, json.Contains("bbox", StrCmp));
                Assert.That(json.IndexOf("null", StrCmp), Is.EqualTo(-1));
                return;
            }


            if (g.IsEmpty)
            {
                // null bbox written only if "ignoreNull" is false
                Assert.AreEqual(!ignoreNull, json.Contains("\"bbox\":null", StrCmp));
            }
            else
            {
                // valid bbox written
                Assert.AreEqual(true, json.Contains("bbox", StrCmp));
                Assert.That(json.IndexOf("null", StrCmp), Is.EqualTo(-1));
            }
        }

        [Test]
        public void BBOXForPolygonShouldBeWritten()
        {
            DoBBoxTest(g: Poly, writeBBOX: true, ignoreNull: false);
            DoBBoxTest(g: Poly, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void BBOXForPolygonShouldNotBeWritten()
        {
            DoBBoxTest(g: Poly, writeBBOX: false, ignoreNull: false);
            DoBBoxTest(g: Poly, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForPointShouldNotBeWrittenEvenIfBBoxFlagIsTrue()
        {
            DoBBoxTest(g: Pnt, writeBBOX: true, ignoreNull: false);
            DoBBoxTest(g: Pnt, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void BBOXForPointShouldNotBeWritten()
        {
            DoBBoxTest(g: Pnt, writeBBOX: false, ignoreNull: false);
            DoBBoxTest(g: Pnt, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForGeomCollShouldBeWritten()
        {
            DoBBoxTest(g: Coll, writeBBOX: true, ignoreNull: false);
            DoBBoxTest(g: Coll, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void BBOXForGeomCollShouldNotBeWritten()
        {
            DoBBoxTest(g: Coll, writeBBOX: false, ignoreNull: false);
            DoBBoxTest(g: Coll, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForGeomCollMadeOfSamePointsShouldBeWritten()
        {
            DoBBoxTest(g: CollSamePoints, writeBBOX: true, ignoreNull: false);
            DoBBoxTest(g: CollSamePoints, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void NullBBOXForGeomCollMadeOfSamePointsShouldNotBeWritten()
        {
            DoBBoxTest(g: CollSamePoints, writeBBOX: false, ignoreNull: false);
            DoBBoxTest(g: CollSamePoints, writeBBOX: false, ignoreNull: true);
        }

        [Test]
        public void BBOXForEmptyGeomShouldBeWritten()
        {
            DoBBoxTest(g: Empty, writeBBOX: true, ignoreNull: false);
        }

        [Test]
        public void BBOXForEmptyGeomShouldNotBeWritten()
        {
            DoBBoxTest(g: Empty, writeBBOX: false, ignoreNull: false);
        }

        [Test]
        public void NullBBOXForEmptyGeomShouldBeIgnored()
        {
            DoBBoxTest(g: Empty, writeBBOX: true, ignoreNull: true);
        }

        [Test]
        public void NullBBOXForEmptyGeomShouldBeNotIgnored()
        {
            DoBBoxTest(g: Empty, writeBBOX: true, ignoreNull: false);
        }
    }
}
