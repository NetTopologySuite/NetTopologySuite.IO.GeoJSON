using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Converters
{
    internal sealed class StjFeature : IFeature, IUnique
    {
        private readonly Feature _implementation = new Feature();

        private readonly string _idPropertyName;

        private object _id;

        public StjFeature(string idPropertyName)
        {
            _idPropertyName = idPropertyName;
        }

        public object Id
        {
            get => _id ?? Attributes?.GetOptionalValue(_idPropertyName);
            set => _id = value;
        }

        public Geometry Geometry
        {
            get => _implementation.Geometry;
            set => _implementation.Geometry = value;
        }

        public Envelope BoundingBox
        {
            get => _implementation.BoundingBox;
            set => _implementation.BoundingBox = value;
        }

        public IAttributesTable Attributes
        {
            get => _implementation.Attributes;
            set => _implementation.Attributes = value;
        }
    }
}
