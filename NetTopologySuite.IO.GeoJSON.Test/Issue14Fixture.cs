using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    [TestFixture]
    public class Issue14Fixture
    {
        private GeoJsonReader geoJsonReader;

        [SetUp]
        public void SetUp()
        {
            geoJsonReader = new GeoJsonReader();
        }


        [Test]
        public void deserialize_geojson_with_crs_propert_type_name_should_not_throw_execption()
        {
            string input = "{\"type\":\"Feature\", \"properties\":{}, \"geometry\":{\"type\":\"Polygon\", \"coordinates\":[[[2.329444885254, 48.849334716797], [2.3412895202638, 48.84916305542], [2.340431213379, 48.841953277588], [2.3278999328614, 48.841953277588], [2.329444885254, 48.849334716797]]]}, \"crs\":{\"type\":\"name\", \"properties\":{\"name\":\"urn:ogc:def:crs:OGC:1.3:CRS84\"}}}";
            Assert.DoesNotThrow(() => geoJsonReader.Read<Feature>(input), "ArgumentExcetpion:Expected value 'Feature' not found.");
        }

        [Test]
        public void deserialize_geojson_with_crs_property_type_link_should_have_property_name()
        {
            string input = "{\"type\":\"Feature\", \"properties\":{}, \"geometry\":{\"type\":\"Polygon\", \"coordinates\":[[[2.329444885254, 48.849334716797], [2.3412895202638, 48.84916305542], [2.340431213379, 48.841953277588], [2.3278999328614, 48.841953277588], [2.329444885254, 48.849334716797]]]}, \"crs\":{\"type\":\"name\", \"properties\":{\"name\":\"urn:ogc:def:crs:OGC:1.3:CRS84\"}}}";
            var feature = geoJsonReader.Read<Feature>(input);
            Assert.True(feature.Attributes.Count == 1);
            Assert.True(feature.Attributes.Exists("name"));
            Assert.True(feature.Attributes["name"].Equals("urn:ogc:def:crs:OGC:1.3:CRS84"));
        }

        [Test]
        public void deserialize_geojson_with_crs_property_type_name_should_have_properties_link()
        {
            string input = "{\"type\":\"Feature\", \"properties\":{}, \"geometry\":{\"type\":\"Polygon\", \"coordinates\":[[[2.329444885254, 48.849334716797], [2.3412895202638, 48.84916305542], [2.340431213379, 48.841953277588], [2.3278999328614, 48.841953277588], [2.329444885254, 48.849334716797]]]}, \"crs\":{\"type\":\"link\", \"properties\":{\"href\":\"http://nettopologiesuite.io/crs/42 \", \"type\":\"proj4\"}}}";
            var feature = geoJsonReader.Read<Feature>(input);

            Assert.True(feature.Attributes.Count == 2);
            Assert.True(feature.Attributes.Exists("href"));
            Assert.True(feature.Attributes.Exists("type"));
            Assert.True(feature.Attributes["href"].Equals("http://nettopologiesuite.io/crs/42"));
            Assert.True(feature.Attributes["type"].Equals("proj4"));
        }

        [Test]
        public void deserialize_geojson_with_crs_should_have_properties()
        {
            string input = "{\"type\":\"Feature\", \"properties\":{\"prop1\": 1}, \"geometry\":{\"type\":\"Polygon\", \"coordinates\":[[[2.329444885254, 48.849334716797], [2.3412895202638, 48.84916305542], [2.340431213379, 48.841953277588], [2.3278999328614, 48.841953277588], [2.329444885254, 48.849334716797]]]}, \"crs\":{\"type\":\"name\", \"properties\":{\"name\":\"urn:ogc:def:crs:OGC:1.3:CRS84\"}}}";
            var feature = geoJsonReader.Read<Feature>(input);
            Assert.True(feature.Attributes.Count == 2);
            Assert.True(feature.Attributes.Exists("name"));
            Assert.True(feature.Attributes.Exists("prop1"));
            Assert.True(feature.Attributes["name"].Equals("urn:ogc:def:crs:OGC:1.3:CRS84"));
            Assert.True(feature.Attributes["prop1"].Equals((long)1));
        }
    }
}
