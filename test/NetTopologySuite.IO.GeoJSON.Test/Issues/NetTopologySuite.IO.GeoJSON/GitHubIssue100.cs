using System;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(100)]
    public sealed class GitHubIssue100
    {
        private static string Serialize(object obj)
        {
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include };
            var serializer = GeoJsonSerializer.CreateDefault(settings);
            var sb = new StringBuilder();
            using var sw = new StringWriter(sb);
            using var jtw = new JsonTextWriter(sw);
            serializer.Serialize(jtw, obj);
            return sb.ToString();
        }

        private const StringComparison StrCmp = StringComparison.InvariantCultureIgnoreCase;

        private static readonly Geometry Geom = GeometryFactory.Default
            .CreatePolygon(new Coordinate[]
            {
                new Coordinate(-89.863283,47.963199),
                new Coordinate(-89.862819,47.963009),
                new Coordinate(-89.86361,47.961897),
                new Coordinate(-89.863596,47.963326),
                new Coordinate(-89.863283,47.963199)
            });

        [Test]
        public void BBOXIsNotWrittenForGeoms()
        {
            string geomJson = Serialize(Geom);
            Assert.AreEqual(false, geomJson.Contains("bbox", StrCmp));
        }

        [Test]
        public void BBOXIsWrittenForFeaturesAndColls()
        {
            var feature = new Feature(Geom, new AttributesTable { { "id", 1 }, { "test", "2" } });
            string featureJson = Serialize(feature);
            Assert.AreEqual(true, featureJson.Contains("bbox", StrCmp));
            var featureColl = new FeatureCollection { feature };
            string featureCollJson = Serialize(featureColl);
            Assert.AreEqual(true, featureCollJson.Contains("bbox", StrCmp));
        }
    }
}
