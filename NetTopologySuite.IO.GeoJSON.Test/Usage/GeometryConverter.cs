using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Usage
{
    public class GeometryConverterTest
    {
        [Test]
        public void TestGeometryConverterAttribute()
        {
            var myModelItem = new MyModelItem();
            myModelItem.Geom = new global::NetTopologySuite.Geometries.MultiLineString(
                new GeoAPI.Geometries.ILineString[]
                {
                    new global::NetTopologySuite.Geometries.LineString(new[]
                        {new GeoAPI.Geometries.Coordinate(10, 10), new GeoAPI.Geometries.Coordinate(20, 10)}),
                    new global::NetTopologySuite.Geometries.LineString(new[]
                        {new GeoAPI.Geometries.Coordinate(10, 11), new GeoAPI.Geometries.Coordinate(20, 11)}),
                    new global::NetTopologySuite.Geometries.LineString(new[]
                        {new GeoAPI.Geometries.Coordinate(10, 12), new GeoAPI.Geometries.Coordinate(20, 12)})
                });

            var s = GeoJsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings(),
                NtsGeometryServices.Instance.CreateGeometryFactory(31467));
            var sb = new System.Text.StringBuilder();
            s.Serialize(new Newtonsoft.Json.JsonTextWriter(new System.IO.StringWriter(sb)), myModelItem);
            System.Console.WriteLine(sb.ToString());

            var myModelItem2 =
                s.Deserialize<MyModelItem>(
                    new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(sb.ToString())));
        }
    }

    public class MyModelItem
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "geometry",
            ItemConverterType = typeof(NetTopologySuite.IO.Converters.GeometryConverter))]
        public global::NetTopologySuite.Geometries.MultiLineString Geom { get; set; }
    }
}
