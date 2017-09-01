using System.Collections.Generic;

namespace SAUtils.CreateOmimTsv
{
    public class GeneSymbolSynonyms
    {
        public string GeneSymbol;
        public List<string> Synonyms = new List<string>();

        private const string EmptyString = "empty";
        public static GeneSymbolSynonyms Empty => new GeneSymbolSynonyms { GeneSymbol = EmptyString };
        public bool IsEmpty => GeneSymbol == EmptyString;
    }
}
