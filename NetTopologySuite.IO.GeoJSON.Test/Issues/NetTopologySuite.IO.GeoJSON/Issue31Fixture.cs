using GeoAPI.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite.IO.GeoJSON
{
    [Category("TestFixture for Issue 31")]
    [NtsIssueNumber(31)]
    [TestFixture]
    public class Issue31Fixture
    {
        [Test(Description = "geojson_array_should_deserialize_a_coordinate")]
        public void geojson_array_should_deserialize_a_coordinate()
        {
            var geoSerializer = GeoJsonSerializer.CreateDefault();

            var coordinate = new Coordinate(1, 2);
            var sb = new StringBuilder();
            geoSerializer.Serialize(new JsonTextWriter(new StringWriter(sb)), coordinate);

            string input = sb.ToString(); 

            var resultCorrdinate = geoSerializer.Deserialize<Coordinate>(new JsonTextReader(new StringReader(input)));

            Assert.IsNotNull(resultCorrdinate);
            Assert.AreEqual(coordinate.X, resultCorrdinate.X);
            Assert.AreEqual(coordinate.Y, resultCorrdinate.Y);
            Assert.AreEqual(coordinate.Z, resultCorrdinate.Z);
        }

        [Test(Description = "geojson_object_should_deserialize_a_coordinate")]
        public void geojson_object_should_deserialize_a_coordinate()
        {
            var geoSerializer = GeoJsonSerializer.CreateDefault();

            var coordinate = new Coordinate(1, 2);
            var sb = new StringBuilder("{");
            sb.AppendFormat("\"coordinates\":[{0},{1}]", coordinate.X, coordinate.Y);
            sb.Append("}");
            string input = sb.ToString();

            var resultCorrdinate = geoSerializer.Deserialize<Coordinate>(new JsonTextReader(new StringReader(input)));

            Assert.IsNotNull(resultCorrdinate);
            Assert.AreEqual(coordinate.X, resultCorrdinate.X);
            Assert.AreEqual(coordinate.Y, resultCorrdinate.Y);
            Assert.AreEqual(coordinate.Z, resultCorrdinate.Z);
        }

        [Test(Description = "geojson_array_of_array_should_be_null")]
        public void geojson_array_of_array_should_be_null()
        {
            var geoSerializer = GeoJsonSerializer.CreateDefault();

            var coordinate = new Coordinate(1, 2);
            var sb = new StringBuilder("{");
            sb.AppendFormat("\"coordinates\":[[{0},{1}],[{0},{1}]]", coordinate.X, coordinate.Y);
            sb.Append("}");
            string input = sb.ToString();

            var resultCorrdinate = geoSerializer.Deserialize<Coordinate>(new JsonTextReader(new StringReader(input)));

            Assert.IsNull(resultCorrdinate);
        }

    }
}
