using System.Collections.Generic;
using ErrorHandling.Exceptions;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;

namespace SAUtils.Custom
{
    public sealed class CustomGene : ISuppGeneItem
    {
        public string GeneSymbol { get; }

        private readonly List<string[]> _values;
        private readonly SaJsonSchema _jsonSchema;
        private readonly string _inputLine;

        public CustomGene(string geneSymbol, List<string[]> values, SaJsonSchema jsonSchema, string inputLine)
        {
            GeneSymbol = geneSymbol;
            _values = values;
            _jsonSchema = jsonSchema;
            _inputLine = inputLine;
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