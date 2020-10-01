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
