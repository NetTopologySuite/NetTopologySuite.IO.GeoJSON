# GeoJSON
GeoJSON IO module for NTS. 

# Usage

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
