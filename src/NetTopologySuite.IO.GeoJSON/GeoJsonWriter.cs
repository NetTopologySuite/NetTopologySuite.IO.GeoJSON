using System;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Represents a GeoJSON Writer allowing for serialization of various GeoJSON elements 
    /// or any object containing GeoJSON elements.
    /// </summary>
    public class GeoJsonWriter
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public GeoJsonWriter()
        {
            SerializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        /// <summary>
        /// Gets or sets a value that is used to create and configure the underlying <see cref="GeoJsonSerializer"/>.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRfc7946RingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        /// <returns>A string representing the geometry's JSON representation</returns>
        public string Write(Geometry geometry, int dimension = 2, RingOrientationOptions enforceRfc7946RingOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (geometry is null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(geometry, writer, dimension, enforceRfc7946RingOrientation);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRfc7946RingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        public void Write(Geometry geometry, JsonWriter writer, int dimension = 2, RingOrientationOptions enforceRfc7946RingOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (geometry is null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var g = GeoJsonSerializer.Create(SerializerSettings, geometry.Factory, dimension, enforceRfc7946RingOrientation);
            g.Serialize(writer, geometry);
        }

        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRfc7946RingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        /// <returns>A string representing the feature's JSON representation</returns>
        public string Write(Feature feature, int dimension = 2, RingOrientationOptions enforceRfc7946RingOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(feature, writer, dimension, enforceRfc7946RingOrientation);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRfc7946RingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        public void Write(Feature feature, JsonWriter writer, int dimension = 2, RingOrientationOptions enforceRfc7946RingOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var factory = feature.Geometry?.Factory ?? GeoJsonSerializer.Wgs84Factory;
            var g = GeoJsonSerializer.Create(SerializerSettings, factory, dimension, enforceRfc7946RingOrientation);
            g.Serialize(writer, feature);
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRfc7946RingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        /// <returns>A string representing the feature collection's JSON representation</returns>
        public string Write(FeatureCollection featureCollection, int dimension = 2, RingOrientationOptions enforceRfc7946RingOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (featureCollection is null)
            {
                throw new ArgumentNullException(nameof(featureCollection));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(featureCollection, writer, dimension, enforceRfc7946RingOrientation);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRfc7946RingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        public void Write(FeatureCollection featureCollection, JsonWriter writer, int dimension = 2, RingOrientationOptions enforceRfc7946RingOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (featureCollection is null)
            {
                throw new ArgumentNullException(nameof(featureCollection));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var factory = SearchForFactory(featureCollection) ?? GeoJsonSerializer.Wgs84Factory;
            var g = GeoJsonSerializer.Create(SerializerSettings, factory, dimension, enforceRfc7946RingOrientation);
            g.Serialize(writer, featureCollection);
        }

        private static GeometryFactory SearchForFactory(FeatureCollection features)
        {
            GeometryFactory result = null;
            foreach (var feature in features)
            {
                result = feature?.Geometry?.Factory;
                if (!(result is null))
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Writes any specified object.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="enforceRfc7946RingOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        /// <returns>A string representing the object's JSON representation</returns>
        public string Write(object value, int dimension = 2, RingOrientationOptions enforceRfc7946RingOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(value, writer, dimension, enforceRfc7946RingOrientation);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes any specified object.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="dimension">
        /// A number of dimensions that are handled.  Must be 2 or 3.
        /// </param>
        /// <param name="ringOrientation">
        /// <see langword="true"/> to ensure that rings are oriented according to the GeoJSON rule,
        /// <see langword="false"/> to write out the coordinates in the order they are given.
        /// </param>
        public void Write(object value, JsonWriter writer, int dimension = 2,
            RingOrientationOptions ringOrientation = RingOrientationOptions.EnforceRfc9746)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var g = GeoJsonSerializer.Create(SerializerSettings, GeoJsonSerializer.Wgs84Factory, dimension, ringOrientation);
            g.Serialize(writer, value);
        }
    }
}
