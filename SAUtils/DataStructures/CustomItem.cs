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

        private readonly string[] _values;
        private readonly SaJsonSchema _jsonSchema;

        public CustomItem(IChromosome chromosome, int start, string refAllele, string altAllele, string[] values, SaJsonSchema jsonSchema)
        {
            Chromosome = chromosome;
            Position = start;
            RefAllele = refAllele;
            AltAllele = altAllele;
            _values = values;
            _jsonSchema = jsonSchema;
        }

        public string GetJsonString()
        {
            var allValues = new List<string> { RefAllele, AltAllele };
            allValues.AddRange(_values);
            return _jsonSchema.GetJsonString(allValues);
        }
    }
}
