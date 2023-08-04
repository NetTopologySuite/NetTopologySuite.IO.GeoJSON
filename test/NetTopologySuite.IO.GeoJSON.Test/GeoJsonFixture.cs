using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    using System;
    using System.IO;

    using NUnit.Framework;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class GeoJsonFixture : AbstractIOFixture
    {
        public static Geometry InExpectedOrientation(Geometry gIn)
        {
            return GeoJsonSerializer.RingOrientationOption == RingOrientationOption.DoNotModify
                ? gIn
                : new GeometryEditor().Edit(gIn, new EnsureOrientationOperation(GeoJsonSerializer.RingOrientationOption));
        }

        protected override void CheckEquality(Geometry gIn, Geometry gParsed, WKTWriter writer)
        {
            base.CheckEquality(InExpectedOrientation(gIn), gParsed, writer);
        }

        protected override Geometry Read(byte[] b)
        {
            string json;
            using (var ms = new MemoryStream(b))
            {
                using (var r = new StreamReader(ms))
                    json = r.ReadToEnd();
            }

            var gjs = GeoJsonSerializer.CreateDefault();

            var j = (JObject)gjs.Deserialize(new JsonTextReader(new StringReader(json)));
            switch (j.Value<string>("type"))
            {
                case "Point":
                    return gjs.Deserialize<Point>(new JsonTextReader(new StringReader(json)));
                case "LineString":
                    return gjs.Deserialize<LineString>(new JsonTextReader(new StringReader(json)));
                case "Polygon":
                    return gjs.Deserialize<Polygon>(new JsonTextReader(new StringReader(json)));
                case "MultiPoint":
                    return gjs.Deserialize<MultiPoint>(new JsonTextReader(new StringReader(json)));
                case "MultiLineString":
                    return gjs.Deserialize<MultiLineString>(new JsonTextReader(new StringReader(json)));
                case "MultiPolygon":
                    return gjs.Deserialize<MultiPolygon>(new JsonTextReader(new StringReader(json)));
                case "GeometryCollection":
                    return gjs.Deserialize<GeometryCollection>(new JsonTextReader(new StringReader(json)));
                default:
                    throw new InvalidOperationException();
            }
        }

        protected override byte[] Write(Geometry gIn)
        {
            var gjw = new GeoJsonWriter();
            string res = gjw.Write(gIn);
            using (var ms = new MemoryStream(res.Length))
            {
                using (var s = new StreamWriter(ms))
                    s.Write(res);
                return ms.ToArray();
            }
        }

        [Ignore("GeometryCollections containing GeometryCollections is not implemented")]
        public override void TestGeometryCollection()
        {
            base.TestGeometryCollection();
        }

        private sealed class EnsureOrientationOperation : GeometryEditor.IGeometryEditorOperation
        {
            private readonly RingOrientationOption _orientation;

            public EnsureOrientationOperation(RingOrientationOption orientation)
            {
                _orientation = orientation;
            }

            public Geometry Edit(Geometry geometry, GeometryFactory factory)
            {
                if (geometry is not Polygon polygon || polygon.IsEmpty)
                {
                    return geometry;
                }

                var rings = new LinearRing[polygon.NumInteriorRings + 1];
                rings[0] = polygon.Shell;
                polygon.Holes.CopyTo(rings.AsSpan(1));
                bool[] shouldBeCCW = new bool[rings.Length];
                shouldBeCCW.AsSpan().Fill(_orientation == RingOrientationOption.NtsGeoJsonV2);
                shouldBeCCW[0] = !shouldBeCCW[0];
                bool anyModified = false;
                for (int i = 0; i < rings.Length; i++)
                {
                    if (rings[i].IsCCW != shouldBeCCW[i])
                    {
                        rings[i] = factory.CreateLinearRing(rings[i].CoordinateSequence.Reversed());
                        anyModified = true;
                    }
                }

                return anyModified
                    ? factory.CreatePolygon(rings[0], rings[1..])
                    : geometry;
            }
        }
    }
}
