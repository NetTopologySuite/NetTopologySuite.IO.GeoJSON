using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    internal class GitHubIssue114
    {
        [Test, GeoJsonIssueNumber(114)]
        public void TestSerializeDoesNotChangeOrientationOfInputSequence()
        {
            const string wkt = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))";
            var g = (Polygon)new WKTReader().Read(wkt);

            var s = GeoJsonSerializer.Create(new JsonSerializerSettings(), NtsGeometryServices.Instance.CreateGeometryFactory(4326), 2, RingOrientationOption.EnforceRfc9746);
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            using var tw = new JsonTextWriter(sw);

            var seq1 = g.ExteriorRing.CoordinateSequence.Copy();
            s.Serialize(tw, g);
            var seq2 = g.ExteriorRing.CoordinateSequence.Copy();

            Assert.That(seq1, Is.EqualTo(seq2).Using(new CsEqComp()));
        }

        private class CsEqComp : IEqualityComparer<CoordinateSequence>
        {
            public bool Equals(CoordinateSequence x, CoordinateSequence y)
            {
                if (x == null ^ y == null) return false;
                if (x == y) return true;

                if (x.Count != y.Count) return false;
                if (x.HasM !=  y.HasM) return false;
                if (x.HasZ != y.HasZ) return false;

                for (int i = 0; i < x.Count; i++)
                {
                    if (x.GetX(i) != y.GetX(i)) return false;
                    if (x.GetY(i) != y.GetY(i)) return false;
                }
                return true;
            }

            public int GetHashCode([DisallowNull] CoordinateSequence obj)
            {
                return obj.GetHashCode();
            }
        }

        [Test]
        public void TestSample()
        {
            const string resourceName = "NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON.GitHubIssue114.json";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                Assert.IsNotNull(stream);
                {
                    var serializer = GeoJsonSerializer.CreateDefault();
                    using (var reader = new StreamReader(stream))
                    using (var textReader = new JsonTextReader(reader))
                    {
                        var collection = serializer.Deserialize<FeatureCollection>(textReader);
                        Assert.IsNotNull(collection);
                        Assert.AreEqual(1, collection.Count);
                        var feature = (Feature)collection[0];

                        Assert.That(feature.Geometry.IsValid);

                        var geom = feature.Geometry.Copy();

                        var sb = new StringBuilder();
                        var jw = new JsonTextWriter(new StringWriter(sb));

                        serializer.Serialize(jw, collection, typeof(FeatureCollection));

                        Assert.That(feature.Geometry.EqualsTopologically(geom));
                        Assert.That(feature.Geometry.EqualsExact(geom));
                    }

                }
            }
        }
    }
}
