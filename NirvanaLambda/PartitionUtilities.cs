using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cloud.Messages.Annotation;
using Compression.FileHandling;
using Genome;
using Intervals;
using IO;
using Tabix;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;

namespace NirvanaLambda
{
    public static class PartitionUtilities
    {
        public static List<long> GetFileOffsets(string vcfUrl, int numPartitions, Index tabixIndex)
        {
            long fileSize = HttpUtilities.GetLength(vcfUrl);
            long[] sizeBasedOffsets = GetEqualSizeOffsets(fileSize, numPartitions);
            return GetBlockOffsets(sizeBasedOffsets, tabixIndex);
        }

        private static List<long> GetBlockOffsets(long[] sizeBasedOffsets, Index tabixIndex)
        {
            long[] allLinearOffsets = GetAllLinearFileOffsets(tabixIndex);

            return FindEqualOrClosestSmallerOffsets(sizeBasedOffsets, allLinearOffsets);
        }

        internal static List<long> FindEqualOrClosestSmallerOffsets(long[] sizeBasedOffsets, long[] allLinearOffsets)
        {
            if (sizeBasedOffsets == null || allLinearOffsets == null) return new List<long>();

            var closestOffsets = new List<long>();
            var startIndex = 0;

            foreach (long offset in sizeBasedOffsets)
            {
                int searchedIndex = Array.BinarySearch(allLinearOffsets, startIndex, allLinearOffsets.Length - startIndex, offset);
                if (searchedIndex < 0) searchedIndex = ~searchedIndex - 1;
                if (searchedIndex < 0) searchedIndex = 0;

                // only add new offset if it is different from the last one in the list
                if (closestOffsets.Count == 0 || startIndex != searchedIndex) closestOffsets.Add(allLinearOffsets[searchedIndex]);
                startIndex = searchedIndex;
            }

            return closestOffsets;
        }

        internal static long[] GetEqualSizeOffsets(long fileSize, int numPartitions)
        {
            var offsets = new long[numPartitions];
            long baseSize = fileSize / numPartitions;

            //put all the extra {fileSize%numPartitions} bytes to the last partition
            for (var i = 0; i < numPartitions; i++) offsets[i] = baseSize * i;

            return offsets;
        }

        private static long[] GetAllLinearFileOffsets(Index tabixIndex) =>
        MergeConsecutiveEqualValues(tabixIndex.ReferenceSequences.SelectMany(x => x.LinearFileOffsets.Select(y => VirtualPosition.From((long)y).FileOffset))).ToArray();

        public static IEnumerable<T> MergeConsecutiveEqualValues<T>(IEnumerable<T> values)
        {
            var isFirstValue = true;
            T lastValue = default;
            foreach (var value in values)
            {
                if (!isFirstValue && lastValue.Equals(value)) continue;

                isFirstValue = false;
                lastValue = value;
                yield return value;
            }
        }

        public static IEnumerable<AnnotationRange> GenerateAnnotationRanges(List<long> blockBasedOffsets, string vcfUrl, IntervalForest<IGene> geneIntervalForest,
            IDictionary<string, IChromosome> refNameToChromosome)
        {
            // There may be less intervals for annotation Lambda after the adjustment
            AnnotationPosition[] adjustedStarts = AdjustPartitionGenomicStarts(blockBasedOffsets, vcfUrl, geneIntervalForest, refNameToChromosome);

            return GetRanges(adjustedStarts);
        }


        private static AnnotationPosition[] AdjustPartitionGenomicStarts(IReadOnlyList<long> blockBasedOffsets, string vcfUrl,
            IIntervalForest<IGene> geneIntervalForest, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var allAdjustedStarts = new AnnotationPosition[blockBasedOffsets.Count];

            for (var index = 0; index < blockBasedOffsets.Count; index++)
            {
                long blockBasedOffset = blockBasedOffsets[index];

                using (var stream     = PersistentStreamUtils.GetReadStream(vcfUrl, blockBasedOffset))
                using (var gzipStream = new BlockGZipStream(stream, CompressionMode.Decompress))
                {
                    var annotationPosition   = GetFirstGenomicPosition(gzipStream, index == 0);
                    allAdjustedStarts[index] = FindProperStartPosition(annotationPosition, geneIntervalForest, refNameToChromosome);
                }
            }

            AnnotationPosition[] adjustedStarts = MergeConsecutiveEqualValues(allAdjustedStarts).ToArray();
            return adjustedStarts;
        }

        private static IEnumerable<AnnotationRange> GetRanges(IList<AnnotationPosition> adjustedStarts)
        {
            for (var i = 0; i < adjustedStarts.Count - 1; i++)
                //The end position in an annotation range can be smaller than 1, which indicate it ends at the end of previous chromosome
                yield return new AnnotationRange(adjustedStarts[i], new AnnotationPosition(adjustedStarts[i + 1].Chromosome, adjustedStarts[i + 1].Position - 1));
            
            yield return new AnnotationRange(adjustedStarts[adjustedStarts.Count - 1], null);
        }


        private static AnnotationPosition GetFirstGenomicPosition(Stream vcfStream, bool isFirstBlock)
        {
            if (vcfStream == null) throw new ArgumentNullException(nameof(vcfStream),"The VCF stream trying to read is null.");

            using (var streamReader = new StreamReader(vcfStream))
            {
                // Discard the first line if this is not the first block, as it may be a partial VCF line
                if (!isFirstBlock) streamReader.ReadLine();

                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.StartsWith('#')) continue;
                    string[] splits = line.Split('\t', 3);
                    if (splits.Length < 3) continue;
                    string chrom = splits[VcfCommon.ChromIndex];
                    string positionString = splits[VcfCommon.PosIndex];
                    if (!int.TryParse(positionString, out int position)) throw new InvalidDataException($"Position {positionString} in VCF line {line} is not a number.");

                    return new AnnotationPosition(chrom, position);
                }

                throw new InvalidDataException("No variant found in the VCF stream.");
            }
        }

        private static AnnotationPosition FindProperStartPosition(AnnotationPosition genomicPosition, IIntervalForest<IGene> geneIntervalForest, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var chromosome = ReferenceNameUtilities.GetChromosome(refNameToChromosome, genomicPosition.Chromosome);

            int currentPosition = genomicPosition.Position;
            IGene[] overlappingGenes;
            while ((overlappingGenes = geneIntervalForest.GetAllOverlappingValues(chromosome.Index,
                       currentPosition, currentPosition)) != null)
            {
                if (overlappingGenes.Length > 0) currentPosition = overlappingGenes.Select(x => x.Start).Min() - 1;
            }

            // Always return the position right before the overlapping genes to KISS
            return new AnnotationPosition(genomicPosition.Chromosome, currentPosition < 1 ? 1 : currentPosition);
        }
    }
}