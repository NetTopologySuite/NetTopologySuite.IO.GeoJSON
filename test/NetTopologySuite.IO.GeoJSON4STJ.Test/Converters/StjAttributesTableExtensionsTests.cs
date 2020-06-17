using System;
using System.Text.Json;

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    public sealed class StjAttributesTableExtensionsTests
    {
        [Test]
        public void TryGetJsonObjectPropertyValueShouldFailGracefullyWhenAttributesTableWasNotCreatedHere()
        {
            var table = new AttributesTable { { "gasStation", new GasStation() } };
            Assert.That(!table.TryGetJsonObjectPropertyValue("gasStation", new JsonSerializerOptions(), out GasStation _));
        }

        [Test]
        public void TryGetJsonObjectPropertyValueShouldFailGracefullyWhenPropertyIsAbsent()
        {
            const string Json = @"{
    ""type"": ""Feature"",
    ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [-74.0445, 40.6892]
    },
    ""properties"": {
        ""heightInFeet"": 305
    }
}";
            var options = new JsonSerializerOptions
            {
                Converters = { new GeoJsonConverterFactory() },
                PropertyNameCaseInsensitive = true,
            };

            var feature = JsonSerializer.Deserialize<IFeature>(Json, options);
            Assert.That(feature.Geometry, Is.InstanceOf<Point>());
            Assert.That(feature.Geometry.Coordinate, Is.EqualTo(new Coordinate(-74.0445, 40.6892)));
            Assert.That(feature.Attributes, Is.Not.Null);

            Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("heightInFeet", options, out int heightInFeet));
            Assert.That(heightInFeet, Is.EqualTo(305));

            Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("nearestGasStation", options, out GasStation _));
        }

        [Test]
        public void TryGetJsonObjectPropertyValueShouldActAppropriatelyWhenPropertyIsPresent()
        {
            const string Json = @"{
    ""type"": ""Feature"",
    ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [-74.0445, 40.6892]
    },
    ""properties"": {
        ""u8"": 245,
        ""s8"": -12,
        ""s16"": -32751,
        ""u16"": 60000,
        ""s32"": -250000000,
        ""u32"": 3000000000,
        ""s64"": -800000000000,
        ""u64"": 10000000000000000000,
        ""f32"": 0.3581578,
        ""f64"": 7.222535371164569,
        ""d128"": 0.1,
        ""dt"": ""2020-06-07T09:00:00"",
        ""dto"": ""2020-06-07T09:00:00-04:00"",
        ""guid"": ""8CAD310E-5891-4CE7-B184-3B2F62AAC600"",
        ""str"": ""bunny"",
        ""null"": null,
        ""nearestGasStation"": {
            ""id"": ""F44EC407-B5C2-4A1E-9D4A-0D8CE930E742"",
            ""name"": ""Cavenpoint Exxon"",
            ""location"": {
                ""type"": ""Point"",
                ""coordinates"": [-74.0737, 40.7060]
            },
            ""owner"": {
                ""id"": ""B0536C0C-0577-4046-A98C-224758BD245C"",
                ""name"": ""Fakey McNamerson"",
                ""age"": 41
            }
        }
    }
}";
            var options = new JsonSerializerOptions
            {
                Converters = { new GeoJsonConverterFactory() },
                PropertyNameCaseInsensitive = true,
            };

            var feature = JsonSerializer.Deserialize<IFeature>(Json, options);

            Assert.That(feature.Geometry, Is.InstanceOf<Point>());
            Assert.That(feature.Geometry.Coordinate, Is.EqualTo(new Coordinate(-74.0445, 40.6892)));
            Assert.That(feature.Attributes, Is.Not.Null);

            Assert.Multiple(() =>
            {
                // simple types: success
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u8", options, out byte u8), () => "convert u8");
                Assert.That(u8, Is.EqualTo(245));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u8", options, out byte? u8Nullable), () => "convert u8Nullable");
                Assert.That(u8Nullable, Is.EqualTo(245));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out u8Nullable), () => "convert u8Nullable null");
                Assert.That(u8Nullable, Is.Null, () => "null u32Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s8", options, out sbyte s8), () => "convert s8");
                Assert.That(s8, Is.EqualTo(-12));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s8", options, out sbyte? s8Nullable), () => "convert s8Nullable");
                Assert.That(s8Nullable, Is.EqualTo(-12));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out s8Nullable), () => "convert s8Nullable null");
                Assert.That(s8Nullable, Is.Null, () => "null u32Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s16", options, out short s16), () => "convert s16");
                Assert.That(s16, Is.EqualTo(-32751));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s16", options, out short? s16Nullable), () => "convert s16Nullable");
                Assert.That(s16Nullable, Is.EqualTo(-32751));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out s16Nullable), () => "convert s16Nullable null");
                Assert.That(s16Nullable, Is.Null, () => "null u32Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u16", options, out ushort u16), () => "convert u16");
                Assert.That(u16, Is.EqualTo(60000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u16", options, out ushort? u16Nullable), () => "convert u16Nullable");
                Assert.That(u16Nullable, Is.EqualTo(60000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out u16Nullable), () => "convert u16Nullable null");
                Assert.That(u16Nullable, Is.Null, () => "null u32Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s32", options, out int s32), () => "convert s32");
                Assert.That(s32, Is.EqualTo(-250000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s32", options, out int? s32Nullable), () => "convert s32Nullable");
                Assert.That(s32Nullable, Is.EqualTo(-250000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out s32Nullable), () => "convert s32Nullable null");
                Assert.That(s32Nullable, Is.Null, () => "null u32Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u32", options, out uint u32), () => "convert u32");
                Assert.That(u32, Is.EqualTo(3000000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u32", options, out uint? u32Nullable), () => "convert u32Nullable");
                Assert.That(u32Nullable, Is.EqualTo(3000000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out u32Nullable), () => "convert u32Nullable null");
                Assert.That(u32Nullable, Is.Null, () => "null u32Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s64", options, out long s64), () => "convert s64");
                Assert.That(s64, Is.EqualTo(-800000000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("s64", options, out long? s64Nullable), () => "convert s64Nullable");
                Assert.That(s64Nullable, Is.EqualTo(-800000000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out s64Nullable), () => "convert s64Nullable null");
                Assert.That(s64Nullable, Is.Null, () => "null s64Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u64", options, out ulong u64), () => "convert u64");
                Assert.That(u64, Is.EqualTo(10000000000000000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("u64", options, out ulong? u64Nullable), () => "convert u64Nullable");
                Assert.That(u64Nullable, Is.EqualTo(10000000000000000000));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out u64Nullable), () => "convert u64Nullable null");
                Assert.That(u64Nullable, Is.Null, () => "null u64Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("f32", options, out float f32), () => "convert f32");
                Assert.That(f32, Is.EqualTo(0.3581578f));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("f32", options, out float? f32Nullable), () => "convert f32Nullable");
                Assert.That(f32Nullable, Is.EqualTo(0.3581578f));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out f32Nullable), () => "convert f32Nullable null");
                Assert.That(f32Nullable, Is.Null, () => "null f32Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("f64", options, out double f64), () => "convert f64");
                Assert.That(f64, Is.EqualTo(7.222535371164569));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("f64", options, out double? f64Nullable), () => "convert f64Nullable");
                Assert.That(f64Nullable, Is.EqualTo(7.222535371164569));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out f64Nullable), () => "convert f64Nullable null");
                Assert.That(f64Nullable, Is.Null, () => "null f64Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("d128", options, out decimal d128), () => "convert d128");
                Assert.That(d128, Is.EqualTo(0.1m));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("d128", options, out decimal? d128Nullable), () => "convert d128Nullable");
                Assert.That(d128Nullable, Is.EqualTo(0.1m));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out d128Nullable), () => "convert d128Nullable null");
                Assert.That(d128Nullable, Is.Null, () => "null d128Nullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("dt", options, out DateTime dt), () => "convert dt");
                Assert.That(dt, Is.EqualTo(new DateTime(2020, 6, 7, 9, 0, 0, 0, DateTimeKind.Unspecified)));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("dt", options, out DateTime? dtNullable), () => "convert dtNullable");
                Assert.That(dtNullable, Is.EqualTo(new DateTime(2020, 6, 7, 9, 0, 0, 0, DateTimeKind.Unspecified)));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out dtNullable), () => "convert dtNullable null");
                Assert.That(dtNullable, Is.Null, () => "null dtNullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("dto", options, out DateTimeOffset dto), () => "convert dto");
                Assert.That(dto, Is.EqualTo(new DateTimeOffset(2020, 6, 7, 9, 0, 0, TimeSpan.FromHours(-4))));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("dto", options, out DateTimeOffset? dtoNullable), () => "convert dtoNullable");
                Assert.That(dtoNullable, Is.EqualTo(new DateTimeOffset(2020, 6, 7, 9, 0, 0, TimeSpan.FromHours(-4))));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out dtoNullable), () => "convert dtoNullable null");
                Assert.That(dtoNullable, Is.Null, () => "null dtoNullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("guid", options, out Guid guid), () => "convert guid");
                Assert.That(guid, Is.EqualTo(new Guid("8CAD310E-5891-4CE7-B184-3B2F62AAC600")));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("guid", options, out Guid? guidNullable), () => "convert guidNullable");
                Assert.That(guidNullable, Is.EqualTo(new Guid("8CAD310E-5891-4CE7-B184-3B2F62AAC600")));
                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out guidNullable), () => "convert guidNullable null");
                Assert.That(guidNullable, Is.Null, () => "null guidNullable");

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("str", options, out string str), () => "convert str");
                Assert.That(str, Is.EqualTo("bunny"));

                Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out string nullString), () => "convert nullString");
                Assert.That(nullString, Is.Null);

                // simple types: wrong JSON token type
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("u8", options, out string _), () => "string from number");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out byte _), () => "byte from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out sbyte _), () => "sbyte from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out short _), () => "short from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out ushort _), () => "ushort from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out int _), () => "int from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out uint _), () => "uint from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out long _), () => "long from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out ulong _), () => "ulong from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out float _), () => "float from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out double _), () => "double from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out decimal _), () => "decimal from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out DateTime _), () => "DateTime from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out DateTimeOffset _), () => "DateTimeOffset from null");
                Assert.That(!feature.Attributes.TryGetJsonObjectPropertyValue("null", options, out Guid _), () => "Guid from null");
            });

            // complex type, with two properties: one of a user-defined complex type, and one that
            // also should be treated as GeoJSON.
            Assert.That(feature.Attributes.TryGetJsonObjectPropertyValue("nearestGasStation", options, out GasStation nearestGasStation));
            Assert.That(nearestGasStation.Id, Is.EqualTo(new Guid("F44EC407-B5C2-4A1E-9D4A-0D8CE930E742")));
            Assert.That(nearestGasStation.Name, Is.EqualTo("Cavenpoint Exxon"));
            Assert.That(nearestGasStation.Location, Is.Not.Null);
            Assert.That(nearestGasStation.Location.Coordinate, Is.EqualTo(new Coordinate(-74.0737, 40.7060)));
            Assert.That(nearestGasStation.Owner, Is.Not.Null);
            Assert.That(nearestGasStation.Owner.Id, Is.EqualTo(new Guid("B0536C0C-0577-4046-A98C-224758BD245C")));
            Assert.That(nearestGasStation.Owner.Name, Is.EqualTo("Fakey McNamerson"));
            Assert.That(nearestGasStation.Owner.Age, Is.EqualTo(41));
        }

        private sealed class GasStation
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public Point Location { get; set; }

            public Person Owner { get; set; }
        }

        private sealed class Person
        {
            public Guid Id { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }
        }
    }
}
