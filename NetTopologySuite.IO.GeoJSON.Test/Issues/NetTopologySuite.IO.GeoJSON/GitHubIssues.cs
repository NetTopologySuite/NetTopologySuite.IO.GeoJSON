using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    /// <summary>
    /// Class for unit tests raised from issues at <a href="https://github.com/NetTopologySuite/NetTopologySuite.IO.GeoJSON/issues"/>
    /// </summary>
    public class GitHubIssues
    {
        [NtsIssueNumber(6)]
        [Test]
        public void TestFeatureIdLostForFeatureWithoutProperties()
        {
            const string geojson =
                @"{
  ""id"": ""featureID"",
  ""type"": ""Feature"",
  ""geometry"": {
    ""type"": ""Point"",
    ""coordinates"": [42, 42]
  }
  // no ""properties"" here!!
 }";

            Feature f = null;
            Assert.That(() => f = new GeoJsonReader().Read<Feature>(geojson), Throws.Nothing);
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Attributes, Is.Not.Null);
            Assert.That(f.Attributes["id"], Is.EqualTo("featureID"));
        }

        [NtsIssueNumber(23)]
        [Test]
        public void TestGeoJsonWithComments()
        {
            const string geojson =
                @"{
  // here we go
  ""id"": ""featureID"",
  // A feature
    ""type"": ""Feature"",
    // Its geometry
    ""geometry"": {
      // look, its a point
      ""type"": ""Point"",
      // where is it
      ""coordinates"": [50.77, 6.11]
      // ah in Aix la Chapelle
    },
    // here come the properties?
    ""properties"": {
      // now what
      ""plz"": ""52xxx""
      // boring
    } 
    // booooring
  }
  // the end
}";

            Feature f = null;
            Assert.That(() => f = new GeoJsonReader().Read<Feature>(geojson), Throws.Nothing);
            Assert.That(f, Is.Not.Null);
        }
    }
}
