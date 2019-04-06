using System;
using System.Linq;
using VariantAnnotation.SA;

namespace SAUtils.Schema
{
    public sealed class SaJsonValueType : IEquatable<SaJsonValueType>
    {
        public JsonDataType[] JsonDataTypes { get; }

        public static readonly SaJsonValueType Number = Create(JsonDataType.Number);
        public static readonly SaJsonValueType Bool = Create(JsonDataType.Bool);
        public static readonly SaJsonValueType String = Create(JsonDataType.String);
        public static readonly SaJsonValueType Object = Create(JsonDataType.Object);
        public static readonly SaJsonValueType Array = Create(JsonDataType.Array);
        public static readonly SaJsonValueType StringArray = Create(JsonDataType.Array, JsonDataType.String);
        public static readonly SaJsonValueType ObjectArray = Create(JsonDataType.Array, JsonDataType.Object);

        private SaJsonValueType(params JsonDataType[] dataTypes)
        {
            JsonDataTypes = dataTypes;
        }

        private static SaJsonValueType Create(params JsonDataType[] dataTypes)
        {
            if (dataTypes.Length > 2) throw new ArgumentException("At most two JSON data types are allowed.");
            if (dataTypes.Length == 2 && !dataTypes[0].IsComplexType()) throw new ArgumentException("The first data type must a complex type when two data types provided.");
            
            return new SaJsonValueType(dataTypes);
        }

        private bool JsonTypeEquals(params JsonDataType[] dataTypes) => JsonDataTypes.Length == dataTypes.Length &&
            JsonDataTypes.SequenceEqual(dataTypes);


        public bool Equals(SaJsonValueType other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || JsonTypeEquals(other.JsonDataTypes);
        }

        public override int GetHashCode()
        {
            if (JsonDataTypes == null) return 0;

            unchecked
            {
                return JsonDataTypes.Aggregate(17, (current, jsonDataType) => (current * 1201) ^ jsonDataType.GetHashCode());
            }
        }

    }
}