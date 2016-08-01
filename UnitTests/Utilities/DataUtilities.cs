using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace UnitTests.Utilities
{
    internal static class DataUtilities
    {
        /// <summary>
        /// returns the desired JSON transcript given an annotated variant, a transcript ID, and an alt allele
        /// </summary>
        public static ITranscript GetTranscript(IAnnotatedVariant annotatedVariant, string transcriptId, string altAllele, string refAllele = null)
        {
            foreach (var variant in annotatedVariant.AnnotatedAlternateAlleles)
            {
                if (altAllele != null && variant.AltAllele != altAllele) continue;
                if (refAllele != null && variant.RefAllele != refAllele) continue;

                foreach (var transcript in variant.EnsemblTranscripts.Where(transcript => transcript.TranscriptID == transcriptId))
                {
                    return transcript;
                }

                foreach (var transcript in variant.RefSeqTranscripts.Where(transcript => transcript.TranscriptID == transcriptId))
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
        internal static IAnnotatedVariant GetVariant(string cacheFile, string vcfLine)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile);
            return GetVariant(annotationSource, vcfLine);
        }

        /// <summary>
        /// returns the desired JSON variant
        /// </summary>
        internal static IAnnotatedVariant GetVariant(string cacheFile, string supplementaryAnnotationPath,
            string vcfLine, List<SupplementaryAnnotationReader> caReaders = null, bool enableRefNoCall = false,
            bool limitRefNoCallToTranscripts = false)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile) as NirvanaAnnotationSource;
            if (enableRefNoCall) annotationSource?.EnableReferenceNoCalls(limitRefNoCallToTranscripts);

            SupplementaryAnnotationReader saReader = null;
            if (supplementaryAnnotationPath != null)
            {
                saReader = supplementaryAnnotationPath.StartsWith(Path.GetTempPath())
                    ? new SupplementaryAnnotationReader(supplementaryAnnotationPath)
                    : ResourceUtilities.GetSupplementaryAnnotationReader(supplementaryAnnotationPath);
                annotationSource?.SetSupplementaryAnnotationReader(saReader);
            }

            if (caReaders != null) annotationSource?.SetCustomAnnotationReader(caReaders);

            var variant = GetVariant(annotationSource, vcfLine);

            if (supplementaryAnnotationPath != null) saReader.Dispose();

            return variant;
        }

        /// <summary>
        /// returns the desired JSON variant
        /// </summary>
        internal static IAnnotatedVariant GetVariant(string cacheFile, IVariant variant)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(cacheFile);
            return annotationSource?.Annotate(variant);
        }

        /// <summary>
        /// returns the desired JSON variant (using only supplementary intervals)
        /// </summary>
        internal static IAnnotatedVariant GetSuppIntervalVariant(string supplementaryFile, string vcfLine)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(null);
            var supplementaryIntervals = ResourceUtilities.GetSupplementaryIntervals(supplementaryFile);
            annotationSource.AddSupplementaryIntervals(supplementaryIntervals);
            return GetVariant(annotationSource, vcfLine);
        }

        /// <summary>
        /// returns the desired JSON variant
        /// </summary>
        internal static IAnnotatedVariant GetVariant(IAnnotationSource annotationSource, string vcfLine)
        {
            var fields  = vcfLine.Split('\t');
            var variant = new VcfVariant(fields, vcfLine, false);
            UnifiedJson.NeedsVariantComma = false;
            return annotationSource?.Annotate(variant);
        }

        /// <summary>
        /// returns the desired JSON transcript given an annotated variant, a transcript ID, and an alt allele
        /// </summary>
        internal static ITranscript GetTranscript(string cacheFile, string vcfLine, string transcriptId, string altAllele = null, string refAllele = null)
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
    }
}
