using NetTopologySuite.Features;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    [TestFixture]
    public class Issue46Fixture
    {
        [Test, GeoJsonIssueNumber(46)]
        public void test_deserialize_nested_geojson()
        {
#pragma warning disable CS0219 // Variable is assigned but its value is never used
            const string sample = "{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"id\":\"63a72ea5-45d6-4d4c-a77c-948e5c814317\",\"geometry\":{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[1,0],[1,1],[0,0]]]},\"properties\":{\"a\":[],\"b\":[],\"c\":0.15403640270233154,\"d\":0.15403640270233154,\"e\":null,\"f\":null,\"g\":null,\"h\":null,\"i\":0,\"n\":\"2018-05-08T00:00:00\",\"o\":{\"a1\":\"2018-09-14T00:00:00\",\"b1\":86400,\"c1\":\"R6\",\"d1\":4,\"e1\":12.47,\"f1\":1563.25,\"g1\":129,\"h1\":1,\"i1\":0.23666,\"j1\":0.00056,\"k1\":0.8}}}]}";
#pragma warning restore CS0219 // Variable is assigned but its value is never used
            const string formatted =
            @"
{
	""type"": ""FeatureCollection"",
	""features"": [{
			""type"": ""Feature"",
			""id"": ""63a72ea5-45d6-4d4c-a77c-948e5c814317"",
			""geometry"": {
				""type"": ""Polygon"",
				""coordinates"": [[[0, 0], [1, 0], [1, 1], [0, 0]]]
			},
			""properties"": {
				""a"": 0,
				""b"": {
					""a1"": 1
				},
                ""c"": [""s1"", ""s2""],
			}
		}
	]
}";

            var reader = new GeoJsonReader();
            var coll = reader.Read<FeatureCollection>(formatted);

            Assert.AreEqual(1, coll.Count);
            var feature = coll.Single();
            var attributes = feature.Attributes;
            Assert.NotNull(attributes);
            Assert.AreEqual(4, attributes.Count);
            Assert.IsNotNull(attributes["id"]);
            Assert.IsNotNull(attributes["a"]);
            Assert.AreEqual(0, attributes["a"]);
            Assert.IsNotNull(attributes["b"]);
            Assert.IsInstanceOf(typeof(AttributesTable), attributes["b"]);
            var inner = (AttributesTable)attributes["b"];
            Assert.AreEqual(1, inner.Count);
            Assert.IsNotNull(inner["a1"]);
            Assert.AreEqual(1, inner["a1"]);
            Assert.IsNotNull(attributes["c"]);
            Assert.IsInstanceOf(typeof(List<object>), attributes["c"]);
            var list = (List<object>)attributes["c"];
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.All(i => i is string));
            Assert.IsTrue(list.All(i => !String.IsNullOrEmpty(i as string)));
        }
    }
}
