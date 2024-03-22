﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    internal readonly struct StjParsedCoordinates
    {
        private static readonly GeoJsonObjectType[] SupportedTypesForPoint = { GeoJsonObjectType.Point };

        private static readonly GeoJsonObjectType[] SupportedTypesForCoordinateSequence = { GeoJsonObjectType.LineString, GeoJsonObjectType.MultiPoint };

        private static readonly GeoJsonObjectType[] SupportedTypesForCoordinateSequenceList = { GeoJsonObjectType.Polygon, GeoJsonObjectType.MultiLineString };

        private static readonly GeoJsonObjectType[] SupportedTypesForMultiPolygon = { GeoJsonObjectType.MultiPolygon };

        private static readonly GeoJsonObjectType[] SupportedTypesForEmpty = { GeoJsonObjectType.Point, GeoJsonObjectType.LineString, GeoJsonObjectType.Polygon, GeoJsonObjectType.MultiPoint, GeoJsonObjectType.MultiLineString, GeoJsonObjectType.MultiPolygon };

        private readonly object _obj;

        private StjParsedCoordinates(object obj)
            => _obj = obj;

        public static StjParsedCoordinates Parse(ref Utf8JsonReader reader, GeometryFactory factory)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            // all Parse* methods require the reader to be positioned on the first "Number" of the
            // innermost array, because that's the earliest point at which we know what kind of data
            // it stores.  they leave the reader's position on the last "EndArray" of the outermost
            // array containing their data, i.e., the "EndArray" that tells them when they are done.
            reader.AssertToken(JsonTokenType.StartArray);
            reader.ReadOrThrow();

            if (reader.TokenType == JsonTokenType.Number)
            {
                return new StjParsedCoordinates(ParsePoint(ref reader, factory));
            }

            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return default;
            }

            reader.AssertToken(JsonTokenType.StartArray);
            reader.ReadOrThrow();

            if (reader.TokenType == JsonTokenType.Number)
            {
                return new StjParsedCoordinates(ParseCoordinateSequence(ref reader, factory));
            }

            reader.AssertToken(JsonTokenType.StartArray);
            reader.ReadOrThrow();

            if (reader.TokenType == JsonTokenType.Number)
            {
                return new StjParsedCoordinates(ParseCoordinateSequenceList(ref reader, factory).ToArray());
            }

            reader.AssertToken(JsonTokenType.StartArray);
            reader.ReadOrThrow();
            reader.AssertToken(JsonTokenType.Number);

            return new StjParsedCoordinates(ParseMultiPolygon(ref reader, factory));
        }

        public ReadOnlySpan<GeoJsonObjectType> SupportedTypes
        {
            get
            {
                switch (_obj)
                {
                    case null:
                        return SupportedTypesForEmpty;

                    case Point _:
                        return SupportedTypesForPoint;

                    case CoordinateSequence _:
                        return SupportedTypesForCoordinateSequence;

                    case CoordinateSequence[] _:
                        return SupportedTypesForCoordinateSequenceList;

                    case MultiPolygon _:
                        return SupportedTypesForMultiPolygon;

                    default:
                        Debug.Fail("Need to update this if you add more types...");
                        throw new InvalidOperationException($"'{_obj}' (type: '{_obj?.GetType().ToString() ?? "(null)"}') is not recognized");
                }
            }
        }

        public Point ToPoint(GeometryFactory factory)
        {
            switch (_obj)
            {
                case null:
                    return factory.CreatePoint();

                case Point point:
                    return point;

                default:
                    throw new InvalidOperationException("Point is not supported (check SupportedTypes before calling any ToX methods)");
            }
        }

        public LineString ToLineString(GeometryFactory factory)
        {
            switch (_obj)
            {
                case null:
                    return factory.CreateLineString();

                case CoordinateSequence seq:
                    return factory.CreateLineString(seq);

                default:
                    throw new InvalidOperationException("LineString is not supported (check SupportedTypes before calling any ToX methods)");
            }
        }

        public MultiPoint ToMultiPoint(GeometryFactory factory)
        {
            switch (_obj)
            {
                case null:
                    return factory.CreateMultiPoint();

                case CoordinateSequence seq:
                    return factory.CreateMultiPoint(seq);

                default:
                    throw new InvalidOperationException("MultiPoint is not supported (check SupportedTypes before calling any ToX methods)");
            }
        }

        public Polygon ToPolygon(GeometryFactory factory)
        {
            switch (_obj)
            {
                case null:
                    return factory.CreatePolygon();

                case CoordinateSequence[] seqs:
                    return ToPolygon(seqs, factory);

                default:
                    throw new InvalidOperationException("Polygon is not supported (check SupportedTypes before calling any ToX methods)");
            }
        }

        public MultiLineString ToMultiLineString(GeometryFactory factory)
        {
            switch (_obj)
            {
                case null:
                    return factory.CreateMultiLineString();

                case CoordinateSequence[] seqs:
                    return factory.CreateMultiLineString(Array.ConvertAll(seqs, factory.CreateLineString));

                default:
                    throw new InvalidOperationException("MultiLineString is not supported (check SupportedTypes before calling any ToX methods)");
            }
        }

        public MultiPolygon ToMultiPolygon(GeometryFactory factory)
        {
            switch (_obj)
            {
                case null:
                    return factory.CreateMultiPolygon();

                case MultiPolygon multiPolygon:
                    return multiPolygon;

                default:
                    throw new InvalidOperationException("MultiPolygon is not supported (check SupportedTypes before calling any ToX methods)");
            }
        }

        private static Point ParsePoint(ref Utf8JsonReader reader, GeometryFactory factory)
        {
            var (x, y, zOrNull, mOrNull) = ReadXYZM(ref reader, factory.PrecisionModel);

            int dimension = mOrNull.HasValue ? 4 : zOrNull.HasValue ? 3 : 2;
            int measures = mOrNull.HasValue ? 1 : 0;
            var seq = factory.CoordinateSequenceFactory.Create(1, dimension, measures);
            seq.SetX(0, x);
            seq.SetY(0, y);
            if (zOrNull is double z)
            {
                seq.SetZ(0, z);
            }
            if (mOrNull is double m)
            {
                seq.SetM(0, m);
            }

            return factory.CreatePoint(seq);
        }

        private static CoordinateSequence ParseCoordinateSequence(ref Utf8JsonReader reader, GeometryFactory factory, List<double> ords = null)
        {
            ords = ords ?? new List<double>();
            bool sequenceHasZ = false;
            bool sequenceHasM = false;

            // read the first coordinate to kick things off...
            {
                var (x, y, zOrNull, mOrNull) = ReadXYZM(ref reader, factory.PrecisionModel);

                ords.Add(x);
                ords.Add(y);
                if (zOrNull is double z)
                {
                    ords.Add(z);
                    sequenceHasZ = true;
                }
                if (mOrNull is double m)
                {
                    ords.Add(m);
                    sequenceHasM = true;
                }

                Debug.Assert(reader.TokenType == JsonTokenType.EndArray, "ReadXYZM was supposed to leave us at the EndArray token just past the last ordinate value");
                reader.ReadOrThrow();
            }

            while (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.ReadOrThrow();

                reader.AssertToken(JsonTokenType.Number);
                var (x, y, zOrNull, mOrNull) = ReadXYZM(ref reader, factory.PrecisionModel);

                if (!sequenceHasZ && zOrNull.HasValue)
                {
                    // we've been reading XY up to this point, but we just saw an XYZ.  take a short
                    // one-time detour to weave dummy Z values into what we've already read so far,
                    // then continue reading the rest of the values.
                    ords = ConvertOrdsToNewDimension(ords, 3);
                    sequenceHasZ = true;
                }

                if (!sequenceHasM && mOrNull.HasValue)
                {
                    // we've been reading XYZ up to this point, but we just saw an XYZM.  take a short
                    // one-time detour to weave dummy M values into what we've already read so far,
                    // then continue reading the rest of the values.
                    ords = ConvertOrdsToNewDimension(ords, 4);
                    sequenceHasM = true;
                }

                ords.Add(x);
                ords.Add(y);
                if (sequenceHasZ)
                {
                    ords.Add(zOrNull ?? Coordinate.NullOrdinate);
                }
                if (sequenceHasM)
                {
                    ords.Add(mOrNull ?? Coordinate.NullOrdinate);
                }

                Debug.Assert(reader.TokenType == JsonTokenType.EndArray, "ReadXYZM was supposed to leave us at the EndArray token just past the last ordinate value");
                reader.ReadOrThrow();
            }

            reader.AssertToken(JsonTokenType.EndArray);

            int dimension = sequenceHasM ? 4 : sequenceHasZ ? 3 : 2;
            int measures = sequenceHasM ? 1 : 0;
            var seq = factory.CoordinateSequenceFactory.Create(ords.Count / dimension, dimension, measures);
            int ordIndex = 0;
            for (int coordIndex = 0; coordIndex < seq.Count; coordIndex++)
            {
                seq.SetX(coordIndex, ords[ordIndex++]);
                seq.SetY(coordIndex, ords[ordIndex++]);
                if (sequenceHasZ)
                {
                    seq.SetZ(coordIndex, ords[ordIndex++]);
                }
                if (sequenceHasM)
                {
                    seq.SetM(coordIndex, ords[ordIndex++]);
                }
            }

            return seq;
        }

        private static List<CoordinateSequence> ParseCoordinateSequenceList(ref Utf8JsonReader reader, GeometryFactory factory, List<CoordinateSequence> seqs = null, List<double> ords = null)
        {
            seqs = seqs ?? new List<CoordinateSequence>();
            ords = ords ?? new List<double>();

            seqs.Add(ParseCoordinateSequence(ref reader, factory, ords));

            reader.ReadOrThrow();
            while (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.ReadOrThrow();
                reader.AssertToken(JsonTokenType.StartArray);
                reader.ReadOrThrow();
                reader.AssertToken(JsonTokenType.Number);

                ords.Clear();
                seqs.Add(ParseCoordinateSequence(ref reader, factory, ords));

                Debug.Assert(reader.TokenType == JsonTokenType.EndArray, "ParseCoordinateSequence was supposed to leave us at the EndArray token just past the last coordinate of the sequence.");
                reader.ReadOrThrow();
            }

            reader.AssertToken(JsonTokenType.EndArray);
            return seqs;
        }

        private static MultiPolygon ParseMultiPolygon(ref Utf8JsonReader reader, GeometryFactory factory)
        {
            var polygons = new List<Polygon>();
            var seqs = new List<CoordinateSequence>();
            var ords = new List<double>();

            polygons.Add(ToPolygon(ParseCoordinateSequenceList(ref reader, factory, seqs, ords), factory));

            reader.ReadOrThrow();
            while (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.ReadOrThrow();
                reader.AssertToken(JsonTokenType.StartArray);
                reader.ReadOrThrow();
                reader.AssertToken(JsonTokenType.StartArray);
                reader.ReadOrThrow();
                reader.AssertToken(JsonTokenType.Number);

                seqs.Clear();
                ords.Clear();
                polygons.Add(ToPolygon(ParseCoordinateSequenceList(ref reader, factory, seqs, ords), factory));

                Debug.Assert(reader.TokenType == JsonTokenType.EndArray, "ParseCoordinateSequenceList was supposed to leave us at the EndArray token just past the last ring of the polygon.");
                reader.ReadOrThrow();
            }

            reader.AssertToken(JsonTokenType.EndArray);
            return factory.CreateMultiPolygon(polygons.ToArray());
        }

        private static (double x, double y, double? zOrNull, double? mOrNull) ReadXYZM(ref Utf8JsonReader reader, PrecisionModel precisionModel)
        {
            Debug.Assert(reader.TokenType == JsonTokenType.Number, "ReadXYZM was supposed to be called with a reader positioned on the first Number token of the array.");

            // x
            double x = precisionModel.MakePrecise(reader.GetDouble());

            // y
            reader.ReadOrThrow();
            reader.AssertToken(JsonTokenType.Number);
            double y = precisionModel.MakePrecise(reader.GetDouble());

            double? z = null;
            double? m = null;

            // z?
            reader.ReadOrThrow();
            if (reader.TokenType == JsonTokenType.Number)
            {
                // yes z
                z = reader.GetDouble();
                reader.ReadOrThrow();
            }

            // m?
            if (reader.TokenType == JsonTokenType.Number)
            {
                // yes m
                m = reader.GetDouble();
                reader.ReadOrThrow();
            }

            AdvanceReaderToEndOfCurrentNumberArray(ref reader);
            return (x, y, z, m);
        }

        private static void AdvanceReaderToEndOfCurrentNumberArray(ref Utf8JsonReader reader)
        {
            while (reader.TokenType == JsonTokenType.Number)
            {
                reader.ReadOrThrow();
            }

            reader.AssertToken(JsonTokenType.EndArray);
        }

        private static List<double> ConvertOrdsToNewDimension(List<double> ords, int dimension)
        {
            Debug.Assert(ords.Count % dimension - 1 == 0, $"This was only supposed to be called with {dimension - 1}-dimensional values.");

            var newOrds = new List<double>(ords.Capacity);
            for (int i = 0; i < dimension - 1; i++)
            {
                newOrds.Add( ords[i] );
            }
            newOrds.Add(Coordinate.NullOrdinate);

            return newOrds;
        }

        private static Polygon ToPolygon(IReadOnlyList<CoordinateSequence> ringSequences, GeometryFactory factory)
        {
            var shell = factory.CreateLinearRing(ringSequences[0]);
            if (ringSequences.Count == 1)
            {
                return factory.CreatePolygon(shell);
            }

            var holes = new LinearRing[ringSequences.Count - 1];
            for (int i = 0; i < holes.Length; i++)
            {
                holes[i] = factory.CreateLinearRing(ringSequences[i + 1]);
            }

            return factory.CreatePolygon(shell, holes);
        }
    }
}
