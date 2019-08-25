using System;
using System.Collections.Generic;

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
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

        public Road(IFeature feature)
            : this()
        {
            Geometry = feature.Geometry;
            if (feature.BoundingBox?.IsNull == false)
            {
                BoundingBox = feature.BoundingBox;
            }

            _attPrx.SetValues(feature.Attributes);
        }

        public IAttributesTable Attributes
        {
            get => _attPrx;
            set => _attPrx.SetValues(value);
        }

        public Geometry Geometry { get; set; }

        public Envelope BoundingBox
        {
            get => _boundingBox ?? Geometry?.EnvelopeInternal ?? new Envelope();
            set
            {
                _boundingBox = value;
                BoundingBoxSet = _boundingBox != null;
            }
        }

        public double Length => Geometry?.Length ?? 0;

        public string Name { get; set; }

        public int NumLanes { get; set; }

        public bool OneWay { get; set; }
    }

    public class AttributesProxy<T> : IAttributesTable
    {
        private static readonly Dictionary<string, (Func<T, object> getter, Action<T, object> setter, Type memberType)>
            ObjectAccess = new Dictionary<string, (Func<T, object> getter, Action<T, object> setter, Type memberType)>();

        private static readonly string[] MemberNames;

        private static readonly Func<T, object>[] ValueAccessors;

        private readonly T _instance;

        static AttributesProxy()
        {
            for (var t = typeof(T); t != typeof(object); t = t.BaseType)
            {
                foreach (var fi in t.GetFields())
                {
                    var get = new Func<T, object>(x => fi.GetValue(x));
                    var set = new Action<T, object>((x, val) => fi.SetValue(x, val));

                    if (!ObjectAccess.TryAdd(fi.Name, (get, set, fi.FieldType)))
                    {
                        throw new NotSupportedException($"name '{fi.Name}' is shadowed on type '{typeof(T)}' (duplicate was a field on type {t})");
                    }
                }

                foreach (var pi in t.GetProperties())
                {
                    // ignore the properties that are actually just there to implement IFeature.
                    switch (pi.Name)
                    {
                        case nameof(IFeature.Geometry):
                        case nameof(IFeature.BoundingBox):
                        case nameof(IFeature.Attributes):
                            continue;
                    }

                    var get = pi.CanRead ? new Func<T, object>(x => pi.GetValue(x)) : null;
                    var set = pi.CanWrite ? new Action<T, object>((x, val) => pi.SetValue(x, val)) : null;

                    if (!ObjectAccess.TryAdd(pi.Name, (get, set, pi.PropertyType)))
                    {
                        throw new NotSupportedException($"name '{pi.Name}' is shadowed on type '{typeof(T)}' (duplicate was a property on type {t})");
                    }
                }
            }

            // cache these for GetNames() and GetValues()
            MemberNames = new string[ObjectAccess.Count];
            ValueAccessors = new Func<T, object>[ObjectAccess.Count];
            int i = 0;
            foreach (var (key, (getter, _, _)) in ObjectAccess)
            {
                MemberNames[i] = key;
                ValueAccessors[i] = getter;

                ++i;
            }
        }

        public AttributesProxy(T instance)
        {
            _instance = instance;
        }

        public void Add(string attributeName, object value)
        {
            this[attributeName] = value;
        }

        public void DeleteAttribute(string attributeName)
        {
            // Don't do anything, we have a business class
        }

        public Type GetType(string attributeName)
        {
            return ObjectAccess.TryGetValue(attributeName, out var item)
                ? item.memberType
                : throw new ArgumentOutOfRangeException();
        }

        public bool Exists(string attributeName)
        {
            return ObjectAccess.ContainsKey(attributeName);
        }

        public string[] GetNames()
        {
            return MemberNames;
        }

        public object[] GetValues()
        {
            return Array.ConvertAll(ValueAccessors, accessor => accessor?.Invoke(_instance));
        }

        public object this[string attributeName]
        {
            get
            {
                if (!ObjectAccess.TryGetValue(attributeName, out var tup))
                {
                    throw new ArgumentOutOfRangeException(nameof(attributeName));
                }

                return tup.getter?.Invoke(_instance);
            }

            set
            {
                if (!ObjectAccess.TryGetValue(attributeName, out var tup))
                {
                    throw new ArgumentOutOfRangeException(nameof(attributeName));
                }

                value = Convert.ChangeType(value, tup.memberType);
                tup.setter?.Invoke(_instance, value);
            }
        }

        public int Count => ObjectAccess.Count;

        public void SetValues(IAttributesTable value)
        {
            if (value == this)
            {
                return;
            }

            foreach (string name in value.GetNames())
            {
                this[name] = value[name];
            }
        }

        public object GetOptionalValue(string attributeName)
        {
            ObjectAccess.TryGetValue(attributeName, out var tuple);
            return tuple.getter?.Invoke(_instance);
        }
    }
}
