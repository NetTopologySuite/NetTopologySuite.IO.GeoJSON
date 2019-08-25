using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    ///<summary>
    ///    This is a test class for AttributesTableConverterTest and is intended
    ///    to contain all AttributesTableConverterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class AttributesTableConverterTest
    {
        ///<summary>
        ///    A test for CanConvert
        ///</summary>
        [Test]
        public void CanConvertTest()
        {
            var target = new AttributesTableConverter();
            var objectType = typeof(AttributesTable);
            const bool expected = true;
            bool actual = target.CanConvert(objectType);
            Assert.AreEqual(expected, actual);
        }

        ///<summary>
        ///    A test for WriteJson
        ///</summary>
        [Test]
        public void WriteJsonTest()
        {
            var target = new AttributesTableConverter();
            var sb = new StringBuilder();
            var writer = new JsonTextWriter(new StringWriter(sb));
            var value = new AttributesTable();
            value.Add("test1", "value1");
            value.Add("test2", "value2");
            var serializer = new JsonSerializer();
            target.WriteJson(writer, value, serializer);
            writer.Flush();
            Assert.AreEqual("{\"test1\":\"value1\",\"test2\":\"value2\"}", sb.ToString());
        }

        ///<summary>
        ///    A test for ReadJson
        ///</summary>
        [Test]
        public void ReadJsonTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\":\"value2\"}}";
            var target = new AttributesTableConverter();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                var serializer = new JsonSerializer();

                // read start object token and prepare the next token
                reader.Read();
                var result =
                    (AttributesTable)
                    target.ReadJson(reader, typeof(AttributesTable), new AttributesTable(), serializer);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("value1", result["test1"]);
                Assert.AreEqual("value2", result["test2"]);
            }
        }

        [Test]
        public void ReadJsonWithInnerObjectTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\": { \"innertest1\":\"innervalue1\" }}}";
            var target = new AttributesTableConverter();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                var serializer = new JsonSerializer();

                // read start object token and prepare the next token
                reader.Read();
                var result =
                    (AttributesTable)
                    target.ReadJson(reader, typeof(AttributesTable), new AttributesTable(), serializer);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("value1", result["test1"]);
                Assert.IsNotNull(result["test2"]);
                Assert.IsInstanceOf<IAttributesTable>(result["test2"]);
                var inner = (IAttributesTable)result["test2"];
                Assert.AreEqual(1, inner.Count);
                Assert.AreEqual("innervalue1", inner["innertest1"]);
            }
        }

        [Test]
        public void ReadJsonWithArrayTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\": [{ \"innertest1\":\"innervalue1\" }]}}";
            var target = new AttributesTableConverter();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                var serializer = new JsonSerializer();

                // read start object token and prepare the next token
                reader.Read();
                var result =
                    (AttributesTable)
                    target.ReadJson(reader, typeof(AttributesTable), new AttributesTable(), serializer);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("value1", result["test1"]);
                Assert.IsNotNull(result["test2"]);
                Assert.IsInstanceOf<IList<object>>(result["test2"]);
                var list = (IList<object>)result["test2"];
                Assert.IsNotEmpty(list);
                Assert.AreEqual(1, list.Count);
                Assert.IsInstanceOf<IAttributesTable>(list[0]);
                var inner = (IAttributesTable)list[0];
                Assert.AreEqual(1, inner.Count);
                Assert.AreEqual("innervalue1", inner["innertest1"]);
            }
        }

        [Test]
        public void ReadJsonWithArrayWithTwoObjectsTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\": [{ \"innertest1\":\"innervalue1\" }, { \"innertest2\":\"innervalue2\", \"innertest3\":\"innervalue3\"}]}}";
            var target = new AttributesTableConverter();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                var serializer = new JsonSerializer();

                // read start object token and prepare the next token
                reader.Read();
                var result =
                    (AttributesTable)
                    target.ReadJson(reader, typeof(AttributesTable), new AttributesTable(), serializer);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("value1", result["test1"]);
                Assert.IsNotNull(result["test2"]);
                Assert.IsInstanceOf<IList<object>>(result["test2"]);
                var list = (IList<object>)result["test2"];
                Assert.IsNotEmpty(list);
                Assert.AreEqual(2, list.Count);
                Assert.IsInstanceOf<IAttributesTable>(list[0]);
                Assert.IsInstanceOf<IAttributesTable>(list[1]);
                var first = (IAttributesTable)list[0];
                Assert.AreEqual(1, first.Count);
                Assert.AreEqual("innervalue1", first["innertest1"]);
                var second = (IAttributesTable)list[1];
                Assert.AreEqual(2, second.Count);
                Assert.AreEqual("innervalue2", second["innertest2"]);
                Assert.AreEqual("innervalue3", second["innertest3"]);
            }
        }

        [Test]
        public void ReadJsonWithArrayWithNestedArrayTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\": [{ \"innertest1\":\"innervalue1\" }, [{ \"innertest2\":\"innervalue2\", \"innertest3\":\"innervalue3\"}]]}}";
            var target = new AttributesTableConverter();
            using (var reader = new JsonTextReader(new StringReader(json)))
            {
                var serializer = new JsonSerializer();

                // read start object token and prepare the next token
                reader.Read();
                var result =
                    (AttributesTable)
                        target.ReadJson(reader, typeof(AttributesTable), new AttributesTable(), serializer);
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.AreEqual("value1", result["test1"]);
                Assert.IsNotNull(result["test2"]);
                Assert.IsInstanceOf<IList<object>>(result["test2"]);
                var list = (IList<object>)result["test2"];
                Assert.IsNotEmpty(list);
                Assert.AreEqual(2, list.Count);
                Assert.IsInstanceOf<IAttributesTable>(list[0]);
                Assert.IsInstanceOf<IList<object>>(list[1]);
                var first = (IAttributesTable)list[0];
                Assert.AreEqual(1, first.Count);
                Assert.IsTrue(first.Exists("innertest1"));
                Assert.AreEqual("innervalue1", first["innertest1"]);
                var innerList = (IList<object>)list[1];
                Assert.IsNotNull(innerList);
                Assert.IsNotEmpty(innerList);
                Assert.AreEqual(1, innerList.Count);
                Assert.IsInstanceOf<IAttributesTable>(innerList[0]);
                var inner = (IAttributesTable)innerList[0];
                Assert.AreEqual(2, inner.Count);
                Assert.IsTrue(inner.Exists("innertest2"));
                Assert.AreEqual("innervalue2", inner["innertest2"]);
                Assert.IsTrue(inner.Exists("innertest3"));
                Assert.AreEqual("innervalue3", inner["innertest3"]);
            }
        }
    }
}
