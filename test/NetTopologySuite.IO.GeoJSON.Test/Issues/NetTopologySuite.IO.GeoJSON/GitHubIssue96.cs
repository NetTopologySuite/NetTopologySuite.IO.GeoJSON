using System;
using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(96)]
    public sealed class GitHubIssue96
    {
        private static T Deserialize<T>(string data)
        {
            var serializer = GeoJsonSerializer.CreateDefault();
            using var sr = new StringReader(data);
            using var jtr = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jtr);
        }

        [Test]
        public void TestInvalidFeatureWithNullCoordinatesDeserialization()
        {
            const string data = @"{
	""type"": ""Feature"",
	""id"": ""955r48cb-129f-44ce-a229-cbd065a67bcf"",
	""properties"": {
		""name"": ""test"",
		""updated"": """",
		""altitude"": 2657,
		""longitude"": -116.425598,
		""latitude"": 33.523399
	},
	""geometry"": {
		""type"": ""Polygon"",
		""coordinates"": [
			[
				[
					null
				]
			]
		]
	}
}";
            Assert.Throws<ArgumentOutOfRangeException>(() => Deserialize<IFeature>(data));
        }

        [Test]
        public void TestInvalidFeatureWithEmptyCoordinatesDeserialization()
        {
            const string data = @"{
	""type"": ""Feature"",
	""id"": ""955r48cb-129f-44ce-a229-cbd065a67bcf"",
	""properties"": {
		""name"": ""test"",
		""updated"": """",
		""altitude"": 2657,
		""longitude"": -116.425598,
		""latitude"": 33.523399
	},
	""geometry"": {
		""type"": ""Polygon"",
		""coordinates"": [
			[
				[
				]
			]
		]
	}
}";
            Assert.Throws<ArgumentOutOfRangeException>(() => Deserialize<IFeature>(data));
        }

        [Test]
        public void TestValidFeatureWithEmptyCoordinatesDeserialization()
        {
            const string data = @"{
	""type"": ""Feature"",
	""id"": ""955r48cb-129f-44ce-a229-cbd065a67bcf"",
	""properties"": {
		""name"": ""test"",
		""updated"": """",
		""altitude"": 2657,
		""longitude"": -116.425598,
		""latitude"": 33.523399
	},
	""geometry"": {
		""type"": ""Polygon"",
		""coordinates"": []
	}
}";
            var f = Deserialize<IFeature>(data);
            Assert.That(f, Is.Not.Null);
            Assert.That(f.Geometry, Is.Not.Null);
            Assert.That(f.Geometry, Is.InstanceOf<Polygon>());
            Assert.That(f.Geometry.IsEmpty, Is.True);
        }

        [Test]
        public void TestValidPointEmptyDeserialization()
        {
            const string data = @"{
	""type"": ""Point"",
	""coordinates"": []
}";
            var g = Deserialize<Geometry>(data);
            Assert.That(g, Is.Not.Null);
            Assert.That(g, Is.InstanceOf<Point>());
            Assert.That(g.IsEmpty, Is.True);
        }

        [Test]
        public void TestValidLineStringEmptyDeserialization()
        {
            const string data = @"{
	""type"": ""LineString"",
	""coordinates"": []
}";
            var g = Deserialize<Geometry>(data);
            Assert.That(g, Is.Not.Null);
            Assert.That(g, Is.InstanceOf<LineString>());
            Assert.That(g.IsEmpty, Is.True);
        }

        [Test]
        public void TestValidPolygonEmptyDeserialization()
        {
            const string data = @"{
	""type"": ""Polygon"",
	""coordinates"": []
}";
            var g = Deserialize<Geometry>(data);
            Assert.That(g, Is.Not.Null);
            Assert.That(g, Is.InstanceOf<Polygon>());
            Assert.That(g.IsEmpty, Is.True);
        }

        [Test]
        public void TestValidMultiPointEmptyDeserialization()
        {
            const string data = @"{
	""type"": ""MultiPoint"",
	""coordinates"": []
}";
            var g = Deserialize<Geometry>(data);
            Assert.That(g, Is.Not.Null);
            Assert.That(g, Is.InstanceOf<MultiPoint>());
            Assert.That(g.IsEmpty, Is.True);
        }

        [Test]
        public void TestValidMultiLineStringEmptyDeserialization()
        {
            const string data = @"{
	""type"": ""MultiLineString"",
	""coordinates"": []
}";
            var g = Deserialize<Geometry>(data);
            Assert.That(g, Is.Not.Null);
            Assert.That(g, Is.InstanceOf<MultiLineString>());
            Assert.That(g.IsEmpty, Is.True);
        }

        [Test]
        public void TestValidMultiPolygonEmptyDeserialization()
        {
            const string data = @"{
	""type"": ""MultiPolygon"",
	""coordinates"": []
}";
            var g = Deserialize<Geometry>(data);
            Assert.That(g, Is.Not.Null);
            Assert.That(g, Is.InstanceOf<MultiPolygon>());
            Assert.That(g.IsEmpty, Is.True);
        }

        [Test]
        public void TestValidGeometryCollectionEmptyDeserialization()
        {
            const string data = @"{
	""type"": ""GeometryCollection"",
	""geometries"": []
}";
            var g = Deserialize<Geometry>(data);
            Assert.That(g, Is.Not.Null);
            Assert.That(g, Is.InstanceOf<GeometryCollection>());
            Assert.That(g.IsEmpty, Is.True);
        }
    }
}
