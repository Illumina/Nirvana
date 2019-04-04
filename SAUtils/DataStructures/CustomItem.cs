using System.Collections.Generic;
using ErrorHandling.Exceptions;
using Genome;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Utilities;

namespace SAUtils.DataStructures
{
    public sealed class CustomItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private readonly string[][] _values;
        private readonly SaJsonSchema _jsonSchema;
        private readonly string _inputLine;

        public CustomItem(IChromosome chromosome, int start, string refAllele, string altAllele, string[][] values, SaJsonSchema jsonSchema, string inputLine)
        {
            Chromosome = chromosome;
            Position = start;
            RefAllele = refAllele;
            AltAllele = altAllele;
            _values = values;
            _jsonSchema = jsonSchema;
            _inputLine = inputLine;
        }

        public string GetJsonString()
        {
            var allValues = new List<string[]> {new []{BaseFormatting.EmptyToDash(RefAllele)}, new []{BaseFormatting.EmptyToDash(AltAllele)} };
            allValues.AddRange(_values);
            try
            {
                return _jsonSchema.GetJsonString(allValues);
            }
            catch (UserErrorException e)
            {
                throw new UserErrorException(e.Message + $"\nInput line: {_inputLine}");
            }
        }
    }
}
