using System;
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
            var translation = mt.CodingRegion.IsNull ? null : new Translation(mt.CodingRegion, CompactId.Convert(mt.ProteinId, mt.ProteinVersion),
                mt.PeptideSequence);

            var startExonPhase  = mt.StartExonPhase < 0 ? (byte)0 : (byte)mt.StartExonPhase;
            var sortedCdnaMaps  = mt.CdnaMaps?.OrderBy(x => x.Start).ToArray();
            var sortedIntrons   = mt.Introns?.OrderBy(x => x.Start).ToArray();
            var sortedMicroRnas = mt.MicroRnas?.OrderBy(x => x.Start).ToArray();

            return new Transcript(mt.Chromosome, mt.Start, mt.End, CompactId.Convert(mt.Id, mt.Version), translation,
                mt.BioType, mt.UpdatedGene, mt.TotalExonLength, startExonPhase, mt.IsCanonical, sortedIntrons,
                sortedMicroRnas, sortedCdnaMaps, mt.SiftIndex, mt.PolyPhenIndex, mt.Source, mt.CdsStartNotFound,
                mt.CdsEndNotFound, mt.SelenocysteinePositions, mt.RnaEdits);
        }
    }
}
