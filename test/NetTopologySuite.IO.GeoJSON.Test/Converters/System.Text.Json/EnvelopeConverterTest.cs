using System;
using System.IO;
using System.Text.Json;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Converters.System.Text.Json
{
    public class EnvelopeConverterTest : SandDTest<Envelope>
    {
        [Test, Obsolete]
        public void TestCanConvert()
        {
            var c = new EnvelopeConverter();
            Assert.That(c.CanConvert(typeof(Envelope)), Is.True);
            Assert.That(c.CanConvert(typeof(object)), Is.False);
        }

        [Test, Obsolete]
        public void TestWriteRead1234()
        {
            var c = new StjEnvelopeConverter();
            var envS = new Envelope(1, 2, 3, 4);


            var ms = new MemoryStream();
            Serialize(c, ms, envS, new JsonSerializerOptions(), false);
            var envD = Deserialize(c, ms, new JsonSerializerOptions(), false);
            //var rdr = new Utf8JsonReader(new ReadOnlySpan<byte>(ms.ToArray()));
            //// nothing read
            //rdr.Read();
            //// consume start object
            //rdr.Read();
            //var envD = c.Read(ref rdr, typeof(Envelope), new JsonSerializerOptions());
            //Assert.That(rdr.TokenType == JsonTokenType.EndObject);
            //// Consume end object
            //rdr.Read();
            //Assert.That(rdr.BytesConsumed, Is.EqualTo(ms.Length));

            Assert.That(envD != null);
            Assert.That(envD.Equals(envS));
        }

        [Test, Obsolete]
        public void TestWriteReadNull()
        {
            var c = new StjEnvelopeConverter();

            var ms = new MemoryStream();
            Serialize(c, ms, null, new JsonSerializerOptions(), false);
            var envD = Deserialize(c, ms, new JsonSerializerOptions(), false);
            //var rdr = new Utf8JsonReader(new ReadOnlySpan<byte>(ms.ToArray()));
            //// nothing read
            //rdr.Read();
            //// consume start object
            //rdr.Read();
            //var envD = c.Read(ref rdr, typeof(Envelope), new JsonSerializerOptions());
            //Assert.That(rdr.TokenType == JsonTokenType.EndObject);
            //// Consume end object
            //rdr.Read();
            //Assert.That(rdr.BytesConsumed, Is.EqualTo(ms.Length));

            Assert.That(envD == null);
        }
    }
}
