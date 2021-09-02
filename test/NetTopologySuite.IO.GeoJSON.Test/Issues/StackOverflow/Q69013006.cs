using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.StackOverflow
{
    public class Q69013006
    {
        [NUnit.Framework.Test, NUnit.Framework.Description("https://stackoverflow.com/questions/69013006/serializing-featurecollection-geojson-in-net-core")]
        public void TestReadWithStjWriteWithNsj()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var file = asm.GetManifestResourceStream("NetTopologySuite.IO.GeoJSON.Test.Issues.StackOverflow.Q69013006.json");
            if (file == null)
                throw new NUnit.Framework.IgnoreException("Resource Q69013006.json not found");

            var opt = new System.Text.Json.JsonSerializerOptions
                { ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip };
            opt.Converters.Add(new GeoJsonConverterFactory());
            opt.PropertyNameCaseInsensitive = true;

            var data = new System.Span<byte>(new byte[file.Length]);
            file.Read(data);
            var fc = System.Text.Json.JsonSerializer.Deserialize<FeatureCollection>(data, opt);
            NUnit.Framework.Assert.That(fc, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(fc.Count, NUnit.Framework.Is.EqualTo(2));

            using var wrt = new StringWriter();

            var serializer = GeoJsonSerializer.CreateDefault();
            NUnit.Framework.Assert.That(() => serializer.Serialize(wrt, fc), NUnit.Framework.Throws.Nothing);

            string json = wrt.ToString();
            NUnit.Framework.Assert.That(json.StartsWith("{"), NUnit.Framework.Is.True);
            NUnit.Framework.Assert.That(json.EndsWith("}"), NUnit.Framework.Is.True);
            NUnit.Framework.Assert.That(json.Length, NUnit.Framework.Is.GreaterThan(2));
        }
    }
}
