using System;
using System.Globalization;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    [Category("GitHub Issue")]
    [TestFixture]
    public class GitHubIssues
    {
        [NtsIssueNumber(83)]
        [Test(Description = "Testcase for Issue 83")]
        public void TestIssue83()
        {
            const string geoJson = @"{ ""type"": ""Feature"", 
                              ""geometry"": { ""type"": ""Point"", ""coordinates"": [10.0, 60.0] }, 
                              ""id"": 1, 
                             ""properties"": { ""Name"": ""test"" } }";

            var s = GeoJsonSerializer.Create(GeometryFactory.Default);
            Feature f = null;
            Assert.That(() => f = s.Deserialize<Feature>(new JsonTextReader(new StringReader(geoJson))), Throws.Nothing);

            Assert.That(f, Is.Not.Null);
            Assert.That(f.Attributes.TryGetId(out object id));
            Assert.That(id, Is.EqualTo(1));

            var sb = new StringBuilder();
            var tw = new JsonTextWriter(new StringWriter(sb));
            s.Serialize(tw, f);
            string geoJsonRes = sb.ToString();

            CompareJson(geoJson, geoJsonRes);
        }

        public void CompareJson(string expectedJson, string json)
        {
            var e = JToken.Parse(expectedJson);
            var j = JToken.Parse(json);
            Assert.That(JToken.DeepEquals(e, j), "The json's do not match:\n1: {0}\n2:{1}", expectedJson, json);
        }

        [NtsIssueNumber(88)]
        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88WithoutAdditionalProperties()
        {
            const string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	}
}
";

            var serializer = GeoJsonSerializer.CreateDefault();
            Feature feat = null;
            Assert.That(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                {
                    feat = serializer.Deserialize<Feature>(reader);
                }
            }, Throws.Nothing);

            Assert.That(feat, Is.Not.Null);
            Assert.That(feat.Attributes.TryGetId(out object id));
            Assert.That(id, Is.EqualTo("00000000-0000-0000-0000-000000000000"));
            Assert.That(feat.Geometry, Is.Null);

            var attributes = feat.Attributes;
            Assert.That(attributes, Is.Not.Null);
            Assert.That(attributes.Count, Is.EqualTo(3));
            Assert.That(attributes.Exists("yesNo 1"));
            Assert.That(attributes["yesNo 1"], Is.False);
            Assert.That(attributes.Exists("date 1"));
            Assert.That(attributes["date 1"], Is.EqualTo(DateTime.Parse("2016-02-16T00:00:00", CultureInfo.InvariantCulture)));
        }

        [NtsIssueNumber(88)]
        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88WithFlatProperties()
        {
            const string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	},	
	""collection_userinfo"" : ""()"",
	""collection_timestamp"" : ""2016-02-25T14:38:01.9087672"",
	""collection_todoid"" : """",
	""collection_templateid"" : ""nj7Glv-AqV0"",
	""collection_layerid"" : ""nj7Glv-AqV0""	
}
";

            var serializer = GeoJsonSerializer.CreateDefault(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Feature feat = null;
            Assert.That(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                {
                    feat = serializer.Deserialize<Feature>(reader);
                }
            }, Throws.Nothing);

            Assert.That(feat, Is.Not.Null);
            Assert.That(feat.Geometry, Is.Null);
        }

        [NtsIssueNumber(88)]
        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88()
        {
            const string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	},
	""metadata"" : {
		""collection_userinfo"" : ""()"",
		""collection_timestamp"" : ""2016-02-25T14:38:01.9087672"",
		""collection_todoid"" : """",
		""collection_templateid"" : ""nj7Glv-AqV0"",
		""collection_layerid"" : ""nj7Glv-AqV0""
	}
}
";

            var serializer = GeoJsonSerializer.CreateDefault(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            Feature feat = null;
            Assert.That(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                {
                    feat = serializer.Deserialize<Feature>(reader);
                }
            }, Throws.Nothing);

            Assert.That(feat, Is.Not.Null);
            Assert.That(feat.Geometry, Is.Null);
        }

        private Feature _feature;
        private GeoJsonReader _reader;
        private GeoJsonWriter _writer;

        [SetUp]
        public void GivenAGeoJsonReaderAndWriter()
        {
            _reader = new GeoJsonReader();
            _writer = new GeoJsonWriter();
            var geometry = new Point(1, 2);
            _feature = new Feature(geometry, new AttributesTable());
        }

        [NtsIssueNumber(92)]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfJsonObjectArraysPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[
          {
            ""xyz"":[
                {""zee"":""xyz""},
                {""hay"":""zus""}
            ]
          }
        ]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            Assert.That(featureCollection, Has.Count.EqualTo(1));
        }

        [NtsIssueNumber(92)]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfJsonObjectArraysPropertyInGeoJsonThenWriteable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[
          {
            ""xyz"":[
                {""zee"":""xyz""},
                {""hay"":""zus""}
            ]
          }
        ]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            string written = _writer.Write(featureCollection);
            Assert.That(written, Contains.Substring("FeatureCollection"));
        }

        [NtsIssueNumber(92)]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfIntArraysPropertyInFeatureThenWritable()
        {
            int[] arrayOfInts = { 1, 2, 3 };
            int[] anotherArrayOfInts = { 4, 5, 6 };
            int[][] arrayOfArrays = { arrayOfInts, anotherArrayOfInts };
            _feature.Attributes.Add("foo", arrayOfArrays);
            string written = _writer.Write(_feature);
            Assert.That(written, Contains.Substring("Feature"));
        }

        [NtsIssueNumber(92)]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfIntArraysPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
    ""type"":""Feature"",
    ""geometry"":{
        ""type"":""Point"",""coordinates"":[1.0,2.0]},
        ""properties"":{
            ""foo"":[[1,2,3],[4,5,6]]
        }
}
";
            var featureCollection = _reader.Read<Feature>(geojsonString);
            CompareJson(geojsonString, _writer.Write(featureCollection));
        }

        [NtsIssueNumber(92)]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenPopulatedIntArrayPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [1.0,2.0]
      },
      ""properties"": {
        ""foo"": [1, 2]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            Assert.That(featureCollection, Has.Count.EqualTo(1));
        }

        [NtsIssueNumber(93)]
        [Test(Description = "Testcase for GitHub Issue 93")]
        public void WhenEmptyArrayPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[]
      }
    }
  ]
}
";
            var featureCollection = new GeoJsonReader().Read<FeatureCollection>(geojsonString);
            Assert.That(featureCollection, Has.Count.EqualTo(1));
        }

        [NtsIssueNumber(95)]
        [Test(Description = "Testcase for GitHub Issue 95, FeatureCollection having \"bbox\" property")]
        public void TestWhenFeatureCollectionHasBBox()
        {
            const string geoJsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[ 
  { 
      ""geometry"":{ 
          ""type"":""Point"", 
          ""coordinates"":[ -6.09, 4.99 ]
      }, 
      ""type"":""Feature"", 
      ""properties"":{
          ""prop1"":[ ""a"", ""b"" ] 
      }, 
      ""id"":1 
  } ], 
  ""bbox"":[ -8.59, 4.35, -2.49, 10.73 ] 
}";
            var featureCollection = new GeoJsonReader().Read<FeatureCollection>(geoJsonString);
            Assert.That(featureCollection, Has.Count.EqualTo(1));
            string res = new GeoJsonWriter().Write(featureCollection);
            CompareJson(geoJsonString, res);
        }

        [NtsIssueNumber(120)]
        [Test(Description = "Testcase for GitHub Issue 120, Feature having null properties")]
        public void TestRoundtripSerializingDeserializingFeature()
        {
            var gf = new GeometryFactory();
            var f1 = new Feature(gf.CreatePoint(new Coordinate(-104.50348159865847, 40.891762392617345)), null);
            var f2 = SandD(f1);
            DoCheck(f1, f2);

            var t1 = new Feature { Geometry = f1.Geometry };
            var t2 = SandD(t1);
            DoCheck(t1, t2);

            string jsonWithoutProps = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[-104.50348159865847,40.891762392617345]}}";
            DoCheck(jsonWithoutProps);
            string jsonWithValidProps = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[-104.50348159865847,40.891762392617345]},\"properties\":{\"aaa\":1,\"bbb\":2}}";
            DoCheck(jsonWithValidProps);
            string jsonWithNullProps = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[-104.50348159865847,40.891762392617345]},\"properties\":null}";
            DoCheck(jsonWithNullProps);
        }

        private static void DoCheck(string json)
        {
            var reader = new GeoJsonReader();
            var feature = reader.Read<Feature>(json);
            Assert.That(feature, Is.Not.Null);
            Assert.That(feature.Geometry, Is.InstanceOf<Point>());
            Assert.That(feature.BoundingBox, Is.Null);
        }

        private static Feature SandD(Feature input)
        {
            var s = GeoJsonSerializer.Create(
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatFormatHandling = FloatFormatHandling.DefaultValue,
                    FloatParseHandling = FloatParseHandling.Double
                }, input.Geometry.Factory ?? GeometryFactory.Default);
            var sb = new StringBuilder();
            s.Serialize(new JsonTextWriter(new StringWriter(sb)), input, typeof(Feature));
            return s.Deserialize<Feature>(new JsonTextReader(new StringReader(sb.ToString())));

        }

        private static void DoCheck(Feature f1, Feature f2)
        {
            if (f1 is null)
            {
                Assert.That(f2, Is.Null);
                return;
            }

            if (!(f1.Geometry is null))
            {
                Assert.That(f2.Geometry, Is.Not.Null, "f2.Geometry is not null");
                Assert.That(f1.Geometry.EqualsExact(f2.Geometry), "f1.Geometry.EqualsExact(f2.Geometry)");
            }

            if (!(f1.BoundingBox is null))
            {
                if (!Equals(f1.BoundingBox, f2.BoundingBox))
                {
                    Assert.Multiple(() =>
                    {
                        Assert.That(f1.BoundingBox.MinX, Is.EqualTo(f2.BoundingBox.MinX).Within(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                        Assert.That(f1.BoundingBox.MaxX, Is.EqualTo(f2.BoundingBox.MaxX).Within(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                        Assert.That(f1.BoundingBox.MinY, Is.EqualTo(f2.BoundingBox.MinY).Within(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                        Assert.That(f1.BoundingBox.MaxY, Is.EqualTo(f2.BoundingBox.MaxY).Within(1e-10), "f1.BoundingBox.Equals(f2.BoundingBox)");
                    });
                }
            }

            if (f1.Attributes is null)
            {
                Assert.That(f2.Attributes, Is.Null);
            }
            else
            {
                Assert.That(f2.Attributes, Is.Not.Null);
                string[] names1 = f1.Attributes.GetNames();
                string[] names2 = f2.Attributes.GetNames();
                Assert.That(names2, Is.EqualTo(names1));
                foreach (string name in names1)
                {
                    Assert.That(() => f2.Attributes[name], Is.EqualTo(f1.Attributes[name]));
                }
            }
        }

        [NtsIssueNumber(178)]
        [Test(Description = "Parsing GeoJSON issue related to Bounding Box #178")]
        public void ParsingCollectionWithBoundingBox()
        {
            /*
             *Parsing GeoJSON issue related to Bounding Box #178
             */

            string json = "{ \"type\": \"FeatureCollection\", " +
                       "\"features\": [{\"type\": \"Feature\",\"properties\": {},\"geometry\": {\"type\": \"Polygon\"," +
                       "\"bbox\": [-105.46875,38.788345355085625,-102.98583984374999,40.27952566881291]," +
                       "\"coordinates\": [[[-105.46875,38.788345355085625],[-102.98583984374999,38.788345355085625]," +
                       "[-102.98583984374999,40.27952566881291],[-105.46875,40.27952566881291]," +
                       "[-105.46875,38.788345355085625]]] }} ]}";

            var rdr = new GeoJsonReader();
            FeatureCollection fc = null;
            bool cbb = Feature.ComputeBoundingBoxWhenItIsMissing;
            Feature.ComputeBoundingBoxWhenItIsMissing = true;
            Assert.DoesNotThrow(() => fc = rdr.Read<FeatureCollection>(json));
            Assert.That(fc != null);
            Assert.That(fc.Count, Is.EqualTo(1));
            Assert.That(fc.BoundingBox, Is.EqualTo(new Envelope(new Coordinate(-105.46875, 38.788345355085625), new Coordinate(-102.98583984374999, 40.27952566881291))));
            Feature.ComputeBoundingBoxWhenItIsMissing = cbb;


        }
    }


}
