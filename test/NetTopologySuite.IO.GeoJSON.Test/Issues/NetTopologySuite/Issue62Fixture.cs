using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    [NtsIssueNumber(62)]
    [Category("GitHub Issue")]
    [TestFixture]
    public class Issue62Fixture
    {
        [Test]
        public void geojson_should_deserialize_a_geometry_with_geometrycollection()
        {
            const string json = @"
{
    ""type"": ""GeometryCollection"",
    ""geometries"": [ 
        {
            ""type"":""Polygon"",
            ""coordinates"":[[[1.0,1.0],[1.0,2.0],[2.0,2.0],[1.0,1.0]]]
        },
        {
            ""type"":""Point"",
            ""coordinates"":[100.0,100.0]
        },
        {
            ""type"":""Polygon"",
            ""coordinates"":[[[201.0,201.0],[201.0,202.0],[202.0,202.0],[201.0,201.0]]]
        }
    ]
}
";
            var reader = new GeoJsonReader();
            var geometry = reader.Read<Geometry>(json);
            Assert.IsNotNull(geometry);
        }

        [Test]
        public void geojson_should_deserialize_a_feature_with_geometrycollection()
        {
            const string json = @"
{
    ""type"": ""Feature"",
    ""geometry"": {
        ""type"": ""GeometryCollection"",
        ""geometries"": [ 
            {
                ""type"":""Polygon"",
                ""coordinates"":[[[1.0,1.0],[1.0,2.0],[2.0,2.0],[1.0,1.0]]]
            },
            {
                ""type"":""Point"",
                ""coordinates"":[100.0,100.0]
            },
            {
                ""type"":""Polygon"",
                ""coordinates"":[[[201.0,201.0],[201.0,202.0],[202.0,202.0],[201.0,201.0]]]
            }
        ]
    },
    ""properties"": {
    }
}
";
            var reader = new GeoJsonReader();
            var geometry = reader.Read<Feature>(json);
            Assert.IsNotNull(geometry);
        }
    }
}
