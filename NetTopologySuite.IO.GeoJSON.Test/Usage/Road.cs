using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.GeoJSON.Test.Usage
{
    [JsonConverter(typeof(FeatureConverter))]
    public class Road : IFeature
    {
        private readonly AttributesProxy<Road> _attPrx;
        private Envelope _boundingBox;
        public bool BoundingBoxSet;

        public Road()
        {
            _attPrx = new AttributesProxy<Road>(this);
        }

        public Road(IFeature feature) : this()
        {
            Geometry = feature.Geometry;
            if (!(feature.BoundingBox ?? new Envelope()).IsNull)
                BoundingBox = feature.BoundingBox;

            _attPrx.SetValues(feature.Attributes);
        }

        public IAttributesTable Attributes
        {
            get => _attPrx;
            set => _attPrx.SetValues(value);
        }

        public IGeometry Geometry { get; set; }

        public Envelope BoundingBox
        {
            get => _boundingBox ?? (Geometry?.EnvelopeInternal ?? new Envelope());
            set
            {
                _boundingBox = value;
                BoundingBoxSet = _boundingBox != null;

            }
        }

        public double Length
        {
            get { return Geometry?.Length ?? 0d; }
        }

        public string Name { get; set; }

        public int NumLanes { get; set; }

        public bool OneWay { get; set; }

    }

    public class AttributesProxy<T> : IAttributesTable
    {
        private static readonly Dictionary<string, Tuple<Func<object, object>, Action<object, object>, Type>>
            ObjectAccess = new Dictionary<string, Tuple<Func<object, object>, Action<object, object>, Type>>();

        private readonly T _instance;

        static AttributesProxy()
        {
            AddFields(typeof(T));
            AddProperties(typeof(T));
        }

        private static void AddFields(Type t)
        {
            foreach (var fi in t.GetFields())
            {
                var get = new Func<object, object>(fi.GetValue);
                var set = new Action<object, object>(fi.SetValue);

                ObjectAccess[fi.Name] = Tuple.Create(get, set, fi.FieldType);
            }
            if (t.BaseType != typeof(object))
                AddFields(t.BaseType);
        }

        private static object GetNull(object item)
        {
            return null;
        }
        private static void SetNull(object item, object value)
        {
        }

        private static void AddProperties(Type t)
        {
            foreach (var pi in t.GetProperties())
            {
                if (pi.Name == "Geometry" || pi.Name == "BoundingBox" ||
                    pi.Name == "Attributes")
                    continue;
                var piTmp = pi;
                var get = piTmp.CanRead ? new Func<object, object>(piTmp.GetValue) : GetNull;
                var set = piTmp.CanWrite ?  new Action<object, object>(piTmp.SetValue) : SetNull;

                ObjectAccess[pi.Name] = Tuple.Create(get, set, pi.PropertyType);
            }
            if (t.BaseType != typeof(object))
                AddProperties(t.BaseType);
        }

        public AttributesProxy(T instance)
        {
            _instance = instance;
        }

        public void AddAttribute(string attributeName, object value)
        {
            this[attributeName] = value;
        }

        public void DeleteAttribute(string attributeName)
        {
            // Don't do anything, we have a business class
        }

        public Type GetType(string attributeName)
        {
            if (ObjectAccess.TryGetValue(attributeName, out var item))
                return item.Item3;
            throw new ArgumentOutOfRangeException();
        }

        public bool Exists(string attributeName)
        {
            return ObjectAccess.ContainsKey(attributeName);
        }

        public string[] GetNames()
        {
            return ObjectAccess.Keys.ToArray();
        }

        public object[] GetValues()
        {
            object[] res = new object[Count];
            int i = 0;
            foreach (var gs in ObjectAccess.Values)
                res[i++] = gs.Item1(_instance);
            return res;
        }

        public object this[string attributeName]
        {
            get
            {
                if (ObjectAccess.TryGetValue(attributeName, out var res))
                    return res.Item1(_instance);
                throw new ArgumentOutOfRangeException(nameof(attributeName));
            }
            set
            {
                if (ObjectAccess.TryGetValue(attributeName, out var res))
                {
                    value = Convert.ChangeType(value, res.Item3);
                    res.Item2(_instance, value);
                }
                else
                    throw new ArgumentOutOfRangeException(nameof(attributeName));
            }
        }

        public int Count
        {
            get { return ObjectAccess.Count; } 
        }

        public void SetValues(IAttributesTable value)
        {
            if (value == this)
                return;

            foreach (string name in value.GetNames())
                this[name] = value[name];
        }
    }
}
