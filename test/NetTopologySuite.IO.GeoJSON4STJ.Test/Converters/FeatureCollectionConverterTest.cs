﻿using System;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    [TestFixture]
    public class FeatureCollectionConverterTest : SandDTest<FeatureCollection>
    {
        ///<summary>
        ///A test for CanConvert
        ///</summary>
        [Test]
        public void CanConvertTest()
        {
            var options = DefaultOptions;
            var target = (JsonConverter<FeatureCollection>)GeoJsonConverterFactory.CreateConverter(typeof(FeatureCollection), options);
            var objectType = typeof(FeatureCollection);
            const bool expected = true;
            bool actual = target.CanConvert(objectType);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestRead()
        {
            Assert.Ignore();
        }

        [Test]
        public void TestWrite()
        {
            Assert.Ignore();
        }

        [TestCase(OgcGeometryType.Point, 5, false)]
        [TestCase(OgcGeometryType.Point, 5, true)]
        [TestCase(OgcGeometryType.LineString, 5, false)]
        [TestCase(OgcGeometryType.LineString, 5, true)]
        [TestCase(OgcGeometryType.Polygon, 5, false)]
        [TestCase(OgcGeometryType.Polygon, 5, true)]
        [TestCase(OgcGeometryType.MultiPoint, 5, false)]
        [TestCase(OgcGeometryType.MultiPoint, 5, true)]
        [TestCase(OgcGeometryType.MultiLineString, 5, false)]
        [TestCase(OgcGeometryType.MultiLineString, 5, true)]
        [TestCase(OgcGeometryType.MultiPolygon, 5, false)]
        [TestCase(OgcGeometryType.MultiPolygon, 5, true)]
        public void TestSanD(OgcGeometryType type, int num, bool threeD)
        {
            var fc = new FeatureCollection();
            for (int i = 0; i < num; i++)
            {
                fc.Add(FeatureFactory.Create(type, ("id", TypeCode.Int32),
                    ("label", TypeCode.String), ("number1", TypeCode.Double),
                    ("number2", TypeCode.Int64)
                    ));
            }

            var options = DefaultOptions;
            options.IgnoreNullValues = true;
            string json = ToJsonString(fc, options);
            var d = Deserialize(json, options);

            Assert.That(d, Is.Not.Null);
            Assert.That(d.Count, Is.EqualTo(fc.Count));
            for (int i = 0; i < fc.Count; i++)
                FeatureConverterTest.CheckEquality(fc[i], d[i]);
        }
    }
}
