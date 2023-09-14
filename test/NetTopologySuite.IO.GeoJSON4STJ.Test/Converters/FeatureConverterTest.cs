﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    ///<summary>
    ///    This is a test class for FeatureConverterTest and is intended
    ///    to contain all FeatureConverterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class FeatureConverterTest : SandDTest<IFeature>
    {
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

        [GeoJsonIssueNumber(57)]
        [TestCase("{\"type\": \"Feature\", \"bbox\": null}")]
        [TestCase("{\"type\": \"Feature\", \"geometry\": null}")]
        [TestCase("{\"type\": \"Feature\", \"properties\": null}")]
        public void DeserializationShouldAllowNullInputValues(string serializedFeature)
        {
            Assert.That(() => JsonSerializer.Deserialize<IFeature>(serializedFeature, DefaultOptions), Throws.Nothing);
        }

        [TestCase("{\"type\": \"Feature\", \"id\": 1, \"extra\": {\"example\": \"value\"}}")]
        [TestCase("{\"type\": \"Feature\", \"id\": 1, \"extra\": {\"type\": \"Line\", \"id\": 2}}")]
        public void DeserializationShouldAllowForeignMembers(string serializedFeature)
        {
            Assert.That(() => JsonSerializer.Deserialize<IFeature>(serializedFeature, DefaultOptions), Throws.Nothing);
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
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            //GeoJsonConverterFactory.OrdinateFormatString = "0.{}";

            string json = ToJsonString(value, options);
            var deserialized = Deserialize(json, options);
            CheckEquality(value, deserialized);
            //Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.1,56.2]},\"properties\":{\"test1\":\"value1\"}}", ToJson(value));
        }

        /// <summary>
        /// Tests whether required feature members are written, even if they are null.
        /// </summary>
        [Test]
        public void WriteJsonShouldIgnoreCustomNullWritingOptionsTest()
        {
            IFeature value = new Feature(null, null);
            var options = DefaultOptions;
            options.WriteIndented = false;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            string json = ToJsonString(value, options);
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":null,\"properties\":null}", json);
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
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            string json = ToJsonString(value, options);
            var deserialized = Deserialize(json, options);
            CheckEquality(value, deserialized);
            //Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.1,56.2]},\"properties\":{\"test1\":[\"value1\",\"value2\"]}}", ToJson(value));
        }

        [TestCaseSource(nameof(FeatureIdTestCases))]
        public void WriteJsonWithIdTest(string idPropertyName, object id)
        {
            var value = new Feature
            {
                Geometry = GeometryFactory.Default.CreatePoint(new Coordinate(23.1, 56.2)),
                Attributes = new AttributesTable
                {
                    { idPropertyName, id },
                    { TestContext.CurrentContext.Random.GetString(), TestContext.CurrentContext.Random.NextGuid() },
                },
            };

            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new GeoJsonConverterFactory(GeometryFactory.Default, false, idPropertyName),
                },
            };

            string json = ToJsonString(value, options);
            var deserialized = Deserialize(json, options);
            CheckEquality(value, deserialized, idPropertyName);
        }

        public static IEnumerable<object[]> FeatureIdTestCases
        {
            get
            {
                var ctx = TestContext.CurrentContext;
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextByte() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextSByte() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextShort() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextUShort() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.Next() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextUInt() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextLong() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextULong() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextFloat() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextDouble() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextDecimal() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.GetString() };
                yield return new object[] { ctx.Random.GetString(), ctx.Random.NextGuid() };
                yield return new object[] { ctx.Random.GetString(), new DateTime(ctx.Random.NextLong(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks + 1)) };
                yield return new object[] { ctx.Random.GetString(), new DateTimeOffset(ctx.Random.NextLong(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks + 1), TimeSpan.FromHours(ctx.Random.Next(-14, 15))) };
            }
        }

        public static void CheckEquality(IFeature s, IFeature d, string idPropertyName = null, RingOrientationOption roo = RingOrientationOption.EnforceRfc9746)
        {
            idPropertyName ??= GeoJsonConverterFactory.DefaultIdPropertyName;

            Assert.That(d, Is.Not.Null);

            if (roo == RingOrientationOption.DoNotModify)
                Assert.That(s.Geometry.EqualsExact(d.Geometry));
            else
                Assert.That(s.Geometry.EqualsTopologically(d.Geometry));

            AttributesTableConverterTest.TestEquality(s.Attributes, d.Attributes, idPropertyName);

            if (s.BoundingBox != null)
            {
                Assert.That(d.BoundingBox, Is.EqualTo(s.BoundingBox));
            }

            if (s.GetOptionalId(idPropertyName) is object sId)
            {
                if (d.GetOptionalId(idPropertyName) is object dId)
                {
                    switch (dId)
                    {
                        // ALL number values get boxed as decimals.
                        case decimal _:
                            sId = JsonSerializer.Deserialize<decimal>(JsonSerializer.Serialize(sId));
                            break;

                        // ALL string values get boxed as strings.
                        case string _:
                            sId = JsonSerializer.Deserialize<string>(JsonSerializer.Serialize(sId));
                            break;

                        // RFC7946, 3.2 says "the value of this member is either
                        // a JSON string or number.
                        default:
                            Assert.Fail("Feature IDs must be either a string or number.");
                            break;
                    }

                    Assert.That(dId, Is.EqualTo(sId));
                }
                else
                {
                    Assert.Fail("s had ID, but d did not.");
                }
            }
            else
            {
                Assert.That(d.GetOptionalId(idPropertyName) is null);
            }
        }

        [GeoJsonIssueNumber(79)]
        [Test]
        public void TestFeatureIdSerializedToRoot()
        {
            var feature = new Feature
            {
                Geometry = new Point(0, 0),
                Attributes = new AttributesTable(new Dictionary<string, object> {
                    { "name", "Test feature" },
                    { "id", 1 }
                })
            };

            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new GeoJsonConverterFactory(GeometryFactory.Default, false, "id"),
                },
            };

            string expected = "{\"type\":\"Feature\",\"id\":1,\"geometry\":{\"type\":\"Point\",\"coordinates\":[0,0]},\"properties\":{\"name\":\"Test feature\"}}";
            Assert.That(JsonSerializer.Serialize(feature, options), Is.EqualTo(expected));
        }

        [GeoJsonIssueNumber(132)]
        [Test]
        public void TestNumericFeatureIdMustBeValidDecimal()
        {
            string serialized = $@"
{{
    ""type"": ""Feature"",
    ""id"": {double.MaxValue},
    ""geometry"": {{
        ""type"": ""Point"",
        ""coordinates"": [0, 0]
    }}
}}
            ";
            
            Assert.That(() => JsonSerializer.Deserialize<Feature>(serialized, DefaultOptions), Throws.InstanceOf<JsonException>());
        }
    }
}
