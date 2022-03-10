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

        [Test]
        public void BBOXesShouldBeWritten()
        {
            DoBBOXTest(true);
        }

        [Test]
        public void BBOXesShouldNotBeWritten()
        {
            DoBBOXTest(false);
        }

        private static void DoBBOXTest(bool writeBBOX)
        {
            var fac = GeometryFactory.Default;
            var geom = fac.CreatePolygon(new Coordinate[]
            {
                new Coordinate(-89.863283,47.963199),
                new Coordinate(-89.862819,47.963009),
                new Coordinate(-89.86361,47.961897),
                new Coordinate(-89.863596,47.963326),
                new Coordinate(-89.863283,47.963199)
            });
            var options = new JsonSerializerOptions()
            {
                IgnoreNullValues = false,
                Converters = { new GeoJsonConverterFactory(fac, writeBBOX) }
            };
            string geomJson = JsonSerializer.Serialize(geom, options);
            Assert.AreEqual(writeBBOX, geomJson.Contains("bbox", StrCmp));
            var feature = new Feature(geom, new AttributesTable { { "id", 1 }, { "test", "2" } });
            string featureJson = JsonSerializer.Serialize(feature, options);
            Assert.AreEqual(writeBBOX, featureJson.Contains("bbox", StrCmp));
            var featureColl = new FeatureCollection { feature };
            string featureCollJson = JsonSerializer.Serialize(featureColl, options);
            Assert.AreEqual(writeBBOX, featureCollJson.Contains("bbox", StrCmp));
        }
    }
}
