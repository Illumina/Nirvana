using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SA;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace UnitTests.Utilities
{
    internal static class DataUtilities
    {
        public static IAnnotationSource EmptyAnnotationSource => ResourceUtilities.GetAnnotationSource(EmptyCachePrefix, null);
        public static string EmptyCachePrefix                 => Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg");

        #region GetTranscript

        /// <summary>
        /// returns the desired JSON transcript given an annotated variant, a transcript ID, and an alt allele
        /// </summary>
        public static IAnnotatedTranscript GetTranscript(IAnnotatedVariant annotatedVariant, string transcriptId, string altAllele, string refAllele = null)
        {
            foreach (var variant in annotatedVariant.AnnotatedAlternateAlleles)
            {
                if (altAllele != null && variant.AltAllele != altAllele) continue;
                if (refAllele != null && variant.RefAllele != refAllele) continue;

                foreach (var transcript in
                    variant.EnsemblTranscripts.Where(
                        transcript => FormatUtilities.SplitVersion(transcript.TranscriptID).Item1 == transcriptId))
                {
                    return transcript;
                }

                foreach (var transcript in
                    variant.RefSeqTranscripts.Where(
                        transcript => FormatUtilities.SplitVersion(transcript.TranscriptID).Item1 == transcriptId))
                {
                    return transcript;
                }
            }

            return null;
        }

        /// <summary>
        /// returns the desired JSON transcript given an annotated variant, a transcript ID, and an alt allele
        /// </summary>
        internal static IAnnotatedTranscript GetTranscript(string cacheFile, string vcfLine, string transcriptId, string altAllele = null, string refAllele = null)
        {
            var annotatedVariant = GetVariant(cacheFile, null as List<string>, vcfLine);
            return GetTranscript(annotatedVariant, transcriptId, altAllele, refAllele);
        }

        #endregion

        #region GetRegulatoryRegion

        internal static IRegulatoryRegion GetRegulatoryRegion(string cacheFile, string vcfLine, string regulatoryRegionId)
        {
            var annotatedVariant = GetVariant(cacheFile, null as List<string>, vcfLine);
            return GetRegulatoryRegion(annotatedVariant, regulatoryRegionId);
        }

        private static IRegulatoryRegion GetRegulatoryRegion(IAnnotatedVariant annotatedVariant, string regulatoryRegionId)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var variant in annotatedVariant.AnnotatedAlternateAlleles)
            {
                foreach (var regulatoryRegion in variant.RegulatoryRegions.Where(r => r.ID == regulatoryRegionId))
                {
                    return regulatoryRegion;
                }
            }

            return null;
        }

        #endregion

        #region GetVariant

        internal static IAnnotatedVariant GetVariant(string cacheFile, string saPath, string vcfLine,
            bool enableRefNoCall = false, bool limitRefNoCallToTranscripts = false)
        {
            return GetVariant(cacheFile, new List<string> { saPath }, vcfLine, enableRefNoCall,
                limitRefNoCallToTranscripts);
        }

        internal static IAnnotatedVariant GetVariant(string cacheFile, List<string> saPaths, string vcfLine,
            bool enableRefNoCall = false, bool limitRefNoCallToTranscripts = false)
        {
            var saReaders = GetSupplementaryAnnotationReaders(saPaths);
            var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile, saReaders) as NirvanaAnnotationSource;
            if (enableRefNoCall) annotationSource?.EnableReferenceNoCalls(limitRefNoCallToTranscripts);

            var variant = VcfUtilities.GetVcfVariant(vcfLine);
            return GetVariant(annotationSource, variant);
        }

        internal static IAnnotatedVariant GetVariant(IAnnotationSource annotationSource, IVariant variant)
        {
            UnifiedJson.NeedsVariantComma = false;
            return annotationSource?.Annotate(variant);
        }

        private static List<ISupplementaryAnnotationReader> GetSupplementaryAnnotationReaders(List<string> saPaths)
        {
            var saReaders = new List<ISupplementaryAnnotationReader>();
            if (saPaths == null) return saReaders;

            foreach (var saPath in saPaths)
            {
                var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(saPath);
                if (saReader != null) saReaders.Add(saReader);
            }

            return saReaders;
        }

        #endregion

        internal static List<ISupplementaryAnnotationReader> GetReaders(ISupplementaryAnnotationReader saReader)
        {
            var saReaders = new List<ISupplementaryAnnotationReader>();
            if (saReader != null) saReaders.Add(saReader);
            return saReaders;
        }

/*
        internal static int GetCount(string s, string substring)
        {
            return Regex.Matches(s, substring).Count;
        }
*/

        internal static void SetConservationScore(IAnnotatedAlternateAllele altAllele, string phylopScore)
        {
            var jsonVariant = altAllele as JsonVariant;
            if (jsonVariant == null) return;
            jsonVariant.PhylopScore = phylopScore;
        }

        public static ICompressedSequence GetCompressedSequence(string cacheStub, string ensemblRefName)
        {
            var basesStream = ResourceUtilities.GetReadStream($"{cacheStub}.bases");
            var sequence = new CompressedSequence();

            using (var reader = new CompressedSequenceReader(basesStream, sequence))
            {
                reader.GetCompressedSequence(ensemblRefName);
            }

            return sequence;
        }

        public static Transcript FindTranscript(string cacheStub, string transcriptId)
        {
            var cache = GetTranscriptCache(cacheStub);
            return cache.Transcripts.FirstOrDefault(transcript => transcript.Id.ToString() == transcriptId);
        }

        public static GlobalCache GetTranscriptCache(string cacheStub)
        {
            GlobalCache cache;
            var transcriptStream = ResourceUtilities.GetReadStream($"{cacheStub}.ndb");
            using (var reader = new GlobalCacheReader(transcriptStream)) cache = reader.Read();
            return cache;
        }

        public static ISaPosition CreateSaPosition(string globalMajorAllele = null)
        {
            var dataSources = new ISaDataSource[1];
            dataSources[0]  = new SaDataSource("test", "GMAF", "-", false, false, "", new[] { "" });

            var saPos = new SaPosition(dataSources, globalMajorAllele);
            return saPos;
        }
    }
}
