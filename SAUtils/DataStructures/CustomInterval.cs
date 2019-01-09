using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface;
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
        private readonly ISaJsonSchema _jsonSchema;


        /// <summary>
        /// constructor
        /// </summary>
        public CustomInterval(IChromosome chromosome, List<string> values, ISaJsonSchema jsonSchema)
        {
            Chromosome      = chromosome;
            Start           = int.Parse(values[0]);
            End             = int.Parse(values[1]);
            VariantType     = VariantType.structural_alteration;
            _values         = values;
            _jsonSchema     = jsonSchema;
        }

        public string GetJsonString() => _jsonSchema.GetJsonString(_values);

	}
}
