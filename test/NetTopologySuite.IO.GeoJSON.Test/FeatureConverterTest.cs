﻿using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    ///<summary>
    ///    This is a test class for FeatureConverterTest and is intended
    ///    to contain all FeatureConverterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class FeatureConverterTest
    {
        ///<summary>
        ///    A test for CanConvert
        ///</summary>
        [Test]
        public void CanConvertTest()
        {
            var target = new FeatureConverter();
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
            var target = new FeatureConverter();
            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));

            var attributes = new AttributesTable();
            attributes.Add("test1", "value1");
            IFeature value = new Feature(new Point(23, 56), attributes);
            var serializer = GeoJsonSerializer.Create(
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore },
                GeometryFactory.Default);
            target.WriteJson(writer, value, serializer);
            writer.Flush();

            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}", sb.ToString());
        }

        /// <summary>
        /// Tests whether required feature members are written, even if they are null.
        /// </summary>
        [Test]
        public void WriteJsonShouldIgnoreCustomNullWritingOptionsTest()
        {
            var target = new FeatureConverter();
            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));

            IFeature value = new Feature(null, null);
            var serializer = GeoJsonSerializer.Create(
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore },
                GeometryFactory.Default);
            target.WriteJson(writer, value, serializer);
            writer.Flush();

            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":null,\"properties\":null}", sb.ToString());
        }

        ///<summary>
        ///    A test for WriteJson
        ///</summary>
        [Test]
        public void WriteJsonWithArrayTest()
        {
            var target = new FeatureConverter();
            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            var attributes = new AttributesTable();
            attributes.Add("test1", new[] { "value1", "value2" });
            IFeature value = new Feature(new Point(23, 56), attributes);
            var serializer = GeoJsonSerializer.Create(new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }, GeometryFactory.Default);
            target.WriteJson(writer, value, serializer);
            writer.Flush();
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":[\"value1\",\"value2\"]}}", sb.ToString());
        }
    }
}
