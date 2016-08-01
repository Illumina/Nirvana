using System;
using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public class CustomInterval : AnnotationInterval, IComparable<CustomInterval>, ICustomInterval
    {
        int ICustomInterval.Start => Start;
        int ICustomInterval.End => End;
        public string ReferenceName { get; protected set; }
        public string Type { get; protected set; }
        public IDictionary<string, string> StringValues { get; set; }
        public IDictionary<string, string> NonStringValues { get; set; }

        public CustomInterval(string referenceName, int start, int end, string type, Dictionary<string, string> stringValues, Dictionary<string, string> nonStringValues) : base(start, end)
        {
            ReferenceName = referenceName;
            Start = start;
            End = end;
            Type = type;
            StringValues = stringValues;
            NonStringValues = nonStringValues;
        }

        protected CustomInterval()
        {
            Clear();
        }

        public static CustomInterval GetEmptyInterval()
        {
            return new CustomInterval(null, -1, -1, null, null, null);
        }

        private void Clear()
        {
            ReferenceName = null;
            Start = -1;
            End = -1;
            Type = null;
            StringValues = null;
            NonStringValues = null;
        }

        public bool IsEmpty()
        {
            return Start == -1
                && End == -1;
        }

        public int CompareTo(CustomInterval other)
        {
            if (ReferenceName != other.ReferenceName)
                return string.Compare(ReferenceName, other.ReferenceName, StringComparison.Ordinal);

            return Start.CompareTo(other.Start);
        }

        public override bool Equals(object other)
        {
            var otherItem = other as CustomInterval;
            if (otherItem == null) return false;

            return ReferenceName.Equals(otherItem.ReferenceName)
                   && Start.Equals(otherItem.Start)
                   && End.Equals(otherItem.End)
                   && Type.Equals(otherItem.Type);
        }

        public override int GetHashCode()
        {
            var hashCode = Start.GetHashCode() ^ ReferenceName.GetHashCode();
            hashCode = (hashCode * 397) ^ End.GetHashCode();
            hashCode = (hashCode * 397) ^ Type.GetHashCode();

            return hashCode;
        }
    }
}
