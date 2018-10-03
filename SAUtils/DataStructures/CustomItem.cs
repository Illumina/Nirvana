using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class CustomItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private Dictionary<string, string> FieldValues { get; }
        

        public CustomItem(IChromosome chromosome, int start, string refAllele, string altAllele, Dictionary<string, string> fieldValues)
        {
            Chromosome  = chromosome;
            Position    = start;
            RefAllele   = refAllele;
            AltAllele   = altAllele;
            FieldValues = fieldValues;
        }

        public string GetJsonString()
	    {
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);

			foreach ((string key, string value)  in FieldValues)
			{
			    jsonObject.AddStringValue(key, value);
			}

			return StringBuilderCache.GetStringAndRelease(sb);
	    }

	}
}
