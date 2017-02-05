using VariantAnnotation.DataStructures;

namespace CacheUtils.CombineAndUpdateGenes.DataStructures
{
    public sealed class MutableGene
    {
        public ushort ReferenceIndex;
        public int Start;
        public int End;
        public bool OnReverseStrand;
        public string Symbol;
        public CompactId EntrezGeneId;
        public CompactId EnsemblId;
        public int HgncId;
        public int MimNumber;

        public TranscriptDataSource TranscriptDataSource;

        // used during gene aggregation
        public bool Invalid;

        public static MutableGene Clone(MutableGene gene)
        {
            return new MutableGene
            {
                ReferenceIndex       = gene.ReferenceIndex,
                Start                = gene.Start,
                End                  = gene.End,
                OnReverseStrand      = gene.OnReverseStrand,
                Symbol               = gene.Symbol,
                EntrezGeneId         = gene.EntrezGeneId,
                EnsemblId            = gene.EnsemblId,
                HgncId               = gene.HgncId,
                MimNumber            = gene.MimNumber,
                TranscriptDataSource = gene.TranscriptDataSource
            };
        }

        /// <summary>
        /// returns a string representation of our gene
        /// </summary>
        public override string ToString()
        {
            var strand    = OnReverseStrand ? 'R' : 'F';
            var hgncId    = HgncId == -1 ? "" : HgncId.ToString();
            var mimNumber = MimNumber == -1 ? "" : MimNumber.ToString();

            return $"{ReferenceIndex}\t{Start}\t{End}\t{strand}\t{Symbol}\t{hgncId}\t{EntrezGeneId}\t{EnsemblId}\t{mimNumber}";
        }

        public Gene ToGene()
        {
            return new Gene(ReferenceIndex, Start, End, OnReverseStrand, Symbol, HgncId, EntrezGeneId, EnsemblId,
                MimNumber);
        }
    }
}
