using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.IO.GeoJSON4STJ.Test.Converters;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    internal class Issue114 : SandDTest<Geometry>
    {
        [Test, GeoJsonIssueNumber(114)]
        public void TestSerializeDoesNotChangeOrientationOfInputSequence()
        {
            const string wkt = "POLYGON((0 0, 0 10, 10 10, 10 0, 0 0))";
            var g = (Polygon)new WKTReader().Read(wkt);

            var seq1 = g.ExteriorRing.CoordinateSequence.Copy();
            var opt = new System.Text.Json.JsonSerializerOptions();
            opt.Converters.Add(new GeoJsonConverterFactory(
                NtsGeometryServices.Instance.CreateGeometryFactory(4326),
                false, GeoJsonConverterFactory.DefaultIdPropertyName,
                RingOrientationOption.EnforceRfc9746));

            using var ms = new MemoryStream();
            Serialize(ms, g, opt);
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
                if (x.HasM != y.HasM) return false;
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
    }
}
