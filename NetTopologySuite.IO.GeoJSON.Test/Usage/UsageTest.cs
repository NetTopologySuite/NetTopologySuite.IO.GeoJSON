using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Usage
{
    public class UsageTest
    {
        private IGeometryFactory _factory = NtsGeometryServices.Instance.CreateGeometryFactory(31466);

        [Test]
        public void Test()
        {
            var road = new Road();
            road.Geometry = _factory.CreateLineString(_factory.CoordinateSequenceFactory.Create(
                new[] {new Coordinate(2500000, 5600000), new Coordinate(2500100, 5600010)}));
            road.Name = "Teststrecke";
            road.OneWay = false;
            road.NumLanes = 6;

            var sr = GeoJsonSerializer.Create(_factory);
            var sb = new StringBuilder();
            sr.Serialize(new JsonTextWriter(new StringWriter(sb)), road, typeof(Road));
            Console.WriteLine(sb.ToString());

            var r2 = new Road((IFeature)sr.Deserialize<Feature>(new JsonTextReader(new StringReader(sb.ToString()))));

        }
    }
}
