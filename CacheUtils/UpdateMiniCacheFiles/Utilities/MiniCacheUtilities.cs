using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.DataDumperImport.Utilities;
using CacheUtils.UpdateMiniCacheFiles.DataStructures;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Reference;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.DataStructures.Variants;
using VariantAnnotation.FileHandling.VCF;

namespace CacheUtils.UpdateMiniCacheFiles.Utilities
{
    public static class MiniCacheUtilities
    {
        private const string TempCacheExt = ".tmp";

        public static void WriteTranscriptCache(GlobalCache cache, string outputStub, List<string> outputFiles)
        {
            var outputPath = outputStub + ".ndb" + TempCacheExt;
            outputFiles.Add(outputPath);

            using (var writer = new GlobalCacheWriter(outputPath, (FileHeader)cache.Header))
            {
                writer.Write(cache);
            }
        }

        public static void WritePredictionCaches(PredictionCacheStaging sift, PredictionCacheStaging polyPhen,
            string outputStub, List<string> outputFiles)
        {
            var siftPath     = outputStub + ".sift" + TempCacheExt;
            var polyphenPath = outputStub + ".polyphen" + TempCacheExt;
            outputFiles.Add(siftPath);
            outputFiles.Add(polyphenPath);

            sift?.Write(siftPath);
            polyPhen?.Write(polyphenPath);
        }

        public static void WriteBases(DataBundle bundle, List<TranscriptPacket> packets, string outputStub,
            List<string> outputFiles)
        {
            var refPath = outputStub + ".bases" + TempCacheExt;
            outputFiles.Add(refPath);

            const int flankingLength = 500;

            using (var writer = new CompressedSequenceWriter(refPath, bundle.SequenceReader.Metadata,
                bundle.Sequence.CytogeneticBands, bundle.Cache.Header.GenomeAssembly))
            {
                for (ushort refIndex = 0; refIndex < bundle.Sequence.Renamer.NumRefSeqs; refIndex++)
                {
                    var interval = GetBoundingInterval(packets, refIndex, flankingLength);
                    if (interval == null) continue;

                    LoadSequence(bundle, refIndex);
                    var bases = bundle.Sequence.Substring(interval.Start, interval.End - interval.Start + 1);

                    var ensemblRefName = bundle.Sequence.Renamer.EnsemblReferenceNames[refIndex];
                    writer.Write(ensemblRefName, bases, interval.Start);
                }
            }
        }

        /// <summary>
        /// this function is used when we want to populate the chromosome renamer and the cytogenetic bands
        /// </summary>
        public static void WriteEmptyBases(DataBundle bundle, HashSet<ushort> refIndices, string outputStub,
            List<string> outputFiles)
        {
            var refPath = outputStub + ".bases" + TempCacheExt;
            outputFiles.Add(refPath);

            using (var writer = new CompressedSequenceWriter(refPath, bundle.SequenceReader.Metadata,
                    bundle.Sequence.CytogeneticBands, bundle.Cache.Header.GenomeAssembly))
            {
                foreach (var refIndex in refIndices.OrderBy(x => x))
                {
                    var ensemblRefName = bundle.Sequence.Renamer.EnsemblReferenceNames[refIndex];
                    writer.Write(ensemblRefName, "A");
                }
            }
        }

        private static void LoadSequence(DataBundle bundle, ushort refIndex)
        {
            if (refIndex == bundle.CurrentRefIndex) return;
            bundle.SequenceReader.GetCompressedSequence(bundle.Sequence.Renamer.EnsemblReferenceNames[refIndex]);
            bundle.CurrentRefIndex = refIndex;
        }

        private static ReferenceAnnotationInterval GetBoundingInterval(List<TranscriptPacket> packets, ushort refIndex,
            int flankingLength)
        {
            int start = Int32.MaxValue;
            int end   = Int32.MinValue;

            foreach (var packet in packets)
            {
                if (packet.ReferenceIndex != refIndex) continue;
                if (packet.Transcript.Start < start) start = packet.Transcript.Start;
                if (packet.Transcript.End > end) end = packet.Transcript.End;
            }

            if (start == Int32.MaxValue || end == Int32.MinValue) return null;

            start -= flankingLength;
            if (start < 1) start = 1;

            end += flankingLength;

            return new ReferenceAnnotationInterval(refIndex, start, end);
        }

        private static Tuple<ushort, int, int> GetTuple(string vcfLine, ChromosomeRenamer renamer, int flankingLength = 0)
        {
            var fields = vcfLine.Split('\t');
            if (fields.Length < VcfCommon.MinNumColumns)
            {
                throw new GeneralException($"Expected at least {VcfCommon.MinNumColumns} fields in the vcf string: [{vcfLine}]");
            }

            var vcfVariant = new VcfVariant(fields, vcfLine, false);
            var variant    = new VariantFeature(vcfVariant, renamer, new VID());

            return new Tuple<ushort, int, int>(variant.ReferenceIndex, variant.VcfReferenceBegin - flankingLength,
                variant.VcfReferenceEnd + flankingLength);
        }

        public static Transcript GetDesiredTranscript(DataBundle bundle, string transcriptId, ushort refIndex)
        {
            return bundle.Cache.Transcripts.FirstOrDefault(t => t.ReferenceIndex == refIndex && t.Id.ToString() == transcriptId);
        }

        public static List<TranscriptPacket> GetDesiredTranscripts(DataBundle bundle, List<string> transcriptIds)
        {
            var idSet = new HashSet<string>();
            foreach (var id in transcriptIds) idSet.Add(id);

            var transcripts = (from transcript in bundle.Cache.Transcripts
                where idSet.Contains(transcript.Id.ToString())
                select new TranscriptPacket(transcript)).ToList();

            return transcripts.OrderBy(x => x.Id).ToList();
        }

        public static RegulatoryElement GetDesiredRegulatoryElement(DataBundle bundle, string regulatoryElementId)
        {
            return bundle.Cache.RegulatoryElements.FirstOrDefault(r => r.Id.ToString() == regulatoryElementId);
        }

        private static Transcript[] GetDesiredTranscripts(DataBundle bundle, Tuple<ushort, int, int> variantInterval)
        {
            var overlappingTranscripts = new List<Transcript>();
            bundle.TranscriptForest.GetAllOverlappingValues(variantInterval.Item1, variantInterval.Item2,
                variantInterval.Item3, overlappingTranscripts);
 
            return overlappingTranscripts.ToArray();
        }

        public static Transcript[] GetTranscriptsByVcf(DataBundle bundle, string vcfLine)
        {
            var variantInterval = GetTuple(vcfLine, bundle.Sequence.Renamer, NirvanaAnnotationSource.FlankingLength);
            return GetDesiredTranscripts(bundle, variantInterval);
        }

        public static GlobalCache CreateCache(IFileHeader header, List<TranscriptPacket> packets)
        {
            var builder = new TranscriptCacheBuilder(header, packets);
            return builder.Create();
        }
    }
}