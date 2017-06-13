using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling.JSON;

namespace SAUtils.DataStructures
{
    public class CustomInterval : IComparable<CustomInterval>
    {
        public string ReferenceName { get; }
        public int Start { get; }
        public int End { get; }
        public string Type { get; }
        public IDictionary<string, string> StringValues { get; }
        public IDictionary<string, string> NonStringValues { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public CustomInterval(string referenceName, int start, int end, string type,
            IDictionary<string, string> stringValues, IDictionary<string, string> nonStringValues)
        {
            ReferenceName   = referenceName;
            Start           = start;
            End             = end;
            Type            = type;
            StringValues    = stringValues;
            NonStringValues = nonStringValues;
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

		public string GetJsonString()
		{
			var sb = new StringBuilder();
			var jsonObject = new JsonObject(sb);

			jsonObject.AddStringValue("start", Start.ToString(), false);
			jsonObject.AddStringValue("end", End.ToString(), false);

			if (StringValues != null)
			{
				foreach (var kvp in StringValues)
				{
					jsonObject.AddStringValue(kvp.Key, kvp.Value);
				}
			}

			if (NonStringValues != null)
			{
				foreach (var kvp in NonStringValues)
				{
					jsonObject.AddStringValue(kvp.Key, kvp.Value, false);
				}
			}

			return sb.ToString();
		}
	}
}
