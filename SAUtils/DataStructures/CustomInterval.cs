using System;
using System.Collections.Generic;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class CustomInterval : IComparable<CustomInterval>
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public string Type { get; }
        public IDictionary<string, string> StringValues { get; }
        public IDictionary<string, string> NonStringValues { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public CustomInterval(IChromosome chromosome, int start, int end, string type,
            IDictionary<string, string> stringValues, IDictionary<string, string> nonStringValues)
        {
            Chromosome   = chromosome;
            Start           = start;
            End             = end;
            Type            = type;
            StringValues    = stringValues;
            NonStringValues = nonStringValues;
        }

        public int CompareTo(CustomInterval other)
        {
            return Chromosome != other.Chromosome ? Chromosome.Index.CompareTo(other.Chromosome.Index) : Start.CompareTo(other.Start);
        }

        public override bool Equals(object other)
        {
            if (!(other is CustomInterval otherItem)) return false;

            return Chromosome.Equals(otherItem.Chromosome)
                   && Start.Equals(otherItem.Start)
                   && End.Equals(otherItem.End)
                   && Type.Equals(otherItem.Type);
        }

        public override int GetHashCode()
        {
            var hashCode = Start.GetHashCode() ^ Chromosome.GetHashCode();
            hashCode = (hashCode * 397) ^ End.GetHashCode();
            hashCode = (hashCode * 397) ^ Type.GetHashCode();

            return hashCode;
        }

		public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
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

		    return StringBuilderCache.GetStringAndRelease(sb);
		}
	}
}
