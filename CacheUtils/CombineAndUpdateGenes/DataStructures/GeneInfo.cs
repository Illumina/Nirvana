namespace CacheUtils.CombineAndUpdateGenes.DataStructures
{
    public sealed class GeneInfo
    {
        public string Symbol;
        public string EnsemblId;
        public string EntrezGeneId;
        public int HgncId = -1;

        public bool IsEmpty => HgncId == int.MaxValue && Symbol == null;
        public static GeneInfo Empty => new GeneInfo { Symbol = null, HgncId = int.MaxValue };

        public override string ToString()
        {
            return $"Symbol: [{Symbol}], HGNC: {HgncId}, Ensembl: [{EnsemblId}], Entrez: [{EntrezGeneId}]";
        }
    }
}
