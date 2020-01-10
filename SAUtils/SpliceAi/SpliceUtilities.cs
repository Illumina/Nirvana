using System.Collections.Generic;
using System.IO;
using System.Linq;
using Intervals;
using OptimizedCore;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.SpliceAi
{
    public static class SpliceUtilities
    {
        public const int SpliceFlankLength = 15;
        public static Dictionary<ushort, IntervalArray<byte>> GetSpliceIntervals(ISequenceProvider sequenceProvider, TranscriptCacheData transcriptData)
        {
            var cache = transcriptData.GetCache();

            var spliceIntervalDict = new Dictionary<ushort, IntervalArray<byte>>(sequenceProvider.RefIndexToChromosome.Count);

            foreach (var chromIndex in sequenceProvider.RefIndexToChromosome.Keys)
            {
                var spliceIntervals = new List<Interval<byte>>(8 * 1024);
                var overlappingTranscripts =
                    cache.TranscriptIntervalForest.GetAllOverlappingValues(chromIndex, 1, int.MaxValue);

                if (overlappingTranscripts == null) continue;

                foreach (var transcript in overlappingTranscripts)
                {
                    if (transcript.Id.IsPredictedTranscript()) continue;
                    bool isFirstExon = true;
                    foreach (var transcriptRegion in transcript.TranscriptRegions)
                    {
                        if (transcriptRegion.Type != TranscriptRegionType.Exon) continue;
                        var firstSplicePosition = transcriptRegion.Start;
                        var secondSplicePosition = transcriptRegion.End;

                        var firstInterval = new Interval<byte>(firstSplicePosition - SpliceFlankLength, firstSplicePosition + SpliceFlankLength, 0);
                        var secondInterval = new Interval<byte>(secondSplicePosition - SpliceFlankLength, secondSplicePosition + SpliceFlankLength, 0);

                        if(!isFirstExon) spliceIntervals.Add(firstInterval);
                        spliceIntervals.Add(secondInterval);
                        isFirstExon = false;
                    }
                    //remove the last added interval since this is the tail of the last exon- which is not a splice site
                    if(spliceIntervals.Count > 0)spliceIntervals.RemoveAt(spliceIntervals.Count - 1);

                }

                spliceIntervalDict[chromIndex] = new IntervalArray<byte>(spliceIntervals.OrderBy(x => x.Begin).ThenBy(x => x.End).ToArray());
            }

            return spliceIntervalDict;
        }

        public static Dictionary<string, string> GetEnstToGeneSymbols(ISequenceProvider sequenceProvider, TranscriptCacheData transcriptData)
        {
            var cache = transcriptData.GetCache();
            var enstToGeneSymbols = new Dictionary<string, string>();

            foreach (var chromIndex in sequenceProvider.RefIndexToChromosome.Keys)
            {
                var overlappingTranscripts =
                    cache.TranscriptIntervalForest.GetAllOverlappingValues(chromIndex, 1, int.MaxValue);

                if (overlappingTranscripts == null) continue;

                foreach (var transcript in overlappingTranscripts)
                {
                    if (transcript.Id.WithoutVersion.StartsWith("ENST"))
                        enstToGeneSymbols[transcript.Id.WithoutVersion] = transcript.Gene.Symbol;
                }

            }

            return enstToGeneSymbols;
        }

        public static Dictionary<string, string> GetSpliceAiGeneSymbols(StreamReader reader)
        {
            var enstToGeneSymbols = new Dictionary<string, string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var splits = line.OptimizedSplit('\t');
                var geneSymbol = splits[0];
                var ensemblId = splits[1].OptimizedSplit('.')[0];

                enstToGeneSymbols[ensemblId] = geneSymbol;
            }

            return enstToGeneSymbols;
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

        public static Dictionary<string, string> GetSymbolMapping(Dictionary<string, string> spliceAiEnstToGeneSymbols, Dictionary<string, string> nirEnstToGeneSymbols)
        {
            var spliceToNirSymbols= new Dictionary<string, string>();
            foreach (var (spliceEnst, spliceGene) in spliceAiEnstToGeneSymbols)
            {
                if (nirEnstToGeneSymbols.TryGetValue(spliceEnst, out var nirGene))
                    spliceToNirSymbols[spliceGene] = nirGene;
            }

            return spliceToNirSymbols;
        }
    }
}