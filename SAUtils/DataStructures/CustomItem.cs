using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.SA;

namespace SAUtils.DataStructures
{
    public sealed class CustomItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private readonly List<string> _values;
        private readonly SaJsonSchema _jsonSchema;


        public CustomItem(IChromosome chromosome, int start, List<string> values, SaJsonSchema jsonSchema)
        {
            Chromosome   = chromosome;
            Position     = start;
            RefAllele    = values[0];
            AltAllele    = values[1];
            _values      = values;
            _jsonSchema = jsonSchema;
        }

        public string GetJsonString() => _jsonSchema.GetJsonString(_values);
    }
}
