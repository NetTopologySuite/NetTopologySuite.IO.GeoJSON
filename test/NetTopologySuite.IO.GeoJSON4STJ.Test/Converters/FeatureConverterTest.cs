using System.Buffers.Text;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    ///<summary>
    ///    This is a test class for FeatureConverterTest and is intended
    ///    to contain all FeatureConverterTest Unit Tests
    ///</summary>
    [TestFixture(true)]
    [TestFixture(false)]
    public class FeatureConverterTest : SandDTest<IFeature>
    {
        public FeatureConverterTest(bool nestedObjectsAsJsonElement)
        {
            NestedObjectsAsJsonElement = nestedObjectsAsJsonElement;
        }

        ///<summary>
        ///A test for CanConvert
        ///</summary>
        [Test]
        public void CanConvertTest()
        {
            var options = DefaultOptions;
            var target = (JsonConverter<IFeature>)GeoJsonConverterFactory.CreateConverter(typeof(IFeature), options);
            var objectType = typeof(IFeature);
            const bool expected = true;
            bool actual = target.CanConvert(objectType);
            Assert.AreEqual(expected, actual);
        }

        ///<summary>
        ///    A test for WriteJson
        ///</summary>
        [Test]
        public void WriteJsonTest()
        {
            var attributes = new AttributesTable();
            attributes.Add("test1", "value1");
            IFeature value = new Feature(new Point(23.1, 56.2), attributes);
            var options = DefaultOptions;
            options.WriteIndented = false;
            options.IgnoreNullValues = true;
            GeoJsonConverterFactory.WriteGeometryBBox = false;
            //GeoJsonConverterFactory.OrdinateFormatString = "0.{}";

            string json = ToJsonString(value, options);
            var deserialized = Deserialize(json, options);
            CheckEquality(value, deserialized, true);
            //Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.1,56.2]},\"properties\":{\"test1\":\"value1\"}}", ToJson(value));
        }


        ///<summary>
        ///    A test for WriteJson
        ///</summary>
        [Test]
        public void WriteJsonWithArrayTest()
        {
            var attributes = new AttributesTable();
            attributes.Add("test1", new[] { "value1", "value2" });
            IFeature value = new Feature(new Point(23.1, 56.2), attributes);
            var options = DefaultOptions;
            options.WriteIndented = false;
            options.IgnoreNullValues = true;

            string json = ToJsonString(value, options);
            var deserialized = Deserialize(json, options);
            CheckEquality(value, deserialized, true);
            //Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.1,56.2]},\"properties\":{\"test1\":[\"value1\",\"value2\"]}}", ToJson(value));
        }

        private void CheckEquality(IFeature s, IFeature d, bool checkType = false)
        {
            CheckEquality(s, d, NestedObjectsAsJsonElement, checkType);
        }

        public static void CheckEquality(IFeature s, IFeature d, bool nestedObjectsAsJsonElement, bool checkType = false, string idPropertyName = "id")
        {
            Assert.That(d, Is.Not.Null);

            if (checkType)
                Assert.That(d.GetType(), Is.EqualTo(s.GetType()));

            Assert.That(s.Geometry.EqualsExact(d.Geometry));

            AttributesTableConverterTest.TestEquality(s.Attributes, d.Attributes, nestedObjectsAsJsonElement, idPropertyName);

            if (s.BoundingBox != null)
                Assert.That(d.BoundingBox, Is.EqualTo(s.BoundingBox));

        }

    }
}
