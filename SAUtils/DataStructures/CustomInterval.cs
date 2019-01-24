using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.SA;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class CustomInterval : ISuppIntervalItem
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        public VariantType VariantType { get; }

        private readonly List<string> _values;
        private readonly SaJsonSchema _jsonSchema;


        /// <summary>
        /// constructor
        /// </summary>
        public CustomInterval(IChromosome chromosome, int start, int end, List<string> values, SaJsonSchema jsonSchema)
        {
            Chromosome      = chromosome;
            Start           = start;
            End             = end;
            VariantType     = VariantType.structural_alteration;
            _values         = values;
            _jsonSchema     = jsonSchema;
        }

        public string GetJsonString() => _jsonSchema.GetJsonString(_values);

	}
}
