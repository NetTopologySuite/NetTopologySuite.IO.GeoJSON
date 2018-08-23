using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    public class NullGeometryTest
    {
        [Test]
        public void TestDeserializeJsonObjectWithNullGeometry()
        {
            const string json = @"
                {
                    ""id"": 1,
                    ""geometry"": null,
                    ""theme"": {
                        ""id"": 1
                    }
                }
            ";
            var reader = new GeoJsonReader();
            var example = reader.Read<Example>(json);
            Assert.That(example.Geometry, Is.Null);
            Assert.That(example.Theme, Is.Not.Null);
            Assert.That(example.Theme.Id, Is.EqualTo(1));
        }

        [Test]
        public void TestDeserializeJsonObjectWithGeometry()
        {
            const string json = @"
                {
                    ""id"": 1,
                    ""geometry"": {""type"":""Point"",""coordinates"":[23.0,56.0]},
                    ""theme"": {
                        ""id"": 1
                    }
                }
            ";
            var reader = new GeoJsonReader();
            var example = reader.Read<Example>(json);
            Assert.That(example.Geometry, Is.Not.Null);
            Assert.That(example.Theme, Is.Not.Null);
            Assert.That(example.Theme.Id, Is.EqualTo(1));
        }
    }

    class Example
    {
        public int Id { get; set; }
        public Geometry Geometry { get; set; }
        public Theme Theme { get; set; }
    }

    class Theme
    {
        public int Id { get; set; }
    }
}
