using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Converters;
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
            Assert.That(f.Attributes.TryGetId(out object id));
            Assert.That(id, Is.EqualTo("0a3f507a-b2e6-32b8-e044-0003ba298018"));
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

            var attributesTable = new AttributesTable { {"key", "value"}};
            Geometry geometry = null;
            var nullGeometryFeature = new Feature(geometry, attributesTable);

            var featureCollection_notNullFirst = new FeatureCollection
            {
                notNullGeometryFeature,
                nullGeometryFeature
            };

            var featureCollection_nullFirst = new FeatureCollection
            {
                nullGeometryFeature,
                notNullGeometryFeature
            };

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

            Geometry g = null;
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json0), Throws.Nothing, "null, null");
            Assert.That(g is Point);
            Assert.That(g.IsEmpty);
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json1), Throws.Nothing, "null, 1");
            Assert.That(g is Point);
            Assert.That(!g.IsEmpty);
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json2), Throws.Nothing, "2, null");
            Assert.That(g is Point);
            Assert.That(!g.IsEmpty);
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(json3), Throws.Nothing, "2, 1, null");
            Assert.That(g is Point);
            Assert.That(!g.IsEmpty);
        }

        [GeoJsonIssueNumber(20)]
        [Test]
        public void TestNullValueHandlingEnvelope()
        {
            var factory = new GeometryFactory(new PrecisionModel(100), 4326);

            var value = new Feature(factory.CreatePoint(new Coordinate(23, 56)), null);
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
            var coords = new Coordinate[]
            {
                new CoordinateZ(0.001, 0.001, 3),
                new CoordinateZ(10.1, 0.002, 3),
                new CoordinateZ(10, 10.1, 3),
                new CoordinateZ(0.05, 9.999, 3),
                new CoordinateZ(0.001, 0.001, 3)
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
            var geom = serializer.Deserialize<Geometry>(reader);
            Assert.That(geom.AsText(), Is.EqualTo("POLYGON ((0 0, 10.1 0, 10 10.1, 0.1 10, 0 0))"));
        }

        [GeoJsonIssueNumber(27)]
        [Test]
        public void TestInputDimension()
        {
            // Create a factory with scale = 10
            var factory = new GeometryFactory(new PrecisionModel(10), 4326);
            var wktWriter = new WKTWriter(4);

            var serializerZ = GeoJsonSerializer.Create(factory, 3);
            var reader = new JsonTextReader(new StringReader("{\"type\":\"Polygon\",\"coordinates\":[[[0.001,0.001,3.0],[10.1,0.002,3.0],[10.0,10.1,3.0],[0.05,9.999,3.0],[0.001,0.001,3.0]]]}"));
            var geom = serializerZ.Deserialize<Geometry>(reader);
            Assert.That(wktWriter.Write(geom), Is.EqualTo("POLYGON Z((0 0 3, 10.1 0 3, 10 10.1 3, 0.1 10 3, 0 0 3))"));

            var serializer = GeoJsonSerializer.Create(factory, 2);
            reader = new JsonTextReader(new StringReader("{\"type\":\"Polygon\",\"coordinates\":[[[0.001,0.001,3.0],[10.1,0.002,3.0],[10.0,10.1,3.0],[0.05,9.999,3.0],[0.001,0.001,3.0]]]}"));
            geom = serializer.Deserialize<Geometry>(reader);
            Assert.That(wktWriter.Write(geom), Is.EqualTo("POLYGON ((0 0, 10.1 0, 10 10.1, 0.1 10, 0 0))"));
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
                jss.Converters.Add(new FeatureCollectionConverter());
                jss.Converters.Add(new FeatureConverter());
                jss.Converters.Add(new GeometryConverter(factory, dimension));
                jss.Converters.Add(new GeometryArrayConverter(factory, dimension));
                //jss.Converters.Add(new CoordinateConverter(factory.PrecisionModel, dimension));
                jss.Converters.Add(new EnvelopeConverter());
            }, Throws.Nothing);
        }

        [Test, GeoJsonIssueNumber(31), Explicit, Ignore("coordinates is not a real GeoJSON object, it is some sort of array, so a converter is not correct.")]
        public void TestCoordinateConverter()
        {
            var geoSerializer = GeoJsonSerializer.CreateDefault();

            var c1 = new Coordinate(1, 2);
            var sb = new StringBuilder();
            geoSerializer.Serialize(new JsonTextWriter(new StringWriter(sb)), c1);
            string json = sb.ToString(); // it is equal to "[1.0,2.0]"

            Coordinate c2 = null;
            Assert.DoesNotThrow(() =>
                c2 = geoSerializer.Deserialize<Coordinate>(new JsonTextReader(new StringReader(json))));

            Assert.That(c2, Is.Not.Null);
            Assert.That(c2, Is.EqualTo(c1));

        }

        [Test, GeoJsonIssueNumber(37)]
        public void TestSelfReferenceLoop()
        {
            var asm = Assembly.GetExecutingAssembly();
            var file = asm.GetManifestResourceStream("NetTopologySuite.IO.GeoJSON.Test.Issue37.GeoJson");
            if (file == null)
                throw new IgnoreException("Resource Issue37.json not found");

            var reader = new GeoJsonReader();
            var writer = new GeoJsonWriter();
            AttributesTableConverter.WriteIdToProperties = true;

            Assert.That(() =>
            {
                var data = reader.Read<Feature>(new JsonTextReader(new StreamReader(file)));

                var admin = AdminType.City;
                int id = Convert.ToInt32(data.Attributes["id"]);
                if (id - 100000000 > 256)
                {
                    admin = AdminType.Distric;
                }

                if (!data.Attributes.Exists("wof:lang_x_official"))
                    data.Attributes.Add("wof:lang_x_official", new List<string>() {"vnm", "eng"});


                var result = AliasNames(data.Attributes["wof:name"].ToString(), admin);
                if (result.Count > 0)
                {
                    if (data.Attributes.Exists("name:eng_x_preferred"))
                        data.Attributes.DeleteAttribute("name:eng_x_preferred");
                    if (data.Attributes.Exists("label:eng_x_preferred"))
                        data.Attributes.DeleteAttribute("label:eng_x_preferred");
                    if (data.Attributes.Exists("name:vnm_x_preferred"))
                        data.Attributes.DeleteAttribute("name:vnm_x_preferred");


                    result.Insert(0, data.Attributes["wof:name"].ToString());
                    data.Attributes.Add("name:eng_x_preferred", result);
                    data.Attributes.Add("name:vnm_x_preferred", result);
                }

                string json = writer.Write(data);
                TestContext.WriteLine(json);
            }, Throws.Nothing);
        }

        private IList<string> AliasNames(string key, AdminType adminType)
        {
            return new List<string>(new [] { $"{key}_{adminType}_1", $"{key}_{adminType}_2" });
        }

        private enum AdminType
        {
            City, Distric
        }

        [Test, GeoJsonIssueNumber(41)]
        public void Test3DPointSerialization()
        {
            var factory = GeometryFactory.Default;
            var point1 = factory.CreatePoint(new CoordinateZM(1, 2, 3, 4));
            var feature1 = new Feature(point1, null);

            Feature feature2;
            using (var ms = new MemoryStream())
            {
                var serializer = GeoJsonSerializer.Create(factory, 3);
                using (var writer = new StreamWriter(ms, Encoding.UTF8, 1024, true))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    serializer.Serialize(jsonWriter, feature1);
                }

                ms.Position = 0;
                using (var reader = new StreamReader(ms, Encoding.UTF8, true, 1024, true))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    feature2 = serializer.Deserialize<Feature>(jsonReader);
                }
            }

            Assert.That(feature2.Geometry, Is.InstanceOf<Point>());
            var point2 = (Point)feature2.Geometry;
            Assert.That(point2.CoordinateSequence.HasZ);
            Assert.That(CoordinateSequences.IsEqual(point1.CoordinateSequence, point2.CoordinateSequence));

            // GeoJSON doesn't support M, so there should NOT be an M present in round-trip.
            Assert.That(!point2.CoordinateSequence.HasM);
        }

        [GeoJsonIssueNumber(45)]
        [Test]
        public void TestDeserializeObjectInsideProperties()
        {
            var f = NtsGeometryServices.Instance.CreateGeometryFactory(4326);
            var p = f.CreatePoint(new CoordinateZ(10, 10, 1));
            var featureS = new Feature(p, new AttributesTable(new[] {
                new KeyValuePair<string, object>("data", new MyClass { ValueString = "Hello", ValueInt32 = 17, ValueDouble = Math.PI }),
                }));

            var js = GeoJsonSerializer.CreateDefault();
            js.TypeNameHandling = TypeNameHandling.Objects;
            var ms = GeoJsonSerializerTest.Serialize(js, featureS);
            var featureD = (Feature)GeoJsonSerializerTest.Deserialize(js, ms, typeof(Feature));

            Assert.That(featureD.Attributes["data"], Is.InstanceOf<MyClass>());
            var mc = (MyClass)featureD.Attributes["data"];
            Assert.That(mc.ValueString, Is.EqualTo("Hello"));
            Assert.That(mc.ValueInt32, Is.EqualTo(17));
            Assert.That(mc.ValueDouble, Is.EqualTo(Math.PI));
        }

        [Test, GeoJsonIssueNumber(51)]
        public void TestPrecisedEnvelope()
        {
            var asm = Assembly.GetExecutingAssembly();
            var file = asm.GetManifestResourceStream("NetTopologySuite.IO.GeoJSON.Test.Issue51.GeoJson");
            if (file == null)
                throw new IgnoreException("Resource Issue51.json not found");

            var serializer = GeoJsonSerializer.Create(new JsonSerializerSettings(),
                NtsGeometryServices.Instance.CreateGeometryFactory(new PrecisionModel(1000), 4326));

            FeatureCollection fc = null;
            Assert.That(() => fc = serializer.Deserialize<FeatureCollection>(new JsonTextReader(new StreamReader(file))), Throws.Nothing);
            Assert.That(fc, Is.Not.Null);

            serializer = GeoJsonSerializer.Create(new JsonSerializerSettings(),
                NtsGeometryServices.Instance.CreateGeometryFactory(new PrecisionModel(1000), 4326));
            var sw = new StringWriter(new StringBuilder());

            serializer.Serialize(sw, fc);
            sw.Flush();

            Assert.That(sw.ToString(), Is.EqualTo("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"bbox\":[-72.331,41.296,-72.277,41.611],\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[-72.316,41.296],[-72.316,41.296],[-72.317,41.297],[-72.318,41.298],[-72.318,41.298]]},\"properties\":{\"cumulativeDistance\":42.3458,\"distance\":42.3458,\"cumulativeTime\":42.3458,\"time\":42.3458}}],\"bbox\":[-72.331,41.296,-72.277,41.611]}"));
        }

        [GeoJsonIssueNumber(59)]
        [Test]
        public void TestGeoJsonWithNestedObjectsInProperties()
        {
            const string geojson =
                @"{
    ""type"": ""Feature"",
    ""geometry"": {
      ""type"": ""Point"",
      ""coordinates"": [1, 2]
    },
    ""properties"": {
      ""complex"": {
        ""a"": [""b"", ""c""],
        ""d"": [""e"", ""f""]
      }
    } 
  }
}";

            Feature f = null;
            Assert.That(() => f = new GeoJsonReader().Read<Feature>(geojson), Throws.Nothing);
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Attributes["complex"], Is.InstanceOf<AttributesTable>());
            var innerTable = f.Attributes["complex"] as AttributesTable;
            Assert.That(innerTable["a"], Is.InstanceOf<List<object>>());
            Assert.That(innerTable["d"], Is.InstanceOf<List<object>>());
        }

        [GeoJsonIssueNumber(65)]
        [TestCase("Point")]
        [TestCase("LineString")]
        [TestCase("Polygon")]
        [TestCase("MultiPoint")]
        [TestCase("MultiLineString")]
        [TestCase("MultiPolygon")]
        [TestCase("GeometryCollection")]
        public void TestGeoJsonWithNullCoordinatesOrGeometries(string geometryType)
        {
            string tag = geometryType == "GeometryCollection" ? "geometries" : "coordinates";
            string geojson = $"{{\"type\": \"{geometryType}\", \"{tag}\": null}}";

            Geometry g = null;
            Assert.That(() => g = new GeoJsonReader().Read<Geometry>(geojson), Throws.Nothing);
            Assert.That(g, Is.Not.Null);
            Assert.That(g.IsEmpty);
        }

        [GeoJsonIssueNumber(79)]
        [Test]
        public void TestFeatureIdSerializedToRoot()
        {
            var feature = new Feature
            {
                Geometry = new Point(0, 0),
                Attributes = new AttributesTable(new Dictionary<string, object> {
                    { "name", "Test feature" },
                    { "id", 1 }
                })
            };

            var serializer = GeoJsonSerializer.Create();
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);
            serializer.Serialize(jsonWriter, feature);

            string expected = "{\"type\":\"Feature\",\"id\":1,\"geometry\":{\"type\":\"Point\",\"coordinates\":[0.0,0.0]},\"properties\":{\"name\":\"Test feature\"}}";
            Assert.That(stringWriter.ToString(), Is.EqualTo(expected));
        }

    }

    class MyClass
    {
        public string ValueString { get; set; }
        public int ValueInt32 { get; set; }
        public double ValueDouble { get; set; }
    }
}
