using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    using System;
    using System.IO;

    using NUnit.Framework;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class GeoJsonFixture : AbstractIOFixture
    {
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
    }
}
