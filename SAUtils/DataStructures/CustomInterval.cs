using System.Collections.Generic;
using ErrorHandling.Exceptions;
using Genome;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class CustomInterval : ISuppIntervalItem
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }
        private VariantType VariantType { get; }

        private readonly List<string[]> _values;
        private readonly SaJsonSchema _jsonSchema;
        private readonly string _inputLine;

        /// <summary>
        /// constructor
        /// </summary>
        public CustomInterval(IChromosome chromosome, int start, int end, List<string[]> values, SaJsonSchema jsonSchema, string inputLine)
        {
            Chromosome      = chromosome;
            Start           = start;
            End             = end;
            VariantType     = VariantType.structural_alteration;
            _values         = values;
            _jsonSchema     = jsonSchema;
            _inputLine      = inputLine;
        }

        public string GetJsonString()
        {
            try
            {
                return _jsonSchema.GetJsonString(_values);
            }
            catch (UserErrorException e)
            {
                throw new UserErrorException(e.Message + $"\nInput line: {_inputLine}");
            }
        }

    }
}
