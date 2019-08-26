using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    [TestFixture]
    public abstract class AbstractIOFixture
    {
        protected readonly RandomGeometryHelper RandomGeometryHelper;

        protected AbstractIOFixture()
            : this(GeometryFactory.Default)
        {
        }

        protected AbstractIOFixture(GeometryFactory factory)
        {
            RandomGeometryHelper = new RandomGeometryHelper(factory);
        }

        private int _counter;

        public int Counter { get { return ++_counter; } }

        public int SRID
        {
            get
            {
                return RandomGeometryHelper.Factory.SRID;
            }
            protected set
            {
                var oldPM = new PrecisionModel();
                if (RandomGeometryHelper != null)
                    oldPM = RandomGeometryHelper.Factory.PrecisionModel;
                Debug.Assert(RandomGeometryHelper != null, "RandomGeometryHelper != null");
                if (RandomGeometryHelper.Factory is OgcCompliantGeometryFactory)
                    RandomGeometryHelper.Factory = new OgcCompliantGeometryFactory(oldPM, value);
                else
                    RandomGeometryHelper.Factory = new GeometryFactory(oldPM, value);
            }
        }

        public PrecisionModel PrecisionModel
        {
            get
            {
                return RandomGeometryHelper.Factory.PrecisionModel;
            }
            protected set
            {
                if (value == null)
                    return;

                if (value == PrecisionModel)
                    return;

                var factory = RandomGeometryHelper.Factory;
                int oldSrid = factory != null ? factory.SRID : 0;
                var oldFactory = factory != null
                                     ? factory.CoordinateSequenceFactory
                                     : CoordinateArraySequenceFactory.Instance;

                if (RandomGeometryHelper.Factory is OgcCompliantGeometryFactory)
                    RandomGeometryHelper.Factory = new OgcCompliantGeometryFactory(value, oldSrid, oldFactory);
                else
                    RandomGeometryHelper.Factory = new GeometryFactory(value, oldSrid, oldFactory);
            }
        }

        public double MinX
        {
            get { return RandomGeometryHelper.MinX; }
            protected set { RandomGeometryHelper.MinX = value; }
        }

        public double MaxX
        {
            get { return RandomGeometryHelper.MaxX; }
            protected set { RandomGeometryHelper.MaxX = value; }
        }

        public double MinY
        {
            get { return RandomGeometryHelper.MinY; }
            protected set { RandomGeometryHelper.MinY = value; }
        }

        public double MaxY
        {
            get { return RandomGeometryHelper.MaxY; }
            protected set { RandomGeometryHelper.MaxY = value; }
        }

        public Ordinates Ordinates
        {
            get { return RandomGeometryHelper.Ordinates; }
            set
            {
                Debug.Assert((value & Ordinates.XY) == Ordinates.XY);
                RandomGeometryHelper.Ordinates = value;
            }
        }

        public void PerformTest(Geometry gIn)
        {
            var writer = new WKTWriter(2) { MaxCoordinatesPerLine = 3, };
            byte[] b = null;
            Assert.DoesNotThrow(() => b = Write(gIn), "Threw exception during write:\n{0}", writer.WriteFormatted(gIn));

            Geometry gParsed = null;
            Assert.DoesNotThrow(() => gParsed = Read(b), "Threw exception during read:\n{0}", writer.WriteFormatted(gIn));

            Assert.IsNotNull(gParsed, "Could not be parsed\n{0}", gIn);
            CheckEquality(gIn, gParsed, writer);
        }

        protected virtual void CheckEquality(Geometry gIn, Geometry gParsed, WKTWriter writer)
        {
            Assert.IsTrue(gIn.EqualsExact(gParsed), "Instances are not equal\n{0}\n\n{1}", gIn, gParsed);
        }

        protected abstract Geometry Read(byte[] b);

        protected abstract byte[] Write(Geometry gIn);

        [Test]
        public virtual void TestPoint()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.Point);
        }
        [Test]
        public virtual void TestLineString()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.LineString);
        }
        [Test]
        public virtual void TestPolygon()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.Polygon);
        }
        [Test]
        public virtual void TestMultiPoint()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiPoint);
        }
        [Test]
        public virtual void TestMultiLineString()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiLineString);
        }

        [Test]
        public virtual void TestMultiPolygon()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiPolygon);
        }

        [Test]

        public virtual void TestGeometryCollection()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.GeometryCollection);
        }
    }
}
