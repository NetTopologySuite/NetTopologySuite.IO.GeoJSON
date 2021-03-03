using System;
using System.IO;
using System.Text;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    [TestFixture]
    public class Test
    {
        private readonly Point _point = new Point(10, 10);
        private readonly LineString _lineString = new LineString(new[] { new Coordinate(10, 10), new Coordinate(20, 20) });

        private readonly Polygon _polygon1 =
            new Polygon(
                new LinearRing(new[]
                                   {
                                       new Coordinate(10, 10), new Coordinate(20, 20), new Coordinate(20, 10),
                                       new Coordinate(10, 10)
                                   }));

        private readonly Polygon _polygon2 = new Polygon(
            new LinearRing(new[]
                               {
                                   new Coordinate(10, 10), new Coordinate(20, 20), new Coordinate(20, 10),
                                   new Coordinate(10, 10)
                               }),
            new[]
                {
                    new LinearRing(new[]
                                       {
                                           new Coordinate(11, 11), new Coordinate(19, 11), new Coordinate(19, 19),
                                           new Coordinate(11, 11)
                                       })
                });
        private readonly MultiPoint _multiPoint = new MultiPoint(new[] { new Point(10, 10), new Point(11, 11), new Point(12, 12), });


        private readonly MultiPolygon _multiPolygon =
            new MultiPolygon(
                new[] {
                    new Polygon(
                        new LinearRing(new[] { new Coordinate (10, 10), new Coordinate (20, 20),
                                               new Coordinate (20, 10), new Coordinate (10, 10) }),
                        new[]
                        {
                            new LinearRing(new[] { new Coordinate (11, 11), new Coordinate (19, 11),
                                                   new Coordinate (19, 19), new Coordinate (11, 11)})
                        }),
                    new Polygon(
                        new LinearRing(new[] { new Coordinate (10, 10), new Coordinate (20, 20),
                                               new Coordinate (20, 10), new Coordinate (10, 10) }))

                });



        private readonly MultiLineString _multiLineString =
            new MultiLineString(new[]
                                    {
                                        new LineString(new[] {new Coordinate(10, 10), new Coordinate(20, 20)}),
                                        new LineString(new[] {new Coordinate(10, 11), new Coordinate(20, 21)})
                                    });

        [Test]
        public void TestMultiPoly()
        {
            PerformGeometryTest(_multiPolygon);
        }

        [Test]
        public void TestAllGeometries()
        {
            PerformGeometryTest(_point);
            PerformGeometryTest(_lineString);
            PerformGeometryTest(_polygon1);
            PerformGeometryTest(_polygon2);
            PerformGeometryTest(_multiPoint);
            PerformGeometryTest(_multiLineString);
            PerformGeometryTest(_multiPolygon);
            PerformGeometryTest(new GeometryCollection(new[] { (Geometry)_point, _lineString, _polygon2 }));
        }

        public void PerformGeometryTest(Geometry geom)
        {
            var s = GeoJsonSerializer.CreateDefault();
            var sb = new StringBuilder();
            s.Serialize(new JsonTextWriter(new StringWriter(sb)), geom);
            string result = sb.ToString();
            Console.WriteLine(result);

            Deserialize(result, geom);
        }

        private static void Deserialize(string result, Geometry geom)
        {
            var s = GeoJsonSerializer.CreateDefault();
            var r = new JsonTextReader(new StringReader(result));

            Geometry des;

            if (geom is Point)
                des = s.Deserialize<Point>(r);
            else if (geom is LineString)
                des = s.Deserialize<LineString>(r);
            else if (geom is Polygon)
                des = s.Deserialize<Polygon>(r);
            else if (geom is MultiPoint)
                des = s.Deserialize<MultiPoint>(r);
            else if (geom is MultiLineString)
                des = s.Deserialize<MultiLineString>(r);
            else if (geom is MultiPolygon)
                des = s.Deserialize<MultiPolygon>(r);
            else if (geom is GeometryCollection)
                des = s.Deserialize<GeometryCollection>(r);
            else
                throw new Exception();

            Console.WriteLine(des.AsText());
            Assert.IsTrue(des.EqualsExact(geom));
        }

        [Test, Ignore("CoordinateConverter no longer added to serializer")]
        public void TestCoordinateSerialize()
        {
            var coordinate = new Coordinate(1, 1);
            var g = GeoJsonSerializer.CreateDefault();
            var sb = new StringBuilder();
            g.Serialize(new JsonTextWriter(new StringWriter(sb)), coordinate);

            Console.WriteLine(sb.ToString());
        }

        [Test, Ignore("CoordinateConverter no longer added to serializer")]
        public void TestCoordinatesSerialize()
        {
            var coordinates = new Coordinate[4];
            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = new CoordinateZ(i, i, i);
            }
            var sb = new StringBuilder();
            var g = GeoJsonSerializer.CreateDefault();
            g.Serialize(new JsonTextWriter(new StringWriter(sb)), coordinates);

            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void TestCoordinateDeserialize()
        {
            string json = "{coordinates:[1.0, 1.0]}";
            var s = GeoJsonSerializer.CreateDefault();
            var c = s.Deserialize<Coordinate>(new JsonTextReader(new StringReader(json)));
            Console.WriteLine(c.ToString());

        }
    }
}
