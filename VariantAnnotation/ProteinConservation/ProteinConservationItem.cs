using System;
using System.Collections.Generic;
using System.Linq;
using IO;

namespace VariantAnnotation.ProteinConservation
{
    public sealed class ProteinConservationItem
    {
        public readonly string TranscriptId;
        public readonly string Chromosome;

        public readonly string ProteinSequence;
        public readonly byte[] Scores;

        public ProteinConservationItem(string chrom, string transcriptId, string proteinSequence, byte[] scores)
        {
            Chromosome      = chrom;
            TranscriptId    = transcriptId;
            ProteinSequence = proteinSequence;
            Scores          = scores;
        }
       
    }
}