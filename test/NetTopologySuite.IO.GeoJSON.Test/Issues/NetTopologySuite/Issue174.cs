using System;
using System.Reflection;
using NUnit.Framework;

namespace NetTopologySuite.IO.GeoJSON.Test.Issues.NetTopologySuite
{
    [NtsIssueNumber(174)]
    [Category("GitHub Issue")]
    class Issue174
    {
        [Test]
        public void ensure_NetTopologySuite_IO_GeoJSON_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(GeoJsonSerializer));
        }

        private void AssertStronglyNamedAssembly(Type typeFromAssemblyToCheck)
        {
            Assert.IsNotNull(typeFromAssemblyToCheck, "Cannot determine assembly from null");
            Assembly assembly = typeFromAssemblyToCheck.Assembly;
            StringAssert.DoesNotContain("PublicKeyToken=null", assembly.FullName, "Strongly named assembly should have a PublicKeyToken in fully qualified name");
        }
    }
}
