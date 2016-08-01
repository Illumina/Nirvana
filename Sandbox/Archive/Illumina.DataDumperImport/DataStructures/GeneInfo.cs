using System.Collections.Generic;
using Illumina.VariantAnnotation.DataStructures;

namespace Illumina.DataDumperImport.DataStructures
{
    public class GeneInfo
    {
        public int GeneId;
        public int? HgncId;
        public string GeneSymbol;
        public GeneSymbolSource GeneSymbolSource;
        public string Synonyms;
        public readonly HashSet<string> RefSeqAccessions = new HashSet<string>(); 
    }
}
