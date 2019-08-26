using System;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test
{
    /// <summary>
    /// The issue number used in this test (or fixture) refers to an issue on
    /// https://github.com/NetTopologySuite/NetTopologySuite.IO.GeoJSON, created
    /// after this project was split out on its own (and thus, it got its own
    /// set of issue numbers).
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class GeoJsonIssueNumberAttribute : PropertyAttribute
    {
        public GeoJsonIssueNumberAttribute(int issueNumber)
            : base("NetTopologySuite.IO.GeoJSON issue", issueNumber)
        {
        }
    }
}
