using System.Diagnostics;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    [NtsIssueNumber(120)]
    [Category("GitHub Issue")]
    class Issue120
    {
        [Test(Description = "GitHub Issue #120")]
        public void Roundtrip_serialization_of_a_feature_with_null_properties_fails()
        {
            // Arrange
            var f = new Feature(new global::NetTopologySuite.Geometries.Point(1, 1), null);
            var s = GeoJsonSerializer.Create(new GeometryFactory());

            // Act
            var f1 = SandD(s, f);
            s.NullValueHandling = NullValueHandling.Include;
            var f2 = SandD(s, f);

            // Assert
            Assert.That(f1, Is.Not.Null, "f1 != null");
            Assert.That(f2, Is.Not.Null, "f2 != null");

        }

        private static IFeature SandD(JsonSerializer s, IFeature f)
        {
            var sb = new StringBuilder();
            var jtw = new JsonTextWriter(new StringWriter(sb));
            s.Serialize(jtw, f);
            var jsonText = sb.ToString();

            Debug.WriteLine(jsonText);

            var jtr = new JsonTextReader(new StringReader(jsonText));
            var res = s.Deserialize<IFeature>(jtr);
            return res;
        }
    }
}
