using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(94)]
    public sealed class GitHubIssue94
    {
        private static void DoTest(string data)
        {
            var serializer = GeoJsonSerializer.CreateDefault();
            IFeature f;
            using (var sr = new StringReader(data))
            using (var jtr = new JsonTextReader(sr))
            {
                f = serializer.Deserialize<IFeature>(jtr);
            }
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Geometry, Is.Not.Null);
            Assert.That(f.Geometry, Is.InstanceOf<Point>());
        }

        [Test]
        public void TestSimplePropertyDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
		""prop1"": true
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestNullPropertyDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
		""prop1"": null
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestSimplePropertiesDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
		""prop1"": true,
        ""prop2"": false
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestComplexPropertyDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
		""complex1"": {
            ""aaa"": ""1"",
            ""bbb"": 2
        }
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestComplexPropertiesDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
		""complex1"": {
            ""aaa"": ""1"",
            ""bbb"": 2
        },
        ""complex2"": {
            ""aaa"": ""3"",
            ""bbb"": 4
        }
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestArrayPropertyDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
		""array1"": [""aaa"", ""bbb""]
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestArrayPropertiesDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
		""array1"": [""aaa"", ""bbb""],
        ""array2"": [""ccc"", ""ddd""]
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestMixedPropertiesDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
        ""array1"": [""aaa"", ""bbb""],
		""complex1"": {
            ""aaa"": ""1"",
            ""bbb"": {
                ""zzzz"": 2
            }
        },
        ""prop1"": true,
        ""prop2"": null

	}
}";
            DoTest(data);
        }

        [Test]
        public void TestMixedPropertiesDeserializationInGeometryChangingPropertiesOrder()
        {
            const string data = @"{
	""geometry"": {
        ""array1"": [""aaa"", ""bbb""],
		""complex1"": {
            ""aaa"": ""1"",
            ""bbb"": {
                ""zzzz"": 2
            }
        },
        ""prop1"": true,
        ""prop2"": null,
        ""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175]
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestMixedPropertiesWithNullsAndKeywordsDeserializationInGeometry()
        {
            const string data = @"{
	""geometry"": {
		""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175],
        ""array1"": [""aaa"", ""bbb""],
		""complex1"": {
            ""aaa"": ""1"",
            ""bbb"": {
                ""zzzz"": 2,
                ""irrelevant"": null
            },
            ""ccc"": 3,
            ""ddd"": null,
            ""type"": ""MultiPoint""
        },
        ""prop1"": true,
        ""prop2"": null
	}
}";
            DoTest(data);
        }

        [Test]
        public void TestMixedPropertiesWithNullsAndKeywordsDeserializationInGeometryChangingPropertiesOrder()
        {
            const string data = @"{
	""geometry"": {
        ""array1"": [""aaa"", ""bbb""],
		""complex1"": {
            ""aaa"": ""1"",
            ""bbb"": {
                ""zzzz"": 2,
                ""irrelevant"": null
            },
            ""ccc"": 3,
            ""ddd"": null,
            ""type"": ""MultiPoint""
        },
        ""prop1"": true,
        ""prop2"": null,
        ""type"": ""Point"",
		""coordinates"": [-117.267131, 32.959175]
	}
}";
            DoTest(data);
        }
    }
}
