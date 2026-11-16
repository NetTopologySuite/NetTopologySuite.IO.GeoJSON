# Benchmarks

Read benchmarks for the System.Text.Json converters (`NetTopologySuite.IO.GeoJSON4STJ`).

```sh
dotnet run -c Release --project benchmarks/NetTopologySuite.IO.GeoJSON4STJ.Benchmarks -- --filter '*'
```

Each benchmark runs twice: against this checkout, and against the released `NetTopologySuite.IO.GeoJSON4STJ`
package (4.0.0 by default) as the baseline, so the summary shows the ratio between the two.

```sh
... -- --filter '*' --baseline 3.0.0        # compare against another released version
... -- --filter '*' --baseline none         # this checkout only
... -- --filter '*' --iterationCount 15     # other BenchmarkDotNet options apply to both runs
```

Reports are written to `BenchmarkDotNet.Artifacts/`.
