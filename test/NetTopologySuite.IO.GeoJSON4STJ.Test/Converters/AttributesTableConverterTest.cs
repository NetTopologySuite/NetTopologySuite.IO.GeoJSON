using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Converters
{
    ///<summary>
    ///    This is a test class for AttributesTableConverterTest and is intended
    ///    to contain all AttributesTableConverterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class AttributesTableConverterTest : SandDTest<IAttributesTable>
    {
        ///<summary>
        ///A test for CanConvert
        ///</summary>
        [Test]
        public void CanConvertTest()
        {
            var options = DefaultOptions;
            var target = (JsonConverter<IAttributesTable>)GeoJsonConverterFactory.CreateConverter(typeof(IAttributesTable), options);
            var objectType = typeof(IAttributesTable);
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
            var sb = new StringBuilder();
            var atS = new AttributesTable();
            IAttributesTable atD = null;

            atS.Add("test1", "value1");
            atS.Add("test2", "value2");
            var options = DefaultOptions;
            using (var ms = new MemoryStream())
            {
                Serialize(ms, atS, options);
                Assert.AreEqual("{\"test1\":\"value1\",\"test2\":\"value2\"}", Encoding.UTF8.GetString(ms.ToArray()));
                atD = Deserialize(ms, options);
            }

            Assert.That(atD, Is.Not.Null);
            Assert.That(atD.Count, Is.EqualTo(atS.Count));
            Assert.That(atD.GetNames()[0], Is.EqualTo(atS.GetNames()[0]));
            Assert.That(atD.GetNames()[1], Is.EqualTo(atS.GetNames()[1]));
            Assert.That(atD[atD.GetNames()[0]], Is.EqualTo(atS[atS.GetNames()[0]]));
            Assert.That(atD[atD.GetNames()[1]], Is.EqualTo(atS[atS.GetNames()[1]]));
        }

        [Test]
        public void ReadJsonWithInnerObjectTest()
        {
            var atS = new AttributesTable();
            var inner = new AttributesTable();
            atS.Add("test1", "value1");
            atS.Add("test2", inner);
            inner.Add("innerTest1", "innerValue1");
            inner.Add("innerTest2", "innerValue2");

            var options = DefaultOptions;
            IAttributesTable atD = null;
            using (var ms = new MemoryStream())
            {
                Serialize(ms, atS, options);
                Console.WriteLine(Encoding.UTF8.GetString(ms.ToArray()));
                atD = Deserialize(ms, options);
            }

            Assert.That(atD, Is.Not.Null);

            // Check properties on attribute table
            Assert.That(atD, Is.InstanceOf<IAttributesTable>());
            Assert.That(atD.Count, Is.EqualTo(atS.Count));
            Assert.That(atD.GetNames()[0], Is.EqualTo(atS.GetNames()[0]));
            Assert.That(atD.GetNames()[1], Is.EqualTo(atS.GetNames()[1]));
            Assert.That(atD[atD.GetNames()[0]], Is.EqualTo(atS[atS.GetNames()[0]]));
            Assert.That(atD[atD.GetNames()[1]], Is.InstanceOf<IAttributesTable>());
            var atdInner = (IAttributesTable)atD[atD.GetNames()[1]];
            object atdInnerTest1 = atdInner["innerTest1"];
            Assert.That(atdInnerTest1, Is.InstanceOf<string>());
            Assert.That((string)atdInnerTest1, Is.EqualTo((string)inner["innerTest1"]));
            object atdInnerTest2 = atdInner["innerTest2"];
            Assert.That(atdInnerTest2, Is.InstanceOf<string>());
            Assert.That((string)atdInnerTest2, Is.EqualTo((string)inner["innerTest2"]));
        }

        [Test]
        public void ReadJsonWithArrayTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\": [{ \"innertest1\":\"innervalue1\" }]}}";
            var options = DefaultOptions;
            var converter = (JsonConverter<IAttributesTable>)GeoJsonConverterFactory.CreateConverter(typeof(IAttributesTable), options);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
            
            // read start object token and prepare the next token
            reader.Read();
            var result = converter.Read(ref reader, typeof(IAttributesTable), options);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["test1"]);
            Assert.IsNotNull(result["test2"]);
            Assert.IsInstanceOf<IList<object>>(result["test2"]);
            var list = (IList<object>)result["test2"];
            Assert.IsNotEmpty(list);
            Assert.AreEqual(1, list.Count);
            Assert.IsInstanceOf<IAttributesTable>(list[0]);
            var inner = (IAttributesTable) list[0];
            Assert.AreEqual(1, inner.Count);
            Assert.AreEqual("innervalue1", inner["innertest1"]);
        }

        [Test]
        public void ReadJsonWithArrayWithTwoObjectsTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\": [{ \"innertest1\":\"innervalue1\" }, { \"innertest2\":\"innervalue2\", \"innertest3\":\"innervalue3\"}]}}";
            var options = DefaultOptions;
            var converter = (JsonConverter<IAttributesTable>)GeoJsonConverterFactory.CreateConverter(typeof(IAttributesTable), options);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            // read start object token and prepare the next token
            reader.Read();
            var result = converter.Read(ref reader, typeof(IAttributesTable), options);

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
            var first = (IAttributesTable) list[0];
            Assert.AreEqual(1, first.Count);
            Assert.AreEqual("innervalue1", first["innertest1"]);
            var second = (IAttributesTable) list[1];
            Assert.AreEqual(2, second.Count);
            Assert.AreEqual("innervalue2", second["innertest2"]);
            Assert.AreEqual("innervalue3", second["innertest3"]);
        }

        [Test]
        public void ReadJsonWithArrayWithNestedArrayTest()
        {
            const string json = "{\"test1\":\"value1\",\"test2\": [{ \"innertest1\":\"innervalue1\" }, [{ \"innertest2\":\"innervalue2\", \"innertest3\":\"innervalue3\"}]]}}";
            var options = DefaultOptions;
            var converter = (JsonConverter<IAttributesTable>) GeoJsonConverterFactory.CreateConverter(typeof(IAttributesTable), options);
            var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));

            // read start object token and prepare the next token
            reader.Read();
            var result = converter.Read(ref reader, typeof(IAttributesTable), options);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["test1"]);
            Assert.IsNotNull(result["test2"]);
            Assert.IsInstanceOf<IList<object>>(result["test2"]);
            var list = (IList<object>)result["test2"];
            Assert.IsNotEmpty(list);
            Assert.AreEqual(2, list.Count);
            Assert.IsInstanceOf<IList<object>>(list[1]);
            Assert.IsInstanceOf<IAttributesTable>(list[0]);
            var first = (IAttributesTable) list[0];
            Assert.AreEqual(1, first.Count);
            Assert.IsTrue(first.Exists("innertest1"));
            Assert.AreEqual("innervalue1", first["innertest1"]);
            var innerList = (IList<object>) list[1];
            Assert.IsNotNull(innerList);
            Assert.IsNotEmpty(innerList);
            Assert.AreEqual(1, innerList.Count);
            Assert.IsInstanceOf<IAttributesTable>(innerList[0]);
            var inner = (IAttributesTable) innerList[0];
            Assert.AreEqual(2, inner.Count);
            Assert.IsTrue(inner.Exists("innertest2"));
            Assert.AreEqual("innervalue2", inner["innertest2"]);
            Assert.IsTrue(inner.Exists("innertest3"));
            Assert.AreEqual("innervalue3", inner["innertest3"]);
        }

        public static void TestEquality(IAttributesTable s, IAttributesTable d, string idPropertyName = "")
        {
            Assert.That(d, Is.Not.Null);
            //Assert.That(d.Count, Is.EqualTo(s.Count));
            var names = new List<string>(s.GetNames());
            names.Remove(idPropertyName);
            for (int i = 0; i < names.Count; i++)
            {
                object sitem = s[names[i]];
                if (sitem == null) continue;
                
                Assert.That(d.Exists(names[i]));
                object ditem = d[names[i]];

                if (sitem is IAttributesTable sAtItem)
                {
                    TestEquality(sAtItem, ditem as IAttributesTable);
                }
                else if (sitem is IEnumerable sEnumItem)
                {
                    var sIt = sEnumItem.GetEnumerator();
                    var dIt = ((IEnumerable) ditem).GetEnumerator();
                    while (sIt.MoveNext())
                    {
                        Assert.That(dIt.MoveNext());
                        object dtCurrent = dIt.Current;
                        if (dtCurrent is JsonElement dtJson)
                            dtCurrent = dtJson.GetString();
                        Assert.That(sIt.Current, Is.EqualTo(dtCurrent));
                    }
                    Assert.That(dIt.MoveNext(), Is.False);

                }
                else if (sitem is Guid sGuidItem)
                {
                    // we box ALL string values as strings, even those that can
                    // be converted to Guid, to simplify the number of cases
                    // callers need to be able to deal with.
                    Assert.That(ditem, Is.EqualTo(sGuidItem.ToString()));
                }
                else
                {
                    Assert.That(ditem, Is.EqualTo(sitem));
                }
            }
        }

        public static void TestEquality(IAttributesTable s, JsonElement? d)
        {
            Assert.That(d, Is.Not.Null);
            Assert.That(d.Value.ValueKind, Is.EqualTo(JsonValueKind.Object));
        }
    }
}
