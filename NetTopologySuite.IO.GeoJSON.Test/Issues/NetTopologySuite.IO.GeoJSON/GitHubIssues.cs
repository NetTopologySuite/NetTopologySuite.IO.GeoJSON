using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
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

        [NtsIssueNumber(7)]
        [Test]
        public void TestReaderFailsToParseProperties()
        {
            const string geojson = @"{
  ""type"": ""Feature"",
  ""geometry"": {
    ""type"": ""Point"",
    ""coordinates"": [
      12.5851472,
      55.68323837
    ]
  },
  ""crs"": {
    ""type"": ""name"",
    ""properties"": {
      ""name"": ""EPSG:4326""
    }
  },
  ""properties"": {
    ""id"": ""0a3f507a-b2e6-32b8-e044-0003ba298018"",
    ""status"": 1,
    ""vejkode"": ""4112"",
    ""vejnavn"": ""Landgreven"",
    ""adresseringsvejnavn"": ""Landgreven"",
    ""husnr"": ""10"",
    ""supplerendebynavn"": null,
    ""postnr"": ""1301"",
    ""postnrnavn"": ""København K"",
    ""kommunekode"": ""0101""
  }
}";

            Feature f = null;
            Assert.That(() => f = new GeoJsonReader().Read<Feature>(geojson), Throws.Nothing);
            Assert.That(f.Attributes, Is.Not.Null);
            Assert.That(f.Attributes.Count, Is.EqualTo(10));
            Assert.That(FeatureExtensions.HasID(f), Is.True);
            Assert.That(FeatureExtensions.ID(f), Is.EqualTo("0a3f507a-b2e6-32b8-e044-0003ba298018"));
        }


        [NtsIssueNumber(18)]
        [Test]
        public void TestDeserializationOfNullGeometries()
        {
            var fooS = new Foo {Name = "fooS"};
            var s = GeoJsonSerializer.Create();
            var sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), fooS);
            Foo fooD = null;
            Assert.That(() => fooD = s.Deserialize<Foo>(new JsonTextReader(new StringReader(sb.ToString()))), Throws.Nothing);
            Assert.That(fooD, Is.Not.Null);
            Assert.That(fooD.Name, Is.EqualTo("fooS"));
        }
        class Foo
        {
            public string Name { get; set; }

            [Newtonsoft.Json.JsonProperty(PropertyName = "geometry",
                ItemConverterType = typeof(global::NetTopologySuite.IO.Converters.GeometryConverter))]
            public Point Point {get; set;} // it can be null
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
