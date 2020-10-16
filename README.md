# GeoJSON

GeoJSON IO module for NTS. 

## GeoJSON4STJ Usage

This is the package for System.Text.Json serialization and deserialization.

### ASP.NET Core Example

Add the `System.Text.Json.Serializer.JsonConverterFactory`, `GeoJsonConverterFactory`, to the `JsonSerializerOptions` when you configure your controllers, MVC, etc in the `ConfigureServices` method of your `Startup.cs` class.

```csharp
public void ConfigureServices(IServiceCollection services) {
  services.AddControllers()
  .AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
  });
}
````

## GeoJSON Usage

**GeoJSON to `Geometry`**:

```c#
var geoJson = "{\"type\":\"Point\",\"coordinates\":[0.0,0.0]}";
Geometry geometry;

var serializer = GeoJsonSerializer.Create();
using (var stringReader = new StringReader(geoJson))
using (var jsonReader = new JsonTextReader(stringReader))
{
    geometry = serializer.Deserialize<Geometry>(jsonReader);
}
```

**`Geometry` to GeoJSON**:

```c#
var geometry = new Point(0, 0);
string geoJson;

var serializer = GeoJsonSerializer.Create();
using (var stringWriter = new StringWriter())
using (var jsonWriter = new JsonTextWriter(stringWriter))
{
    serializer.Serialize(jsonWriter, geometry);
    geoJson = stringWriter.ToString();
}
```

