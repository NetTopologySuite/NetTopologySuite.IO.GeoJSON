using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.Triangulate;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    /// <summary>
    /// Class for unit tests raised from issues at <a href="https://github.com/NetTopologySuite/NetTopologySuite.IO.GeoJSON/issues"/>
    /// </summary>
    public class GitHubIssues
    {
        [GeoJsonIssueNumber(6)]
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

        [GeoJsonIssueNumber(7)]
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

        [GeoJsonIssueNumber(16)]
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
            Assert.That(() => f = new GeoJsonReader(gf, new JsonSerializerSettings()).Read<Feature>(geojson),
                Throws.Nothing);
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

        [GeoJsonIssueNumber(18)]
        [Test]
        public void TestDeserializationOfNullGeometries()
        {
            var fooS = new Foo {Name = "fooS"};
            var s = GeoJsonSerializer.Create();
            var sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), fooS);
            Foo fooD = null;
            Assert.That(() => fooD = s.Deserialize<Foo>(new JsonTextReader(new StringReader(sb.ToString()))),
                Throws.Nothing);
            Assert.That(fooD, Is.Not.Null);
            Assert.That(fooD.Name, Is.EqualTo("fooS"));
        }

        class Foo
        {
            public string Name { get; set; }

            [Newtonsoft.Json.JsonProperty(PropertyName = "geometry",
                ItemConverterType = typeof(global::NetTopologySuite.IO.Converters.GeometryConverter))]
            public Point Point { get; set; } // it can be null
        }

        [GeoJsonIssueNumber(19)]
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

        [GeoJsonIssueNumber(20)]
        [Test]
        public void TestWriteFeatureCollectionWithFirstFeatureGeometryNull()
        {
            // Setup
            var geoJsonWriter = new GeoJsonWriter();

            string featureJson =
                "{\"type\": \"Feature\",\"geometry\": {\"type\": \"LineString\",\"coordinates\": [[0,0],[2,2],[3,2]]},\"properties\": {\"key\": \"value\"}}";
            var notNullGeometryFeature = new GeoJsonReader().Read<Feature>(featureJson);

            var attributesTable = new AttributesTable {{"key", "value"}};
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

        [GeoJsonIssueNumber(23)]
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

        [GeoJsonIssueNumber(20)]
        [Test]
        public void TestCoordinatesWithNullOrdinates()
        {
            const string json0 = "{ \"type\":\"Point\",\"coordinates\":[null,null]} }";
            const string json1 = "{ \"type\":\"Point\",\"coordinates\":[null,1]} }";
            const string json2 = "{ \"type\":\"Point\",\"coordinates\":[2,null]} }";
            const string json3 = "{ \"type\":\"Point\",\"coordinates\":[2,1,null]} }";

            IGeometry g = null;
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json0), Throws.Nothing, "null, null");
            Assert.That(g is IPoint);
            Assert.That(g.IsEmpty);
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json1), Throws.Nothing, "null, 1");
            Assert.That(g is IPoint);
            Assert.That(!g.IsEmpty);
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json2), Throws.Nothing, "2, null");
            Assert.That(g is IPoint);
            Assert.That(!g.IsEmpty);
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json3), Throws.Nothing, "2, 1, null");
            Assert.That(g is IPoint);
            Assert.That(!g.IsEmpty);
        }

        [GeoJsonIssueNumber(20)]
        [Test]
        public void TestNullValueHandlingEnvelope()
        {
            var factory = new GeometryFactory(new PrecisionModel(100), 4326);

            IFeature value = new Feature(factory.CreatePoint(new Coordinate(23, 56)), new AttributesTable());
            var writer = new GeoJsonWriter();
            writer.SerializerSettings.NullValueHandling = NullValueHandling.Include;

            Console.WriteLine(writer.Write(value));
        }

        [GeoJsonIssueNumber(27)]
        [Test]
        public void TestOutputPrecision()
        {
            var coords = new[]
            {
                new Coordinate(0.001, 0.001),
                new Coordinate(10.1, 0.002),
                new Coordinate(10, 10.1),
                new Coordinate(0.05, 9.999),
                new Coordinate(0.001, 0.001)
            };

            // Create a factory with scale = 10
            var factory = new GeometryFactory(new PrecisionModel(10), 4326);

            // Creating the polygon with the above factory
            var polygon = factory.CreatePolygon(coords);

            var serializer = GeoJsonSerializer.Create(factory);
            var writer = new StringWriter();
            serializer.Serialize(writer, polygon);

            string json = writer.ToString();
            Assert.That(json, Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[0.0,0.0],[10.1,0.0],[10.0,10.1],[0.1,10.0],[0.0,0.0]]]}"));

            var gjWriter = new GeoJsonWriter();
            string json2 = gjWriter.Write(polygon);
            Assert.That(json2, Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[0.0,0.0],[10.1,0.0],[10.0,10.1],[0.1,10.0],[0.0,0.0]]]}"));
        }
        [GeoJsonIssueNumber(27)]
        [Test]
        public void TestOutputDimension()
        {
            var coords = new[]
            {
                new Coordinate(0.001, 0.001, 3),
                new Coordinate(10.1, 0.002, 3),
                new Coordinate(10, 10.1, 3),
                new Coordinate(0.05, 9.999, 3),
                new Coordinate(0.001, 0.001, 3)
            };

            // Create a factory with scale = 10
            var factory = new GeometryFactory(new PrecisionModel(10), 4326);

            // Creating the polygon with the above factory
            var polygon = factory.CreatePolygon(coords);

            var serializer2 = GeoJsonSerializer.Create(factory, 2);
            var serializer3 = GeoJsonSerializer.Create(factory, 3);
            var writer = new StringWriter();
            serializer2.Serialize(writer, polygon);

            string json2 = writer.ToString();
            Assert.That(json2, Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[0.0,0.0],[10.1,0.0],[10.0,10.1],[0.1,10.0],[0.0,0.0]]]}"));

            writer = new StringWriter();
            serializer3.Serialize(writer, polygon);
            string json3 = writer.ToString();
            Assert.That(json3, Is.EqualTo("{\"type\":\"Polygon\",\"coordinates\":[[[0.0,0.0,3.0],[10.1,0.0,3.0],[10.0,10.1,3.0],[0.1,10.0,3.0],[0.0,0.0,3.0]]]}"));
        }

        [GeoJsonIssueNumber(27)]
        [Test]
        public void TestInputPrecision()
        {
            // Create a factory with scale = 10
            var factory = new GeometryFactory(new PrecisionModel(10), 4326);

            var serializer = GeoJsonSerializer.Create(factory);
            var reader = new JsonTextReader(new StringReader("{\"type\":\"Polygon\",\"coordinates\":[[[0.001,0.001],[10.1,0.002],[10.0,10.1],[0.05,9.999],[0.001,0.001]]]}"));
            var geom = serializer.Deserialize<IGeometry>(reader);
            Assert.That(geom.AsText(), Is.EqualTo("POLYGON ((0 0, 10.1 0, 10 10.1, 0.1 10, 0 0))"));
        }

        [GeoJsonIssueNumber(27)]
        [Test]
        public void TestInputDimension()
        {
            var coords = new[]
            {
                new Coordinate(0.001, 0.001),
                new Coordinate(10.1, 0.002),
                new Coordinate(10, 10.1),
                new Coordinate(0.05, 9.999),
                new Coordinate(0.001, 0.001)
            };

            // Create a factory with scale = 10
            var factory = new GeometryFactory(new PrecisionModel(10), 4326);

            var serializer3 = GeoJsonSerializer.Create(factory, 3);
            var reader = new JsonTextReader(new StringReader("{\"type\":\"Polygon\",\"coordinates\":[[[0.001,0.001,3.0],[10.1,0.002,3.0],[10.0,10.1,3.0],[0.05,9.999,3.0],[0.001,0.001,3.0]]]}"));
            var geom = serializer3.Deserialize<IGeometry>(reader);
            Assert.That(geom.AsText(), Is.EqualTo("POLYGON ((0 0 3, 10.1 0 3, 10 10.1 3, 0.1 10 3, 0 0 3))"));

            var serializer2 = GeoJsonSerializer.Create(factory, 2);
            reader = new JsonTextReader(new StringReader("{\"type\":\"Polygon\",\"coordinates\":[[[0.001,0.001,3.0],[10.1,0.002,3.0],[10.0,10.1,3.0],[0.05,9.999,3.0],[0.001,0.001,3.0]]]}"));
            geom = serializer2.Deserialize<IGeometry>(reader);
            Assert.That(geom.AsText(), Is.EqualTo("POLYGON ((0 0, 10.1 0, 10 10.1, 0.1 10, 0 0))"));
        }

        [Test, GeoJsonIssueNumber(30)]
        public void TestAddToSerializerSettings()
        {
            var jss = new JsonSerializerSettings();
            var factory = new GeometryFactory(new PrecisionModel(1E6),4326);
            const int dimension = 2;
            Assert.That(() =>
            {
                // see https://github.com/NetTopologySuite/NetTopologySuite.IO.GeoJSON/blob/v1.15.2/NetTopologySuite.IO.GeoJSON/GeoJsonSerializer.cs#L64
                jss.Converters.Add(new ICRSObjectConverter());
                jss.Converters.Add(new FeatureCollectionConverter());
                jss.Converters.Add(new FeatureConverter());
                jss.Converters.Add(new AttributesTableConverter());
                jss.Converters.Add(new GeometryConverter(factory, dimension));
                jss.Converters.Add(new GeometryArrayConverter(factory, dimension));
                jss.Converters.Add(new CoordinateConverter(factory.PrecisionModel, dimension));
                jss.Converters.Add(new EnvelopeConverter());
            }, Throws.Nothing);
        }
    }
}
