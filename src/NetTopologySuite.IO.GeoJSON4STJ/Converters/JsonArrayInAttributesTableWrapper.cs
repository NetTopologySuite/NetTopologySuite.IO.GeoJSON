using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

using NetTopologySuite.IO.Converters;

namespace NetTopologySuite.Features
{
    internal sealed class JsonArrayInAttributesTableWrapper : IList<object>, IReadOnlyList<object>
    {
        private readonly JsonArray _array;

        private readonly JsonSerializerOptions _serializerOptions;

        public JsonArrayInAttributesTableWrapper(JsonArray array, JsonSerializerOptions serializerOptions)
        {
            _array = array;
            _serializerOptions = serializerOptions;
        }

        public object this[int index]
        {
            get => Utility.ObjectFromJsonNode(_array[index], _serializerOptions);
            set => _array[index] = Utility.ObjectToJsonNode(value, _serializerOptions);
        }

        public int Count => _array.Count;

        bool ICollection<object>.IsReadOnly => false;

        public void Add(object item)
        {
            _array.Add(Utility.ObjectToJsonNode(item, _serializerOptions));
        }

        public void Clear()
        {
            _array.Clear();
        }

        public bool Contains(object item)
        {
            foreach (JsonNode node in _array)
            {
                object obj = Utility.ObjectFromJsonNode(node, _serializerOptions);
                if (Equals(item, obj))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            foreach (JsonNode node in _array)
            {
                array[arrayIndex++] = Utility.ObjectFromJsonNode(node, _serializerOptions);
            }
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _array.Select(node => Utility.ObjectFromJsonNode(node, _serializerOptions)).GetEnumerator();
        }

        public int IndexOf(object item)
        {
            for (int i = 0; i < _array.Count; i++)
            {
                object obj = Utility.ObjectFromJsonNode(_array[i], _serializerOptions);
                if (Equals(item, obj))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, object item)
        {
            _array.Insert(index, Utility.ObjectToJsonNode(item, _serializerOptions));
        }

        public bool Remove(object item)
        {
            for (int i = 0; i < _array.Count; i++)
            {
                object obj = Utility.ObjectFromJsonNode(_array[i], _serializerOptions);
                if (Equals(item, obj))
                {
                    _array.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            _array.RemoveAt(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
