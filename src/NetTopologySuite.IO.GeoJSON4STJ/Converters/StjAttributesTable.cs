using System;
using System.Linq;
using System.Text.Json;

using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Converters
{
    internal sealed class StjAttributesTable : IAttributesTable
    {
        public StjAttributesTable()
        {
            using (var doc = JsonDocument.Parse(""))
            {
                RootElement = doc.RootElement.Clone();
            }
        }

        public StjAttributesTable(JsonElement rootElement)
        {
            RootElement = rootElement.Clone();
        }

        public JsonElement RootElement { get; }

        public object this[string attributeName]
        {
            get
            {
                return RootElement.TryGetProperty(attributeName, out var prop)
                    ? ConvertValue(prop)
                    : throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            set
            {
                ThrowNotSupportedExceptionForReadOnlyTable();
            }
        }

        public int Count => RootElement.EnumerateObject().Count();

        public void Add(string attributeName, object value)
        {
            ThrowNotSupportedExceptionForReadOnlyTable();
        }

        public void DeleteAttribute(string attributeName)
        {
            ThrowNotSupportedExceptionForReadOnlyTable();
        }

        public bool Exists(string attributeName)
        {
            return RootElement.TryGetProperty(attributeName, out _);
        }

        public object GetOptionalValue(string attributeName)
        {
            return RootElement.TryGetProperty(attributeName, out var prop)
                ? ConvertValue(prop)
                : null;
        }

        public Type GetType(string attributeName)
        {
            if (!RootElement.TryGetProperty(attributeName, out var prop))
            {
                throw new ArgumentException($"Attribute {attributeName} does not exist!", nameof(attributeName));
            }

            return ConvertValue(prop)?.GetType() ?? typeof(object);
        }

        public string[] GetNames()
        {
            return RootElement.EnumerateObject()
                              .Select(prop => prop.Name)
                              .ToArray();
        }

        public object[] GetValues()
        {
            return RootElement.EnumerateObject()
                              .Select(prop => GetOptionalValue(prop.Name))
                              .ToArray();
        }

        private static void ThrowNotSupportedExceptionForReadOnlyTable()
        {
            throw new NotSupportedException("Modifying this attribute table is not supported.");
        }

        private static object ConvertValue(JsonElement prop)
        {
            switch (prop.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    return null;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.String:
                    return prop.GetString();

                case JsonValueKind.Object:
                    return new StjAttributesTable(prop);

                case JsonValueKind.Array:
                    return prop.EnumerateArray()
                               .Select(ConvertValue)
                               .ToArray();

                case JsonValueKind.Number when prop.TryGetDecimal(out decimal d):
                    return d;

                case JsonValueKind.Number:
                    throw new NotSupportedException("Number value cannot be boxed as a decimal: " + prop.GetRawText());

                default:
                    throw new NotSupportedException("Unrecognized JsonValueKind: " + prop.ValueKind);
            }
        }
    }
}
