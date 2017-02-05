using System.Collections.Generic;

namespace VariantAnnotation.DataStructures
{
    // ReSharper disable InconsistentNaming
    public enum GeneSymbolSource : byte
    {
        Unknown,
        CloneBasedEnsemblGene,
        CloneBasedVegaGene,
        EntrezGene,
        HGNC,
        LRG,
        NCBI,
        miRBase,
        RFAM,
        UniProtGeneName
    }
    // ReSharper restore InconsistentNaming

    public static class GeneSymbolSourceUtilities
    {
        #region members

        private static readonly Dictionary<string, GeneSymbolSource> StringToGeneSymbolSource = new Dictionary<string, GeneSymbolSource>();

        private const string CloneBasedEnsemblGeneSymbolKey = "Clone_based_ensembl_gene";
        private const string CloneBasedVegaGeneSymbolKey    = "Clone_based_vega_gene";
        private const string EntrezGeneSymbolKey            = "EntrezGene";
        private const string HgncSymbolKey                  = "HGNC";
        private const string LrgSymbolKey                   = "LRG";
        private const string MirBaseSymbolKey               = "miRBase";
        private const string NcbiSymbolKey                  = "NCBI";
        private const string RfamSymbolKey                  = "RFAM";
        private const string UniProtGeneNameSymbolKey       = "Uniprot_gn";

        #endregion

        // constructor
        static GeneSymbolSourceUtilities()
        {
            AddGeneSymbolSource(CloneBasedEnsemblGeneSymbolKey, GeneSymbolSource.CloneBasedEnsemblGene);
            AddGeneSymbolSource(CloneBasedVegaGeneSymbolKey,    GeneSymbolSource.CloneBasedVegaGene);
            AddGeneSymbolSource(EntrezGeneSymbolKey,            GeneSymbolSource.EntrezGene);
            AddGeneSymbolSource(HgncSymbolKey,                  GeneSymbolSource.HGNC);
            AddGeneSymbolSource(LrgSymbolKey,                   GeneSymbolSource.LRG);
            AddGeneSymbolSource(MirBaseSymbolKey,               GeneSymbolSource.miRBase);
            AddGeneSymbolSource(NcbiSymbolKey,                  GeneSymbolSource.NCBI);
            AddGeneSymbolSource(RfamSymbolKey,                  GeneSymbolSource.RFAM);
            AddGeneSymbolSource(UniProtGeneNameSymbolKey,       GeneSymbolSource.UniProtGeneName);
        }

        /// <summary>
        /// adds the gene symbol source to both dictionaries
        /// </summary>
        private static void AddGeneSymbolSource(string s, GeneSymbolSource geneSymbolSource)
        {
            StringToGeneSymbolSource[s] = geneSymbolSource;
        }

        /// <summary>
        /// returns the gene symbol source given the string representation
        /// </summary>
        public static GeneSymbolSource GetGeneSymbolSourceFromString(string s)
        {
            if (s == null) return GeneSymbolSource.Unknown;

            GeneSymbolSource ret;
            return !StringToGeneSymbolSource.TryGetValue(s, out ret) ? GeneSymbolSource.Unknown : ret;
        }
    }
}
