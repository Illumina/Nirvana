using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Algorithms.Consequences
{
    public sealed class BasicVariantEffects
    {
        #region members

        public bool IsCoding { get; private set; }

        public bool IsDeletion { get; private set; }

        public bool IsInsertion { get; }

        // Nirvana additions
        public readonly string ReferenceCodon;
        public readonly string AlternateCodon;

        public readonly int ReferenceCodonLen;
        public int AlternateCodonLen;

        public readonly string ReferenceAminoAcids;
        public readonly string AlternateAminoAcids;

        public readonly int ReferenceAminoAcidsLen;
        public readonly int AlternateAminoAcidsLen;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public BasicVariantEffects(TranscriptAnnotation ta)
        {
            var altAllele = ta.AlternateAllele;

            // codons
            ReferenceCodon = ta.ReferenceCodon ?? "";
            AlternateCodon = ta.AlternateCodon ?? "";

            ReferenceCodonLen = ReferenceCodon.Length;
            AlternateCodonLen = AlternateCodon.Length;

            // amino acids
            ReferenceAminoAcids = ta.ReferenceAminoAcids ?? "";
            AlternateAminoAcids = ta.AlternateAminoAcids ?? "";

            ReferenceAminoAcidsLen = ReferenceAminoAcids.Length;
            AlternateAminoAcidsLen = AlternateAminoAcids.Length;

            // variant type-specific
	        IsDeletion = altAllele.NirvanaVariantType == VariantType.deletion;
	        IsInsertion = altAllele.NirvanaVariantType == VariantType.insertion;

            // within coding region
            if (ta.HasValidCdsStart || ta.HasValidCdsEnd) IsCoding = true;

			//insertion in start codon and do not change start codon
	        if (IsInsertion && ta.ProteinBegin <= 1 && AlternateAminoAcids.EndsWith(ReferenceAminoAcids)) IsCoding = false;
        }
    }
}
