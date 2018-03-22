using VariantAnnotation.GeneAnnotation;

namespace SAUtils.CreateOmimTsv
{
    public sealed class OmimImportEntry
    {
        private readonly int _mimNumber;
        public string GeneSymbol;
        private readonly string _description;
        private readonly string _phenotypeInfo;
        public readonly string EntrezGeneId;
        public readonly string EnsemblGeneId;

        public OmimImportEntry(int mimNumber, string geneSymbol, string description, string phenotypeInfo,
            string entrezGeneId, string ensemblGeneId)
        {
            _mimNumber     = mimNumber;
            GeneSymbol     = geneSymbol;
            _description   = description;
            _phenotypeInfo = phenotypeInfo;
            EntrezGeneId   = entrezGeneId;
            EnsemblGeneId  = ensemblGeneId;
        }

        public OmimEntry ToOmimEntry()
        {
            var phenotypes = OmimPhenotype.Parse(_phenotypeInfo);
            return new OmimEntry(GeneSymbol, _description, _mimNumber, phenotypes);
        }
    }
}
