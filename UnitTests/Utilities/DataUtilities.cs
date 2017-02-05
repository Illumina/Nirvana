using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace UnitTests.Utilities
{
    internal static class DataUtilities
    {
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

        /// <summary>
        /// returns the desired JSON variant
        /// </summary>
        internal static IAnnotatedVariant GetVariant(string cachePath, string vcfLine)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(cachePath, null);
            return GetVariant(annotationSource, vcfLine);
        }

        internal static IAnnotatedVariant GetVariant(string cacheFile, ISupplementaryAnnotationReader saReader,
            string vcfLine)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile, saReader);
            return GetVariant(annotationSource, vcfLine);
        }

        /// <summary>
        /// returns the desired JSON variant
        /// </summary>
        internal static IAnnotatedVariant GetVariant(string cacheFile, string supplementaryAnnotationPath,
            string vcfLine, bool enableRefNoCall = false, bool limitRefNoCallToTranscripts = false)
        {
            var saReader         = ResourceUtilities.GetSupplementaryAnnotationReader(supplementaryAnnotationPath);
            var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile, saReader) as NirvanaAnnotationSource;
            if (enableRefNoCall) annotationSource?.EnableReferenceNoCalls(limitRefNoCallToTranscripts);

            return GetVariant(annotationSource, vcfLine);
        }

		internal static IAnnotatedVariant GetVariant(string cachePath, ISupplementaryAnnotationReader saReader,
            string vcfLine, IConservationScoreReader csReader)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(cachePath, saReader, csReader);
            return GetVariant(annotationSource, vcfLine);
        }

		internal static IAnnotatedVariant GetCustomVariant(string cacheFile, string  supplementaryAnnotationPath, List<string> customAnnotationPaths,
			string vcfLine)
		{
			var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(supplementaryAnnotationPath);
			var caReader = ResourceUtilities.GetCustomAnnotationReaders(customAnnotationPaths);
			var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile, saReader, null, caReader) as NirvanaAnnotationSource;
			
			return GetVariant(annotationSource, vcfLine);
		}

		/// <summary>
		/// returns the desired JSON variant
		/// </summary>
		internal static IAnnotatedVariant GetVariant(string cacheFile, IVariant variant)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile, null);
            return annotationSource?.Annotate(variant);
        }

        /// <summary>
        /// returns the desired JSON variant (using only supplementary intervals)
        /// </summary>
        internal static IAnnotatedVariant GetSuppIntervalVariant(string supplementaryAnnotationPath, string vcfLine)
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(supplementaryAnnotationPath);
            var annotationSource = ResourceUtilities.GetAnnotationSource(EmptyCachePrefix, saReader);
            return GetVariant(annotationSource, vcfLine);
        }

        /// <summary>
        /// returns the desired JSON variant
        /// </summary>
        internal static IAnnotatedVariant GetVariant(IAnnotationSource annotationSource, string vcfLine)
        {
            var variant = VcfUtilities.GetVcfVariant(vcfLine);
            UnifiedJson.NeedsVariantComma = false;
            return annotationSource?.Annotate(variant);
        }

        /// <summary>
        /// returns the desired JSON transcript given an annotated variant, a transcript ID, and an alt allele
        /// </summary>
        internal static IAnnotatedTranscript GetTranscript(string cacheFile, string vcfLine, string transcriptId, string altAllele = null, string refAllele = null)
        {
            var annotatedVariant = GetVariant(cacheFile, vcfLine);
            return GetTranscript(annotatedVariant, transcriptId, altAllele, refAllele);
        }

        internal static int GetCount(string s, string substring)
        {
            return Regex.Matches(s, substring).Count;
        }

        /// <summary>
        /// returns the desired JSON regulatory region given an annotated variant and regulatory region ID
        /// </summary>
        internal static IRegulatoryRegion GetRegulatoryRegion(string cacheFile, string vcfLine, string regulatoryRegionId)
        {
            var annotatedVariant = GetVariant(cacheFile, vcfLine);
            return GetRegulatoryRegion(annotatedVariant, regulatoryRegionId);
        }

        /// <summary>
        /// adds a phyloP score to the specified alternate allele
        /// </summary>
        internal static void SetConservationScore(IAnnotatedAlternateAllele altAllele, string phylopScore)
        {
            var jsonVariant = altAllele as JsonVariant;
            if (jsonVariant == null) return;
            jsonVariant.PhylopScore = phylopScore;
        }

        public static CacheData GetFirstTranscript(string cacheStub, string ensemblRefName)
        {
            var cache              = GetTranscriptCache(cacheStub);
            var compressedSequence = GetCompressedSequence(cacheStub, ensemblRefName);
            return new CacheData(compressedSequence, cache.Transcripts[0]);
        }

        public static Transcript GetTranscript(string cacheStub, string transcriptId)
        {
            var cache = GetTranscriptCache(cacheStub);
            return cache.Transcripts.FirstOrDefault(transcript => transcript.Id.ToString() == transcriptId);
        }

        public static ICompressedSequence GetCompressedSequence(string cacheStub, string ensemblRefName)
        {
            var basesStream = ResourceUtilities.GetReadStream($"{cacheStub}.bases");
            var sequence    = new CompressedSequence();

            using (var reader = new CompressedSequenceReader(basesStream, sequence))
            {
                reader.GetCompressedSequence(ensemblRefName);
            }

            return sequence;
        }

        public static GlobalCache GetTranscriptCache(string cacheStub)
        {
            GlobalCache cache;
            var transcriptStream = ResourceUtilities.GetReadStream($"{cacheStub}.ndb");
            using (var reader = new GlobalCacheReader(transcriptStream)) cache = reader.Read();
            return cache;
        }

        public static IAnnotationSource EmptyAnnotationSource
            => ResourceUtilities.GetAnnotationSource(EmptyCachePrefix, null);

        public static string EmptyCachePrefix => Resources.CacheGRCh37("ENSR00001584270_chr1_Ensembl84_reg");
    }
}
