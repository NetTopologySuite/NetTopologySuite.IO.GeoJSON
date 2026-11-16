using System;
using System.Buffers;
using System.Text.Json;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Properties;

namespace NetTopologySuite.IO.Converters
{
    // "coordinates" read into plain arrays, so the geometry can be built once "type" is known.
    // _shape has one entry per array, in document order: n > 0 for an array of n arrays, -n for
    // a position of n numbers, 0 for an empty array.  the numbers themselves go in _ords.
    internal struct StjRawCoordinates : IDisposable
    {
        private const int MaxNesting = 64;

        private double[] _ords;
        private int _ordCount;

        private int[] _shape;
        private int _shapeCount;

        private bool _isPresent;
        private bool _isMalformed;

        private int _shapeCursor;
        private int _ordCursor;

        public void Read(ref Utf8JsonReader reader)
        {
            Dispose();

            if (reader.TokenType == JsonTokenType.Null)
            {
                return;
            }

            _isPresent = true;
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                _isMalformed = true;
                reader.SkipOrThrow();
                return;
            }

            _ords = ArrayPool<double>.Shared.Rent(64);
            _shape = ArrayPool<int>.Shared.Rent(16);

            Span<int> open = stackalloc int[MaxNesting];
            int depth = 0;
            open[depth++] = AddShape();

            while (depth > 0)
            {
                reader.ReadOrThrow();
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray:
                    {
                        ref int parent = ref _shape[open[depth - 1]];
                        if (parent < 0)
                        {
                            _isMalformed = true; // numbers and arrays mixed in one array
                        }
                        else
                        {
                            parent++;
                        }

                        if (depth == MaxNesting)
                        {
                            throw new JsonException(string.Format(Resources.EX_UnexpectedToken, reader.TokenType));
                        }

                        open[depth++] = AddShape();
                        break;
                    }

                    case JsonTokenType.EndArray:
                        depth--;
                        break;

                    case JsonTokenType.Number:
                    {
                        ref int parent = ref _shape[open[depth - 1]];
                        if (parent > 0)
                        {
                            _isMalformed = true; // numbers and arrays mixed in one array
                        }
                        else
                        {
                            parent--;
                        }

                        AddOrd(reader.GetDouble());
                        break;
                    }

                    case JsonTokenType.Comment:
                        break;

                    default:
                        _isMalformed = true;
                        reader.SkipOrThrow();
                        break;
                }
            }
        }

        public Geometry ToGeometry(GeoJsonObjectType geometryType, GeometryFactory factory)
        {
            try
            {
                if (_isMalformed)
                {
                    throw new IncompatibleCoordinatesException();
                }

                bool present = _isPresent;
                _shapeCursor = 0;
                _ordCursor = 0;

                switch (geometryType)
                {
                    case GeoJsonObjectType.Point:
                        return present ? ReadPoint(factory) : factory.CreatePoint();

                    case GeoJsonObjectType.LineString:
                        return present ? factory.CreateLineString(ReadSequence(factory)) : factory.CreateLineString();

                    case GeoJsonObjectType.Polygon:
                        return present ? ReadPolygon(factory) : factory.CreatePolygon();

                    case GeoJsonObjectType.MultiPoint:
                        return present ? ReadMultiPoint(factory) : factory.CreateMultiPoint();

                    case GeoJsonObjectType.MultiLineString:
                        return present ? ReadMultiLineString(factory) : factory.CreateMultiLineString();

                    case GeoJsonObjectType.MultiPolygon:
                        return present ? ReadMultiPolygon(factory) : factory.CreateMultiPolygon();

                    default:
                        throw new IncompatibleCoordinatesException();
                }
            }
            catch (IncompatibleCoordinatesException)
            {
                throw new JsonException(string.Format(Resources.EX_CoordinatesIncompatibleWithType, geometryType));
            }
        }

        public void Dispose()
        {
            if (_ords != null)
            {
                ArrayPool<double>.Shared.Return(_ords);
            }

            if (_shape != null)
            {
                ArrayPool<int>.Shared.Return(_shape);
            }

            this = default;
        }

        // depth 1: [x, y, z?] or []
        private Point ReadPoint(GeometryFactory factory)
        {
            int shape = NextShape();
            if (shape == 0)
            {
                return factory.CreatePoint();
            }

            int count = PositionLength(shape);
            int dimension = count >= 3 ? 3 : 2;
            var seq = factory.CoordinateSequenceFactory.Create(1, dimension, 0);
            SetPosition(seq, 0, count, dimension, factory.PrecisionModel);
            return factory.CreatePoint(seq);
        }

        // depth 2: [[x, y], [x, y], ...] or []
        private CoordinateSequence ReadSequence(GeometryFactory factory)
        {
            int count = ArrayLength(NextShape());

            if (_shapeCursor + count > _shapeCount)
            {
                throw new IncompatibleCoordinatesException();
            }

            int dimension = 2;
            for (int i = 0; i < count; i++)
            {
                if (PositionLength(_shape[_shapeCursor + i]) >= 3)
                {
                    dimension = 3;
                }
            }

            var seq = factory.CoordinateSequenceFactory.Create(count, dimension, 0);
            var precisionModel = factory.PrecisionModel;
            for (int i = 0; i < count; i++)
            {
                SetPosition(seq, i, PositionLength(_shape[_shapeCursor++]), dimension, precisionModel);
            }

            return seq;
        }

        // depth 2: [[x, y], [], ...] or []
        private MultiPoint ReadMultiPoint(GeometryFactory factory)
        {
            var points = new Point[ArrayLength(NextShape())];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = ReadPoint(factory);
            }

            return factory.CreateMultiPoint(points);
        }

        // depth 3: [ring, ring, ...] or []
        private Polygon ReadPolygon(GeometryFactory factory)
        {
            int count = ArrayLength(NextShape());
            if (count == 0)
            {
                return factory.CreatePolygon();
            }

            var shell = factory.CreateLinearRing(ReadSequence(factory));
            var holes = new LinearRing[count - 1];
            for (int i = 0; i < holes.Length; i++)
            {
                holes[i] = factory.CreateLinearRing(ReadSequence(factory));
            }

            return factory.CreatePolygon(shell, holes);
        }

        // depth 3: [line, line, ...] or []
        private MultiLineString ReadMultiLineString(GeometryFactory factory)
        {
            var lines = new LineString[ArrayLength(NextShape())];
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = factory.CreateLineString(ReadSequence(factory));
            }

            return factory.CreateMultiLineString(lines);
        }

        // depth 4: [polygon, polygon, ...] or []
        private MultiPolygon ReadMultiPolygon(GeometryFactory factory)
        {
            var polygons = new Polygon[ArrayLength(NextShape())];
            for (int i = 0; i < polygons.Length; i++)
            {
                polygons[i] = ReadPolygon(factory);
            }

            return factory.CreateMultiPolygon(polygons);
        }

        private int NextShape()
        {
            if (_shapeCursor >= _shapeCount)
            {
                throw new IncompatibleCoordinatesException();
            }

            return _shape[_shapeCursor++];
        }

        private static int ArrayLength(int shape)
            => shape >= 0 ? shape : throw new IncompatibleCoordinatesException();

        private static int PositionLength(int shape)
            => shape <= -2 ? -shape : throw new IncompatibleCoordinatesException();

        private void SetPosition(CoordinateSequence seq, int index, int count, int dimension, PrecisionModel precisionModel)
        {
            seq.SetX(index, precisionModel.MakePrecise(_ords[_ordCursor]));
            seq.SetY(index, precisionModel.MakePrecise(_ords[_ordCursor + 1]));
            if (dimension == 3)
            {
                seq.SetZ(index, count >= 3 ? _ords[_ordCursor + 2] : Coordinate.NullOrdinate);
            }

            _ordCursor += count;
        }

        private int AddShape()
        {
            if (_shapeCount == _shape.Length)
            {
                _shape = Grow(_shape);
            }

            _shape[_shapeCount] = 0;
            return _shapeCount++;
        }

        private void AddOrd(double value)
        {
            if (_ordCount == _ords.Length)
            {
                _ords = Grow(_ords);
            }

            _ords[_ordCount++] = value;
        }

        private static T[] Grow<T>(T[] array)
        {
            var bigger = ArrayPool<T>.Shared.Rent(array.Length * 2);
            Array.Copy(array, bigger, array.Length);
            ArrayPool<T>.Shared.Return(array);
            return bigger;
        }

        private sealed class IncompatibleCoordinatesException : Exception
        {
        }
    }
}
