using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public class CustomInterval : IComparable<CustomInterval>, ICustomInterval
    {
        public string ReferenceName { get; }
        public int Start { get; }
        public int End { get; }
        public string Type { get; }
        public IDictionary<string, string> StringValues { get; set; }
        public IDictionary<string, string> NonStringValues { get; set; }

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

        public static CustomInterval GetEmptyInterval()
        {
            return new CustomInterval(null, -1, -1, null, null, null);
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

		public void SerializeJson(StringBuilder sb)
		{
			var jsonObject = new JsonObject(sb);

			sb.Append(JsonObject.OpenBrace);
			jsonObject.AddStringValue("Start", Start.ToString(), false);
			jsonObject.AddStringValue("End", End.ToString(), false);
			// jsonObject.AddStringValue("ReferenceName", ReferenceName);//should be a quoted string
			// jsonObject.AddStringValue("Type",Type);//should be a quoted string

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

			sb.Append(JsonObject.CloseBrace);
		}
	}
}
