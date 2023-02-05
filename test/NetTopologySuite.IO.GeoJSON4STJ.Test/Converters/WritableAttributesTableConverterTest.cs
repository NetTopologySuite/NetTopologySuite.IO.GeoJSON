using System;
using System.Text.Json.Nodes;

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    [TestFixture]
    public sealed class WritableAttributesTableConverterTest : SandDTest<IFeature>
    {
        public WritableAttributesTableConverterTest()
            : base(new GeoJsonConverterFactory(null, false, null, RingOrientationOption.EnforceRfc9746, true))
        {
        }

        [Test]
        public void ModificationsShouldBeVisibleNearlyEverywhere()
        {
            IFeature feature = FromJsonString(@"{
    ""type"": ""Feature"",
    ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [-74.0445, 40.6892]
    },
    ""properties"": {
        ""hello"": ""world!""
    }
}", DefaultOptions);

            Assert.That(feature.Geometry, Is.InstanceOf<Point>());
            Assert.That(feature.Geometry.Coordinate, Is.EqualTo(new Coordinate(-74.0445, 40.6892)));

            Assert.That(feature.Attributes, Is.InstanceOf<JsonObjectAttributesTable>());
            JsonObjectAttributesTable attributes = (JsonObjectAttributesTable)feature.Attributes;
            JsonObject rootObject = attributes.RootObject; // modifications should write through

            Assert.That(feature.Attributes.Exists("hello"));
            Assert.That(feature.Attributes["hello"], Is.EqualTo("world!"));
            Assert.That(rootObject.TryGetPropertyValue("hello", out JsonNode helloValue));
            Assert.That(helloValue.GetValue<string>(), Is.EqualTo("world!"));

            feature.Attributes.DeleteAttribute("hello");
            Assert.That(!feature.Attributes.Exists("hello"));
            Assert.That(!rootObject.ContainsKey("hello"));

            // initialize "nearestGasStation" to just a string at first...
            feature.Attributes.Add("nearestGasStation", "there are none");
            Assert.That(feature.Attributes.Exists("nearestGasStation"));
            Assert.That(feature.Attributes["nearestGasStation"], Is.EqualTo("there are none"));
            Assert.That(rootObject.TryGetPropertyValue("nearestGasStation", out JsonNode nearestGasStationValue));
            Assert.That(nearestGasStationValue.GetValue<string>(), Is.EqualTo("there are none"));

            // ...but override it right away with a complex object.
            GasStation nearestGasStation = new GasStation
            {
                Id = Guid.NewGuid(),
                Name = "Somebody",
                Location = GeometryFactory.Default.CreatePoint(new Coordinate(1, 3)),
            };
            feature.Attributes["nearestGasStation"] = nearestGasStation;

            Assert.That(feature.Attributes["nearestGasStation"], Is.InstanceOf<JsonObjectAttributesTable>());
            JsonObjectAttributesTable nearestGasStationAttribute = (JsonObjectAttributesTable)feature.Attributes["nearestGasStation"];

            Assert.That(nearestGasStationAttribute["Location"], Is.InstanceOf<JsonObjectAttributesTable>());
            JsonObjectAttributesTable nearestGasStationLocationAttribute = (JsonObjectAttributesTable)nearestGasStationAttribute["Location"];

            Assert.That(nearestGasStationLocationAttribute.TryDeserializeJsonObject(null, out Point nearestGasStationLocationPt));
            Assert.That(nearestGasStationLocationPt.Coordinate, Is.EqualTo(new Coordinate(1, 3)));

            nearestGasStationLocationAttribute["coordinates"] = new JsonArray(JsonValue.Create(6.0), JsonValue.Create(8.0));
            Assert.That(((JsonObjectAttributesTable)nearestGasStationAttribute["Location"]).TryDeserializeJsonObject(null, out nearestGasStationLocationPt));
            Assert.That(nearestGasStationLocationPt.Coordinate, Is.EqualTo(new Coordinate(6, 8)));

            // overwriting the coordinates at the deepest attributes table that we create in our
            // tree should write through ALL the way to the topmost root object.
            Assert.That(((JsonValue)(((JsonArray)((JsonObject)((JsonObject)rootObject["nearestGasStation"])["Location"])["coordinates"])[0])).GetValue<double>(), Is.EqualTo(6.0));
            Assert.That(((JsonValue)(((JsonArray)((JsonObject)((JsonObject)rootObject["nearestGasStation"])["Location"])["coordinates"])[1])).GetValue<double>(), Is.EqualTo(8.0));

            // all these modifications should be visible after a round-trip through JSON
            IFeature roundTripFeature = RoundTrip(feature, DefaultOptions);
            JsonObjectAttributesTable roundTripAttributes = (JsonObjectAttributesTable)roundTripFeature.Attributes;
            Assert.Multiple(() =>
            {
                Assert.That(!roundTripAttributes.Exists("hello"));
                Assert.That(roundTripAttributes.TryGetJsonObjectPropertyValue("nearestGasStation", null, out GasStation roundTripNearestGasStation));
                Assert.That(roundTripNearestGasStation.Id, Is.EqualTo(nearestGasStation.Id));
                Assert.That(roundTripNearestGasStation.Name, Is.EqualTo(nearestGasStation.Name));
                Assert.That(roundTripNearestGasStation.Location.Coordinate, Is.EqualTo(new Coordinate(6, 8)));
            });
        }

        private sealed class GasStation
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public Point Location { get; set; }
        }
    }
}
