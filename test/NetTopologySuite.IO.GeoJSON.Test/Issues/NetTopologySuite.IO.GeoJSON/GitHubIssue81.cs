using System;
using System.IO;

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

using Newtonsoft.Json;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(81)]
    public sealed class GitHubIssue81
    {
        [Test]
        public void TestSpecialCoordinatesArrayFormatting()
        {
            //RunTest(GeoJsonSerializer.CreateDefault);
            RunTest((outer, inner) => GeoJsonSerializer.Create(outer, GeometryFactory.Default, 2, RingOrientationOption.NtsGeoJsonV2, inner));
        }

        private static void RunTest(Func<JsonSerializerSettings, JsonSerializerSettings, JsonSerializer> createSerializer)
        {
            var outer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            var inner = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
            };

            const string Data = @"{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""MultiPolygon"",""coordinates"":[[[[0.0,0.0],[0.0,1.0],[1.0,1.0],[1.0,0.0],[0.0,0.0]]]]
      }
    },
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""MultiPolygon"",""coordinates"":[[[[2.0,2.0],[2.0,4.0],[4.0,4.0],[4.0,2.0],[2.0,2.0]]]]
      }
    }
  ]
}";
            var serializer = createSerializer(outer, inner);
            FeatureCollection fc;
            using (var sr = new StringReader(Data))
            using (var jtr = new JsonTextReader(sr))
            {
                fc = serializer.Deserialize<FeatureCollection>(jtr);
            }

            string data2;
            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, fc);
                data2 = sw.ToString();
            }

            Assert.That(data2, Is.EqualTo(Data));
        }
    }
}
