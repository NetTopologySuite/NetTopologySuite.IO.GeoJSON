using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace NetTopologySuite.IO.GeoJSON4STJ.Benchmarks
{
    public static class Program
    {
        // compared against this checkout unless --baseline is given; "--baseline none" skips it
        private const string DefaultBaseline = "4.0.0";

        public static void Main(string[] args)
        {
            string baseline = DefaultBaseline;
            int i = Array.IndexOf(args, "--baseline");
            if (i >= 0 && i + 1 < args.Length)
            {
                baseline = args[i + 1];
                args = args.Take(i).Concat(args.Skip(i + 2)).ToArray();
            }

            var config = DefaultConfig.Instance.AddJob(Job.Default.WithId("this checkout"));
            if (baseline != "none")
            {
                config = config.AddJob(Job.Default
                    .WithArguments(new[] { new MsBuildArgument($"/p:GeoJsonStjVersion={baseline}") })
                    .WithId(baseline)
                    .AsBaseline());
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
