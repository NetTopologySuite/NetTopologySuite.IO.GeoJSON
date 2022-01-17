using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GeoJSON4STJ.Test;
using NetTopologySuite.IO.GeoJSON4STJ.Test.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(94)]
    public sealed class Issue94 : SandDTest<IFeature>
    {
        private void DoTest(string data)
        {
            var options = DefaultOptions;
            var f = Deserialize(data, options);
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
            ""bbb"": 2
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
            ""bbb"": 2
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
