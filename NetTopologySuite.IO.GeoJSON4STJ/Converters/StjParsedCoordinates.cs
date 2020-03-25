using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    internal readonly struct StjParsedCoordinates
    {
        private static readonly GeoJsonObjectType[] SupportedTypesForPoint = { GeoJsonObjectType.Point };

        private static readonly GeoJsonObjectType[] SupportedTypesForCoordinateSequence = { GeoJsonObjectType.LineString, GeoJsonObjectType.MultiPoint };

        private static readonly GeoJsonObjectType[] SupportedTypesForCoordinateSequenceList = { GeoJsonObjectType.Polygon, GeoJsonObjectType.MultiLineString };

        private static readonly GeoJsonObjectType[] SupportedTypesForMultiPolygon = { GeoJsonObjectType.MultiPolygon };

        private readonly object _obj;

        private StjParsedCoordinates(object obj)
            => _obj = obj;

        public static StjParsedCoordinates Parse(ref Utf8JsonReader reader, IGeometryFactory factory)
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
                        return default;

                    case Point _:
                        return SupportedTypesForPoint;

                    case ICoordinateSequence _:
                        return SupportedTypesForCoordinateSequence;

                    case ICoordinateSequence[] _:
                        return SupportedTypesForCoordinateSequenceList;

                    case MultiPolygon _:
                        return SupportedTypesForMultiPolygon;

                    default:
                        Debug.Fail("Need to update this if you add more types...");
                        throw new InvalidOperationException($"'{_obj}' (type: '{_obj?.GetType().ToString() ?? "(null)"}') is not recognized");
                }
            }
        }

        public IPoint ToPoint()
        {
            return _obj as IPoint ??
                   throw new InvalidOperationException("Point is not supported (check SupportedTypes before calling any ToX methods)");
        }

        public ILineString ToLineString(IGeometryFactory factory)
        {
            return _obj is ICoordinateSequence seq
                ? factory.CreateLineString(seq)
                : throw new InvalidOperationException("LineString is not supported (check SupportedTypes before calling any ToX methods)");
        }

        public IMultiPoint ToMultiPoint(IGeometryFactory factory)
        {
            return _obj is ICoordinateSequence seq
                ? factory.CreateMultiPoint(seq)
                : throw new InvalidOperationException("MultiPoint is not supported (check SupportedTypes before calling any ToX methods)");
        }

        public IPolygon ToPolygon(IGeometryFactory factory)
        {
            return _obj is ICoordinateSequence[] seqs
                ? ToPolygon(seqs, factory)
                : throw new InvalidOperationException("Polygon is not supported (check SupportedTypes before calling any ToX methods)");
        }

        public IMultiLineString ToMultiLineString(IGeometryFactory factory)
        {
            return _obj is ICoordinateSequence[] seqs
                ? factory.CreateMultiLineString(Array.ConvertAll(seqs, factory.CreateLineString))
                : throw new InvalidOperationException("MultiLineString is not supported (check SupportedTypes before calling any ToX methods)");
        }

        public MultiPolygon ToMultiPolygon()
        {
            return _obj is MultiPolygon multiPolygon
                ? multiPolygon
                : throw new InvalidOperationException("MultiPolygon is not supported (check SupportedTypes before calling any ToX methods)");
        }

        private static IPoint ParsePoint(ref Utf8JsonReader reader, IGeometryFactory factory)
        {
            var (x, y, zOrNull) = ReadXYZ(ref reader, factory.PrecisionModel);

            var seq = factory.CoordinateSequenceFactory.Create(1, zOrNull.HasValue ? 3 : 2);
            seq.SetOrdinate(0, Ordinate.X, x);
            seq.SetOrdinate(0, Ordinate.Y, y);
            if (zOrNull is double z)
            {
                seq.SetOrdinate(0, Ordinate.Z, z);
            }

            return factory.CreatePoint(seq);
        }

        private static ICoordinateSequence ParseCoordinateSequence(ref Utf8JsonReader reader, IGeometryFactory factory, List<double> ords = null)
        {
            ords = ords ?? new List<double>();
            bool sequenceHasZ = false;

            // read the first coordinate to kick things off...
            {
                var (x, y, zOrNull) = ReadXYZ(ref reader, factory.PrecisionModel);

                ords.Add(x);
                ords.Add(y);
                if (zOrNull is double z)
                {
                    ords.Add(z);
                    sequenceHasZ = true;
                }

                Debug.Assert(reader.TokenType == JsonTokenType.EndArray, "ReadXYZ was supposed to leave us at the EndArray token just past the last ordinate value");
                reader.ReadOrThrow();
            }

            while (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.ReadOrThrow();

                reader.AssertToken(JsonTokenType.Number);
                var (x, y, zOrNull) = ReadXYZ(ref reader, factory.PrecisionModel);

                if (!sequenceHasZ && zOrNull.HasValue)
                {
                    // we've been reading XY up to this point, but we just saw an XYZ.  take a short
                    // one-time detour to weave dummy Z values into what we've already read so far,
                    // then continue reading the rest of the values.
                    ords = ConvertXYToXYZ(ords);
                    sequenceHasZ = true;
                }

                ords.Add(x);
                ords.Add(y);
                if (sequenceHasZ)
                {
                    ords.Add(zOrNull ?? Coordinate.NullOrdinate);
                }

                Debug.Assert(reader.TokenType == JsonTokenType.EndArray, "ReadXYZ was supposed to leave us at the EndArray token just past the last ordinate value");
                reader.ReadOrThrow();
            }

            reader.AssertToken(JsonTokenType.EndArray);

            int dimension = sequenceHasZ ? 3 : 2;
            var seq = factory.CoordinateSequenceFactory.Create(ords.Count / dimension, dimension);
            int ordIndex = 0;
            for (int coordIndex = 0; coordIndex < seq.Count; coordIndex++)
            {
                seq.SetOrdinate(coordIndex, Ordinate.X, ords[ordIndex++]);
                seq.SetOrdinate(coordIndex, Ordinate.Y, ords[ordIndex++]);
                if (sequenceHasZ)
                {
                    seq.SetOrdinate(coordIndex, Ordinate.Z, ords[ordIndex++]);
                }
            }

            return seq;
        }

        private static List<ICoordinateSequence> ParseCoordinateSequenceList(ref Utf8JsonReader reader, IGeometryFactory factory, List<ICoordinateSequence> seqs = null, List<double> ords = null)
        {
            seqs = seqs ?? new List<ICoordinateSequence>();
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

        private static IMultiPolygon ParseMultiPolygon(ref Utf8JsonReader reader, IGeometryFactory factory)
        {
            var polygons = new List<IPolygon>();
            var seqs = new List<ICoordinateSequence>();
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

        private static (double x, double y, double? zOrNull) ReadXYZ(ref Utf8JsonReader reader, IPrecisionModel precisionModel)
        {
            Debug.Assert(reader.TokenType == JsonTokenType.Number, "ReadXYZ was supposed to be called with a reader positioned on the first Number token of the array.");

            // x
            double x = precisionModel.MakePrecise(reader.GetDouble());

            // y
            reader.ReadOrThrow();
            reader.AssertToken(JsonTokenType.Number);
            double y = precisionModel.MakePrecise(reader.GetDouble());

            // z?
            reader.ReadOrThrow();
            if (reader.TokenType == JsonTokenType.Number)
            {
                // yes z
                double z = reader.GetDouble();
                AdvanceReaderToEndOfCurrentNumberArray(ref reader);
                return (x, y, z);
            }
            else
            {
                // no z
                reader.AssertToken(JsonTokenType.EndArray);
                return (x, y, null);
            }
        }

        private static void AdvanceReaderToEndOfCurrentNumberArray(ref Utf8JsonReader reader)
        {
            while (reader.TokenType == JsonTokenType.Number)
            {
                reader.ReadOrThrow();
            }

            reader.AssertToken(JsonTokenType.EndArray);
        }

        private static List<double> ConvertXYToXYZ(List<double> xys)
        {
            Debug.Assert(xys.Count % 2 == 0, "This was only supposed to be called with XY values.");

            var xyzs = new List<double>(xys.Capacity);
            int i = 0;
            while (i < xys.Count)
            {
                xyzs.Add(xys[i++]);
                xyzs.Add(xys[i++]);
                xyzs.Add(Coordinate.NullOrdinate);
            }

            return xyzs;
        }

        private static IPolygon ToPolygon(IReadOnlyList<ICoordinateSequence> ringSequences, IGeometryFactory factory)
        {
            var shell = factory.CreateLinearRing(ringSequences[0]);
            if (ringSequences.Count == 1)
            {
                return factory.CreatePolygon(shell);
            }

            var holes = new ILinearRing[ringSequences.Count - 1];
            for (int i = 0; i < holes.Length; i++)
            {
                holes[i] = factory.CreateLinearRing(ringSequences[i + 1]);
            }

            return factory.CreatePolygon(shell, holes);
        }
    }
}
