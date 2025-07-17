using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;
using System.Text.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    public class MyModel
    {
        public IFeature Feature { get; set; }
    }

    internal class Issue154
    {
        const string myFeatureJson = @"{
  ""feature"": {
    ""type"": ""Feature"",
    ""geometry"": {
      ""type"": ""Point"",
      ""coordinates"": [102.0, 0.5]
    },
    ""properties"": {
      ""name"": ""Test Point""
    }
  }
}";

        [Test, GeoJsonIssueNumber(154)]
        public void TestDeserialization()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            options.Converters.Add(new GeoJsonConverterFactory());

            MyModel mm = null;
            Assert.That(() => mm = JsonSerializer.Deserialize<MyModel>(myFeatureJson, options), Throws.Nothing); // => throws the invalid cast exception
            Assert.That(mm, Is.Not.Null);
            Assert.That(mm.Feature.GetType().Name, Is.EqualTo("StjFeature"));
            Assert.That(mm.Feature.Geometry.OgcGeometryType, Is.EqualTo(Geometries.OgcGeometryType.Point));
        }
    }
}
