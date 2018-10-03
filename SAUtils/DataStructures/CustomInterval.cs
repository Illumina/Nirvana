using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class CustomInterval : ISuppIntervalItem
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public VariantType VariantType { get; }

        public IDictionary<string, string> StringValues { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public CustomInterval(IChromosome chromosome, int start, int end, IDictionary<string, string> stringValues)
        {
            Chromosome      = chromosome;
            Start           = start;
            End             = end;
            VariantType     = VariantType.structural_alteration;
            StringValues    = stringValues;
        }

        public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);

			jsonObject.AddStringValue("start", Start.ToString());
			jsonObject.AddStringValue("end", End.ToString());

		    foreach ((string key, string value) in StringValues)
		    {
		        jsonObject.AddStringValue(key, value);
		    }

            return StringBuilderCache.GetStringAndRelease(sb);
		}
	}
}
