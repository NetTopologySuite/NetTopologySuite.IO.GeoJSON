using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
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

        [NtsIssueNumber(16)]
        [Test]
        public void TestDefaultSridOfDeserializedGeometryIs4326()
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
            Assert.That(f.Geometry, Is.Not.Null);
            Assert.That(f.Geometry.SRID, Is.EqualTo(4326));

            f = null;
            var gf = new GeometryFactory(new PrecisionModel(), 10010);
            Assert.That(() => f = new GeoJsonReader(gf, new JsonSerializerSettings()).Read<Feature>(geojson), Throws.Nothing);
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Geometry, Is.Not.Null);
            Assert.That(f.Geometry.SRID, Is.EqualTo(10010));

            f = null;
            var s = GeoJsonSerializer.CreateDefault();
            f = s.Deserialize<Feature>(new JsonTextReader(new StringReader(geojson)));
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Geometry, Is.Not.Null);
            Assert.That(f.Geometry.SRID, Is.EqualTo(4326));
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

        [NtsIssueNumber(19)]
        [Test]
        public void TestGeoJsonWriterWritesEmptyFeatureCollection()
        {
            var w = new GeoJsonWriter();
            var r = new GeoJsonReader();

            var fc = new FeatureCollection();

            string geoJson = null;
            Assert.That(() => geoJson = w.Write(fc), Throws.Nothing);
            Assert.That(geoJson, Is.Not.Null.Or.Empty);
            fc = null;
            Assert.That(() => fc = r.Read<FeatureCollection>(geoJson), Throws.Nothing);
            Assert.That(fc, Is.Not.Null);

            var f = new Feature();
            Assert.That(() => geoJson = w.Write(f), Throws.Nothing);
            Assert.That(geoJson, Is.Not.Null.Or.Empty);
            f = null;
            Assert.That(() => f = r.Read<Feature>(geoJson), Throws.Nothing);
            Assert.That(f, Is.Not.Null);

        }

        [NtsIssueNumber(20)]
        [Test]
        public void TestWriteFeatureCollectionWithFirstFeatureGeometryNull()
        {
            // Setup
            var geoJsonWriter = new GeoJsonWriter();

            var featureJson = "{\"type\": \"Feature\",\"geometry\": {\"type\": \"LineString\",\"coordinates\": [[0,0],[2,2],[3,2]]},\"properties\": {\"key\": \"value\"}}";
            var notNullGeometryFeature = new GeoJsonReader().Read<Feature>(featureJson);

            var attributesTable = new AttributesTable { { "key", "value" } };
            IGeometry geometry = null;
            var nullGeometryFeature = new Feature(geometry, attributesTable);

            var features_notNullFirst = new Collection<IFeature>
            {
                notNullGeometryFeature,
                nullGeometryFeature
            };

            var features_nullFirst = new Collection<IFeature>
            {
                nullGeometryFeature,
                notNullGeometryFeature
            };

            var featureCollection_notNullFirst = new FeatureCollection(features_notNullFirst);
            var featureCollection_nullFirst = new FeatureCollection(features_nullFirst);

            // Act
            TestDelegate write_notNullFirst = () => geoJsonWriter.Write(featureCollection_notNullFirst);
            TestDelegate write_nullFirst = () => geoJsonWriter.Write(featureCollection_nullFirst);

            Assert.That(write_notNullFirst, Throws.Nothing);
            Assert.That(write_nullFirst, Throws.Nothing);
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
