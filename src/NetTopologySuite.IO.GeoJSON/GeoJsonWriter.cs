﻿using System;
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
        private int _dimension;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public GeoJsonWriter()
        {
            SerializerSettings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            };
            Dimension = GeoJsonSerializer.Dimension;
            RingOrientationOption = GeoJsonSerializer.RingOrientationOption;
        }

        /// <summary>
        /// Gets or sets a value that is used to create and configure the underlying <see cref="GeoJsonSerializer"/>.
        /// </summary>
        public JsonSerializerSettings SerializerSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the polygon ring orientation must adhere to
        /// rule defined in RFC7964 §3.1.6
        /// </summary>
        /// <a href="https://www.rfc-editor.org/rfc/rfc7946#section-3.1.6">Polygon</a>
        public RingOrientationOption RingOrientationOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the number of dimensions the serializer should handle
        /// </summary>
        /// <remarks>Value must be 2 or 3.</remarks>
        public int Dimension
        {
            get => _dimension;
            set
            {
                if (value < Dimension || 3 < value)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Must be Dimension or 3.");
                _dimension = value;
            }
        }
        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <returns>A string representing the geometry's JSON representation</returns>
        public string Write(Geometry geometry)
        {
            if (geometry is null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(geometry, writer);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified geometry.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        public void Write(Geometry geometry, JsonWriter writer)
        {
            if (geometry is null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var g = GeoJsonSerializer.Create(SerializerSettings, geometry.Factory, Dimension, RingOrientationOption);
            g.Serialize(writer, geometry);
        }

        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns>A string representing the feature's JSON representation</returns>
        public string Write(Feature feature)
        {
            if (feature is null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(feature, writer);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature.
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        public void Write(Feature feature, JsonWriter writer)
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
            var g = GeoJsonSerializer.Create(SerializerSettings, factory, Dimension, RingOrientationOption);
            g.Serialize(writer, feature);
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        /// <returns>A string representing the feature collection's JSON representation</returns>
        public string Write(FeatureCollection featureCollection)
        {
            if (featureCollection is null)
            {
                throw new ArgumentNullException(nameof(featureCollection));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(featureCollection, writer);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes the specified feature collection.
        /// </summary>
        /// <param name="featureCollection">The feature collection.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        public void Write(FeatureCollection featureCollection, JsonWriter writer)
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
            var g = GeoJsonSerializer.Create(SerializerSettings, factory, Dimension, RingOrientationOption);
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
        /// <returns>A string representing the object's JSON representation</returns>
        public string Write(object value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var sb = new StringBuilder();
            using (var writer = new JsonTextWriter(new StringWriter(sb)))
            {
                Write(value, writer);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes any specified object.
        /// </summary>
        /// <param name="value">Any object.</param>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        public void Write(object value, JsonWriter writer)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var g = GeoJsonSerializer.Create(SerializerSettings, GeoJsonSerializer.Wgs84Factory, Dimension, RingOrientationOption);
            g.Serialize(writer, value);
        }
    }
}
