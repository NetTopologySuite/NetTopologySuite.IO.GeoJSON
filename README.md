# GeoJSON

GeoJSON IO module for NTS. 

## Usage

### ASP.NET Core

Add the `System.Text.Json.Serializer.JsonConverterFactory`, `GeoJsonConverterFactory`, to the `JsonSerializerOptions` when you configure your controllers, MVC, etc in the `ConfigureServices` method of your `Startup.cs` class.

#### example

```csharp
public void ConfigureServices(IServiceCollection services) {
  services.AddControllers()
  .AddJsonOptions(options => {
    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
  });
}
````
