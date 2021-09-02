namespace NetTopologySuite.IO.GeoJSON4STJ.Test.Issues
{
    [GeoJsonIssueNumber(87)]
    public class Issue87
    {
        [NUnit.Framework.Test]
        public void Test()
        {
            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var file = asm.GetManifestResourceStream("NetTopologySuite.IO.GeoJSON4STJ.Test.Issues.Issue87.json");
            if (file == null)
                throw new NUnit.Framework.IgnoreException("Resource Issue87.json not found");

            var opt = new System.Text.Json.JsonSerializerOptions
                { ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip };
            opt.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
            opt.PropertyNameCaseInsensitive = true;

            var data = new System.Span<byte>(new byte[file.Length]);
            file.Read(data);
            var res = System.Text.Json.JsonSerializer.Deserialize<PartnerForCreationDto>(data, opt);

            NUnit.Framework.Assert.That(res, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(res.Pdvs, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(res.Pdvs.Count, NUnit.Framework.Is.GreaterThan(0));
        }
    }
    public class PartnerForCreationDto
    {
        public System.Collections.Generic.List<Pdvs> Pdvs { get; set; }
    }

    public class Pdvs
    {
        public string TradingName { get; set; }

        public string OwnerName { get; set; }

        public string Document { get; set; }

        //public MultiPolygonData CoverageArea { get; set; }
        public Geometries.Geometry CoverageArea { get; set; }
    }
}
