using System;
using System.Collections.Generic;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    internal static class FeatureFactory
    {
        private const int PrecisionScale = 100;
        private static readonly Random RND = new Random(17);
        private static readonly PrecisionModel PM = new PrecisionModel(PrecisionScale);
        private static readonly GeometryFactory GF = new GeometryFactory(PM, 4326);
        private static readonly string[] RNDNames = new[] {"Random", "Zufall", "Hasard", "Caso", "Azar"};
        private static readonly Geometry Bounds = GF.ToGeometry(new Envelope(-BoundX, BoundX, -BoundY, BoundY));

        public static IFeature Create(OgcGeometryType geometryType, params (string, TypeCode)[] properties)
        {
            var geom = CreateRandomGeometry(geometryType);
            var att = CreateRandomAttributes(properties);
            return new Feature(geom, att);
        }

        private static IAttributesTable CreateRandomAttributes((string, TypeCode)[] properties)
        {
            var res = new AttributesTable();
            foreach ((string name, var type) in properties)
            {
                object value = null;
                switch (type)
                {
                    case TypeCode.Boolean:
                        value = RND.NextDouble() > 0.5d;
                        break;
                    case TypeCode.Double:
                        value = (decimal)(500d * RND.NextDouble());
                        break;
                    case TypeCode.Single:
                        value = 500f * (float)RND.NextDouble();
                        break;
                    case TypeCode.Empty:
                        value = null;
                        break;
                    case TypeCode.Int16:
                        value = (short) RND.Next(short.MinValue, short.MaxValue);
                        break;
                    case TypeCode.Int32:
                        value = RND.Next(int.MinValue, int.MaxValue);
                        break;
                    case TypeCode.Int64:
                        value = 5L * (long)RND.Next(int.MinValue, int.MaxValue);
                        break;
                    case TypeCode.String:
                        value = RandomString();
                        break;
                    case TypeCode.Object:
                        value = System.Guid.NewGuid();
                        break;
                    default:
                        value = RND.NextDouble() > 0.5d ? RandomString() : null;
                        break;
                }
                res.Add(name, value);
            }

            return res;
        }

        private static object RandomString()
        {
            return $"{RNDNames[RND.Next(0, RNDNames.Length)]} - {RND.Next(0, 500)}";
        }

        public static Geometry CreateRandomGeometry(in OgcGeometryType geometryType, in bool threeD = false)
        {
            switch (geometryType)
            {
                case OgcGeometryType.Point:
                    return CreatePoint(threeD);
                case OgcGeometryType.LineString:
                    return CreateLineString(threeD);
                case OgcGeometryType.Polygon:
                    return CreatePolygon(threeD);
                case OgcGeometryType.MultiPoint:
                    return CreateMultiPoint(threeD);
                case OgcGeometryType.MultiLineString:
                    return CreateMultiLineString(threeD);
                case OgcGeometryType.MultiPolygon:
                    return CreateMultiPolygon(threeD);
                case OgcGeometryType.GeometryCollection:
                    return CreateGeometryCollection(threeD);
            }
            throw new NotSupportedException();
        }

        const int BoundX = 180 * PrecisionScale;
        const int BoundY = 90 * PrecisionScale;
        const int BoundZ = 100 * PrecisionScale;

        private static Coordinate CreateRandomCoordinate(in bool threeD = false)
        {
            double x = (double)RND.Next(-BoundX, BoundX) / PrecisionScale;
            double y = (double)RND.Next(-BoundY, BoundY) / PrecisionScale;

            var res = threeD
                ? new CoordinateZ(x, y, (double) RND.Next(0, BoundZ) / PrecisionScale)
                : new Coordinate(x, y);
            PM.MakePrecise(res);
            return res;
        }


        public static Point CreatePoint(in bool threeD)
        {
            return GF.CreatePoint(CreateRandomCoordinate(threeD));
        }

        public static LineString CreateLineString(bool threeD)
        {
            var start = CreateRandomCoordinate(threeD);
            var end = CreateRandomCoordinate(threeD);
            double[] pointsAt = CreatePositions(RND.Next(6));

            var v = Vector2D.Create(start, end).Normalize();
            v = Vector2D.Create(v.Y, v.X);

            var ls = new LineSegment(start, end);
            double dz = (end.Z - start.Z) / ls.Length;

            var cs = GF.CoordinateSequenceFactory.Create(pointsAt.Length + 2, threeD ? 3 : 2);
            int j = 0;
            SetCoordinate(cs, j++, start);
            for (int i = 0; i < pointsAt.Length; i++)
            {
                var pt = ls.PointAlong(pointsAt[i]);
                var tmp = v.Multiply(10d * RND.NextDouble());
                if (threeD)
                    pt = new CoordinateZ(pt.X + tmp.X, pt.Y + tmp.Y, start.Z + dz * pointsAt[i]);
                else
                    pt = new Coordinate(pt.X + tmp.X, pt.Y + tmp.Y);

                Clamp(pt);
                PM.MakePrecise(pt);
                SetCoordinate(cs, j++, pt);
            }
            SetCoordinate(cs, j, end);

            return GF.CreateLineString(cs);
        }

        private static Polygon CreatePolygon(in bool threeD)
        {
            var centre = CreateRandomCoordinate(threeD);
            double radius = PM.MakePrecise(0.5 + 3d * RND.NextDouble());

            var polygon = (Polygon)GF.CreatePoint(centre).Buffer(radius, 3);
            if (RND.NextDouble() < 0.5d)
                return polygon;

            int numHoles = RND.Next(1, 3);
            radius *= 0.3;
            Geometry hole = null;
            for (int i = 0; i < numHoles; i++)
            {
                centre = Move(centre, 2 * Math.PI * RND.NextDouble(), RND.NextDouble() * 1.5 * radius);
                var tmp = GF.CreatePoint(centre).Buffer(radius, 3);
                if (hole == null)
                    hole = tmp;
                else
                    hole = hole.Union(tmp);
            }

            polygon = (Polygon)polygon.Difference(hole);

            return (Polygon)Bounds.Intersection(polygon);

        }

        private static MultiPoint CreateMultiPoint(in bool threeD)
        {
            var geoms = new Point[RND.Next(2, 10)];
            for (int i = 0; i < geoms.Length; i++)
                geoms[i] = (Point)CreatePoint(threeD);
            return GF.CreateMultiPoint(geoms);
        }

        private static MultiLineString CreateMultiLineString(in bool threeD)
        {
            var geoms = new LineString[RND.Next(2, 10)];
            for (int i = 0; i < geoms.Length; i++)
                geoms[i] = (LineString)CreateLineString(threeD);
            return GF.CreateMultiLineString(geoms);
        }

        private static MultiPolygon CreateMultiPolygon(in bool threeD)
        {
            var geoms = new Polygon[RND.Next(2, 10)];
            for (int i = 0; i < geoms.Length; i++)
                geoms[i] = (Polygon)CreatePolygon(threeD);
            return GF.CreateMultiPolygon(geoms);
        }

        private static Geometry CreateGeometryCollection(in bool threeD)
        {
            var geoms = new Geometry[RND.Next(2, 5)];
            for (int i = 0; i < geoms.Length; i++)
            {
                switch (RND.Next(2))
                {
                    case 0:
                        geoms[i] = CreatePoint(threeD);
                        break;
                    case 1:
                        geoms[i] = CreateLineString(threeD);
                        break;
                    case 2:
                        geoms[i] = CreatePolygon(threeD);
                        break;
                }
            }
            return GF.CreateGeometryCollection(geoms);
        }

        private static Coordinate Move(Coordinate centre, double radians, double distance)
        {
            centre = centre.Copy();
            double dx = distance * Math.Cos(radians);
            double dy = distance * Math.Sin(radians);
            centre.X += dx;
            centre.Y += dy;

            return centre;
        }

        private static void Clamp(Coordinate coord)
        {
            if (coord.X < -BoundX) coord.X = -BoundX;
            if (coord.X > BoundX) coord.X = BoundX;
            if (coord.Y < -BoundY) coord.Y = -BoundY;
            if (coord.Y > BoundY) coord.Y = BoundY;
        }

        private static void SetCoordinate(CoordinateSequence cs, int index, Coordinate coord)
        {
            cs.SetX(index, coord.X);
            cs.SetY(index, coord.Y);
            if (coord is CoordinateZ)
                cs.SetZ(index, coord.Z);
        }

        private static double[] CreatePositions(int num)
        {
            var tmp = new List<double>(num);
            for(int i = 0; i < num; i++)
                tmp.Add(RND.NextDouble());
            tmp.Sort();
            return tmp.ToArray();
        }
    }
}
