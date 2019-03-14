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
            const string json =
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
            Assert.That(() => f = new GeoJsonReader().Read<Feature>(json), Throws.Nothing);
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Attributes, Is.Not.Null);
            Assert.That(f.Attributes["id"], Is.EqualTo("featureID"));
        }
    }
}
