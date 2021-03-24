using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(82)]
    public class GitHubIssue82
    {
        [Test]
        public void TestJsonConverter()
        {
            var model = new MyModel
            {
                poly = new Polygon(new LinearRing(new[]
                {
                    new Coordinate(-100, 45),
                    new Coordinate(-98, 45),
                    new Coordinate(-99, 46),
                    new Coordinate(-100, 45),
                }))
            };
            string geoJson = JsonConvert.SerializeObject(model, Formatting.None,
                new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    //Converters = new List<JsonConverter>(new JsonConverter[]{new CoordinateConverter()})
                });

            Assert.That(geoJson, Is.EqualTo("{\"poly\":{\"type\":\"Polygon\",\"coordinates\":[[[-100.0,45.0],[-98.0,45.0],[-99.0,46.0],[-100.0,45.0]]]}}"));
        }
    }

    public class MyModel
    {
        [JsonConverter(typeof(GeometryConverter))]
        public Polygon poly { get; set; }
    }
}
