namespace VariantAnnotation.DataStructures
{
    public class TranscriptAnnotation
    {
        #region members

        // adjusted for transcript orientation
        public string TranscriptReferenceAllele;
        public string TranscriptAlternateAllele;

        public VariantAlternateAllele AlternateAllele;

        // =================================================================================
        // null values indicate an error, deletions should be indicated with an empty string
        // =================================================================================

        public string ReferenceAminoAcids;
        public string AlternateAminoAcids;

        public string ReferenceCodon;
        public string AlternateCodon;

        // =================================================================================

        public int ComplementaryDnaBegin;
        public int ComplementaryDnaEnd;

        // cDNA begin and end that hasn't been masked with the gaps
        public int BackupCdnaBegin;
        public int BackupCdnaEnd;

        // the start and end positions of the coding sequence (from the transcription start site to the end site)
        public int CodingDnaSequenceBegin;
        public int CodingDnaSequenceEnd;

        public int ProteinBegin;
        public int ProteinEnd;

        public string HgvsCodingSequenceName;
        public string HgvsProteinSequenceName;

        // ================
        // intronic effects
        // ================

        public bool IsEndSpliceSite;
        public bool IsStartSpliceSite;
        public bool IsWithinFrameshiftIntron;
        public bool IsWithinIntron;

        // the definition of splice_region (SO:0001630) is "within 1-3 bases of the exon or 3-8 bases of the intron"
        // We also need to special case insertions between the edge of an exon and a donor or acceptor site and between
        // a donor or acceptor site and the intron
        public bool IsWithinSpliceSiteRegion;

        // ==============
        // exonic effects
        // ==============

        public bool HasExonOverlap;

        // true if the start position of the coding region of the exon (contains UTRs) is larger than 0
        public bool HasValidCdnaCodingStart;

        // true if the start position of the coding sequence (no UTRs) is larger than 0
        public bool HasValidCdsStart;
        public bool HasValidCdsEnd;

        // true if both cDNA coordinates are set
        public bool HasValidCdnaStart;
        public bool HasValidCdnaEnd;

        // true if a frameshift was observed
        public bool HasFrameShift;

        #endregion

        /// <summary>
        /// returns the codon string for this annotation
        /// </summary>
        public string GetCodonString()
        {
            if (string.IsNullOrEmpty(ReferenceCodon) && string.IsNullOrEmpty(AlternateCodon)) return "";
            return $"{(string.IsNullOrEmpty(ReferenceCodon) ? "-" : ReferenceCodon)}/{(string.IsNullOrEmpty(AlternateCodon) ? "-" : AlternateCodon)}";
        }
    }
}
