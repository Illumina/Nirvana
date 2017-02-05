using System.Collections.Generic;
using VariantAnnotation.DataStructures;

namespace CreateGeneSymbolsAndSynonyms
{
    public class GeneInfo
    {
        public int GeneID;
        public int? HgncID;
        public string GeneSymbol;
        public GeneSymbolSource GeneSymbolSource;
        public string Synonyms;
        public HashSet<string> RefSeqAccessions = new HashSet<string>(); 
    }
}
