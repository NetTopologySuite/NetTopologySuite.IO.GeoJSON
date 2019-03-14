using System.Linq;
using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    [GeoJsonIssueNumber(14)]
    [Category("GitHub Issue")]
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
        public void deserialize_geojson_with_crs_property_type_name_should_not_throw_execption()
        {
            string input = "{'type':'Feature', 'properties':{'type': 'string'},'unmanaged':{'type':'unmanaged'}, 'geometry':{'type':'Polygon', 'coordinates':[[[2.329444885254, 48.849334716797], [2.3412895202638, 48.84916305542], [2.340431213379, 48.841953277588], [2.3278999328614, 48.841953277588], [2.329444885254, 48.849334716797]]]}, 'crs':{'type':'name', 'properties':{'name':'urn:ogc:def:crs:OGC:1.3:CRS84'}}}";
            Assert.DoesNotThrow(() => geoJsonReader.Read<Feature>(input), "ArgumentException:Expected value 'Feature' not found.");
        }

        [Test]
        public void deserialize_geojson_with_object_with_unmanaged_attribute_should_should_not_throw_execption()
        {
            string input = "{'type':'Feature', 'properties':{'type': 'string'},'unmanagedValue':'type', 'unmanagedObject':{'type':'unmanaged'}, 'geometry':{'type':'Polygon', 'coordinates':[[[2.329444885254, 48.849334716797], [2.3412895202638, 48.84916305542], [2.340431213379, 48.841953277588], [2.3278999328614, 48.841953277588], [2.329444885254, 48.849334716797]]]}, 'crs':{'type':'name', 'properties':{'name':'urn:ogc:def:crs:OGC:1.3:CRS84'}}}";
            var feature = geoJsonReader.Read<Feature>(input);
            Assert.DoesNotThrow(() => geoJsonReader.Read<Feature>(input), "ArgumentException:Expected value 'Feature' not found.");
        }

        [Test]
        public void deserialize_geojson_with_object_with_unmanaged_attribute_should_be_ignored()
        {
            string input = "{'type':'Feature', 'properties':{'type': 'string'},'unmanagedValue':'type', 'unmanagedObject':{'type':'unmanaged'}, 'geometry':{'type':'Polygon', 'coordinates':[[[2.329444885254, 48.849334716797], [2.3412895202638, 48.84916305542], [2.340431213379, 48.841953277588], [2.3278999328614, 48.841953277588], [2.329444885254, 48.849334716797]]]}, 'crs':{'type':'name', 'properties':{'name':'urn:ogc:def:crs:OGC:1.3:CRS84'}}}";
            var feature = geoJsonReader.Read<Feature>(input);
            Assert.That(feature.Attributes.Count, Is.EqualTo(1));
            Assert.True(feature.Attributes.Exists("type"));
            Assert.That(feature.Attributes["type"], Is.EqualTo("string"));
        }

        [Test]
        public void deserialize_geojson_collectionfeature_with_feature_custom_type_should_have_feature_with_type_atrribute()
        {
            string input = "{'type':'FeatureCollection','crs':{'type':'name','properties':{'name':'urn:ogc:def:crs:EPSG::4324'}},'features':[{'type':'Feature', 'geometry':{'type': 'MultiPolygon', 'coordinates': [[[[-5e6, 6e6], [-5e6, 8e6], [-3e6, 8e6], [-3e6, 6e6], [-5e6, 6e6]]], [[[-2e6, 6e6], [-2e6, 8e6], [0, 8e6], [0, 6e6], [-2e6, 6e6]]], [[[1e6, 6e6], [1e6, 8e6], [3e6, 8e6], [3e6, 6e6], [1e6, 6e6]]]]},'properties':{'time':1800.0,'area':66575357.832716957,'type':'typeTest'},'id':'fid-536a694f_16088ddd78e_-1eba'}]}";
            var featureCollection = geoJsonReader.Read<FeatureCollection>(input);
            Assert.That(featureCollection.Features.Count, Is.EqualTo(1));
            var feature = featureCollection.Features.First();
            Assert.True(feature.Attributes.Exists("type"));
            Assert.That(feature.Attributes["type"], Is.EqualTo("typeTest"));
        }
    }
}
