using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.TranscriptCache
{
    public static class TranscriptConversionExtensions
    {
        public static IEnumerable<ITranscript> ToTranscripts(this MutableTranscript[] mutableTranscripts)
        {
            var transcripts = new List<ITranscript>(mutableTranscripts.Length);
            transcripts.AddRange(mutableTranscripts.Select(mt => mt.ToTranscript()));
            return transcripts;
        }

        private static ITranscript ToTranscript(this MutableTranscript mt)
        {
            var translation = mt.CodingRegion == null
                ? null
                : GetTranslation(mt.CodingRegion, mt.CdsLength, CompactId.Convert(mt.ProteinId, mt.ProteinVersion),
                    mt.PeptideSequence);

            var sortedMicroRnas = mt.MicroRnas?.OrderBy(x => x.Start).ToArray();

            return new Transcript(mt.Chromosome, mt.Start, mt.End, CompactId.Convert(mt.Id, mt.Version), translation,
                mt.BioType, mt.UpdatedGene, mt.TotalExonLength, mt.NewStartExonPhase, mt.IsCanonical,
                mt.TranscriptRegions, (ushort) mt.Exons.Length, sortedMicroRnas, mt.SiftIndex, mt.PolyPhenIndex,
                mt.Source, mt.CdsStartNotFound, mt.CdsEndNotFound, mt.SelenocysteinePositions, mt.RnaEdits);
        }

        private static ITranslation GetTranslation(ICodingRegion oldCodingRegion, int cdsLength, CompactId proteinId,
            string peptideSeq)
        {
            var codingRegion = new CodingRegion(oldCodingRegion.Start, oldCodingRegion.End, oldCodingRegion.CdnaStart,
                oldCodingRegion.CdnaEnd, cdsLength);

            return new Translation(codingRegion, proteinId, peptideSeq);
        }
    }
}
