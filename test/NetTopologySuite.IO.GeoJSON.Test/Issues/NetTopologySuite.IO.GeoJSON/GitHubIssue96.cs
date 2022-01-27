using System;
using System.IO;
using NetTopologySuite.Features;
using Newtonsoft.Json;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [GeoJsonIssueNumber(96)]
    public sealed class GitHubIssue96
    {
        private static void DoTest(string data)
        {
            var serializer = GeoJsonSerializer.CreateDefault();
            using var sr = new StringReader(data);
            using var jtr = new JsonTextReader(sr);
            Assert.Throws<ArgumentOutOfRangeException>(
                () => serializer.Deserialize<IFeature>(jtr));
        }

        [Test]
        public void TestInvalidPolygonWithNullCoordinatesDeserialization()
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
            DoTest(data);

        }

        [Test]
        public void TestInvalidPolygonWithEmptyCoordinatesDeserialization()
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
            DoTest(data);
        }
    }
}
