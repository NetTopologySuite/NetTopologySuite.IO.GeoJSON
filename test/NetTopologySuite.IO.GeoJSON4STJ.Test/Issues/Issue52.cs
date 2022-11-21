using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.IO.GeoJSON4STJ.Test.Converters;
using NUnit.Framework;
using System;
using System.Text.Json;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(52)]
    internal class Issue52 : SandDTest<Geometry>
    {
        [GeoJsonIssueNumber(52)]
        [TestCase(RingOrientationOption.DoNotModify, "POLYGON((0 0, 10 0, 0 10, 0 0), (1 1, 8 1, 1 8, 1 1))")]
        [TestCase(RingOrientationOption.DoNotModify, "POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 8 1, 1 8, 1 1))")]
        [TestCase(RingOrientationOption.DoNotModify, "POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 1 8, 8 1, 1 1))")]
        [TestCase(RingOrientationOption.EnforceRfc9746, "POLYGON((0 0, 10 0, 0 10, 0 0), (1 1, 8 1, 1 8, 1 1))")]
        [TestCase(RingOrientationOption.EnforceRfc9746, "POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 8 1, 1 8, 1 1))")]
        [TestCase(RingOrientationOption.EnforceRfc9746, "POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 1 8, 8 1, 1 1))")]
        [TestCase(RingOrientationOption.NtsGeoJsonV2, "POLYGON((0 0, 10 0, 0 10, 0 0), (1 1, 8 1, 1 8, 1 1))")]
        [TestCase(RingOrientationOption.NtsGeoJsonV2, "POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 8 1, 1 8, 1 1))")]
        [TestCase(RingOrientationOption.NtsGeoJsonV2, "POLYGON((0 0, 0 10, 10 0, 0 0), (1 1, 1 8, 8 1, 1 1))")]
        public void TestIssue52(RingOrientationOption roo, string wkt)
        {
            var rdr = new WKTReader();
            var poly1 = (Polygon)rdr.Read(wkt);

            var options = new JsonSerializerOptions
                { ReadCommentHandling = JsonCommentHandling.Skip };

            options.Converters.Add(new GeoJsonConverterFactory(
                NtsGeometryServices.Instance.CreateGeometryFactory(4326),
                false, GeoJsonConverterFactory.DefaultIdPropertyName,
                roo));

            string json = ToJsonString(poly1, options);

            var poly2 = (Polygon)FromJsonString(json, options);

            switch (roo)
            {
                case RingOrientationOption.DoNotModify:
                    CheckRingOrientation(poly2.ExteriorRing, GetRingOrientation(poly1.ExteriorRing));
                    for (int i = 0; i < poly2.NumInteriorRings; i++)
                        CheckRingOrientation(poly2.GetInteriorRingN(i), GetRingOrientation(poly1.GetInteriorRingN(i)));
                    break;

                case RingOrientationOption.EnforceRfc9746:
                    CheckRingOrientation(poly2.ExteriorRing, OrientationIndex.CounterClockwise);
                    for (int i = 0; i < poly2.NumInteriorRings; i++)
                        CheckRingOrientation(poly2.GetInteriorRingN(i), OrientationIndex.Clockwise);
                    break;

                case RingOrientationOption.NtsGeoJsonV2:
                    CheckRingOrientation(poly2.ExteriorRing, OrientationIndex.Clockwise);
                    for (int i = 0; i < poly2.NumInteriorRings; i++)
                        CheckRingOrientation(poly2.GetInteriorRingN(i), OrientationIndex.CounterClockwise);
                    break;
            }
        }

        private static void CheckRingOrientation(LineString ring, OrientationIndex ori)
        {
            Assert.That(GetRingOrientation(ring), Is.EqualTo(ori));
        }


        private static OrientationIndex GetRingOrientation(LineString ring)
        {
            if (!(ring is LinearRing lr))
                throw new ArgumentException(nameof(ring));

            return lr.IsCCW ? OrientationIndex.CounterClockwise : OrientationIndex.Clockwise;
        }
    }
}
