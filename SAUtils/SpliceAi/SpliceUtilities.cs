using System.Collections.Generic;
using System.Linq;
using Intervals;
using OptimizedCore;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using Variants;

namespace SAUtils.SpliceAi
{
    public static class SpliceUtilities
    {
        public const int SpliceFlankLength = 15;
        public static Dictionary<ushort, IntervalArray<byte>> GetSpliceIntervals(ISequenceProvider sequenceProvider, TranscriptCacheData transcriptData)
        {
            var cache = transcriptData.GetCache();

            var spliceIntervals = new Dictionary<ushort, IntervalArray<byte>>(sequenceProvider.RefIndexToChromosome.Count);

            foreach (var chromIndex in sequenceProvider.RefIndexToChromosome.Keys)
            {
                var spliceInterval = new List<Interval<byte>>(8 * 1024);
                var overlappingTranscripts =
                    cache.TranscriptIntervalForest.GetAllOverlappingValues(chromIndex, 1, int.MaxValue);

                if (overlappingTranscripts == null) continue;

                foreach (var transcript in overlappingTranscripts)
                {
                    if (transcript.Id.IsPredictedTranscript()) continue;
                    foreach (var transcriptRegion in transcript.TranscriptRegions)
                    {
                        if (transcriptRegion.Type != TranscriptRegionType.Exon) continue;
                        var firstSplicePosition = transcriptRegion.Start;
                        var secondSplicePosition = transcriptRegion.End;

                        var firstInterval = new Interval<byte>(firstSplicePosition - SpliceFlankLength, firstSplicePosition + SpliceFlankLength, 0);
                        var secondInterval = new Interval<byte>(secondSplicePosition - SpliceFlankLength, secondSplicePosition + SpliceFlankLength, 0);

                        spliceInterval.Add(firstInterval);
                        spliceInterval.Add(secondInterval);
                    }
                }

                spliceIntervals[chromIndex] = new IntervalArray<byte>(spliceInterval.OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray());
            }

            return spliceIntervals;
        }

        public static IntervalForest<string> GetGeneForest(TranscriptCacheData transcriptData)
        {
            var geneDictionary = new Dictionary<ushort, List<Interval<string>>> ();
            foreach (var gene in transcriptData.Genes)
            {
                if (!geneDictionary.ContainsKey(gene.Chromosome.Index))
                    geneDictionary[gene.Chromosome.Index] = new List<Interval<string>>();

                geneDictionary[gene.Chromosome.Index].Add(new Interval<string>(gene.Start, gene.End, gene.Symbol));
            }
            var geneIntervalArrays = new IntervalArray<string>[geneDictionary.Keys.Max()+1];
            foreach (var (index, geneIntervals) in geneDictionary)
            {
                geneIntervalArrays[index] = new IntervalArray<string>( geneIntervals.OrderBy(x=>x.Begin).ThenBy(x=>x.End).ToArray());
            }

            return new IntervalForest<string>(geneIntervalArrays);
        }

        //get gene boundaries from spliceAI input vcf and create an interval forest

        private static (ushort chormIndex, int position, string geneName) GetGenePosition(string vcfLine, ISequenceProvider sequenceProvider)
        {
            var splitLine = vcfLine.Split('\t');
            if (splitLine.Length < VcfCommon.InfoIndex + 1) return (ushort.MaxValue, -1, null);

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!sequenceProvider.RefNameToChromosome.ContainsKey(chromosomeName)) return (ushort.MaxValue, -1, null);

            var chromosome = sequenceProvider.RefNameToChromosome[chromosomeName];
            var position = int.Parse(splitLine[VcfCommon.PosIndex]);
            var geneSymbol = GetGeneSymbol(splitLine[VcfCommon.InfoIndex]);

            return (chromosome.Index, position, geneSymbol);

        }

        private static string GetGeneSymbol(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return null;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (var infoItem in infoItems)
            {
                var (key, value) = infoItem.OptimizedKeyValue();
                // sanity check
                if (key == "SYMBOL") return value;
            }

            return null;
        }
    }
}