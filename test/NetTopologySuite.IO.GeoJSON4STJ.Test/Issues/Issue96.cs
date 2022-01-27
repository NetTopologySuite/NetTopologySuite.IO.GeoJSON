using System.Text.Json;
using NetTopologySuite.Features;
using NetTopologySuite.IO.GeoJSON4STJ.Test.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(96)]
    public sealed class Issue96 : SandDTest<IFeature>
    {
        private void DoTest(string data)
        {
            Assert.Throws<JsonException>(
                () => Deserialize(data, DefaultOptions));
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
