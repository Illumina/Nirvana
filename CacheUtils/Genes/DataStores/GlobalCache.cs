using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes.Utilities;
using CacheUtils.IntermediateIO;
using Compression.Utilities;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.Genes.DataStores
{
    public sealed class GlobalCache
    {
        public readonly Dictionary<ushort, List<MutableGene>> EnsemblGenesByRef;
        public readonly Dictionary<ushort, List<MutableGene>> RefSeqGenesByRef;

        private GlobalCache(Dictionary<ushort, List<MutableGene>> ensemblGenesByRef,
            Dictionary<ushort, List<MutableGene>> refSeqGenesByRef)
        {
            EnsemblGenesByRef = ensemblGenesByRef;
            RefSeqGenesByRef  = refSeqGenesByRef;
        }

        public static GlobalCache Create(string refSeqCachePath, string ensemblCachePath,
            IDictionary<ushort, IChromosome> refIndexToChromosome, IDictionary<string, IChromosome> refNameToChromosome38)
        {
            var ensemblGenesByRef = FlattenGenes(LoadGenes(GZipUtilities.GetAppropriateReadStream(ensemblCachePath), refIndexToChromosome, refNameToChromosome38));
            var refSeqGenesByRef  = FlattenGenes(LoadGenes(GZipUtilities.GetAppropriateReadStream(refSeqCachePath),  refIndexToChromosome, refNameToChromosome38));

            return new GlobalCache(ensemblGenesByRef, refSeqGenesByRef);
        }

        private static Dictionary<ushort, List<MutableGene>> FlattenGenes(IEnumerable<MutableGene> genes)
        {
            var genesByRef = genes.GetMultiValueDict(x => x.Chromosome.Index);
            var result     = new Dictionary<ushort, List<MutableGene>>();

            foreach (var kvp in genesByRef.OrderBy(x => x.Key))
            {
                result[kvp.Key] = kvp.Value.GetMultiValueDict(x => x.GeneId).FlattenGeneList();
            }

            return result;
        }

        private static IEnumerable<MutableGene> LoadGenes(Stream stream,
            IDictionary<ushort, IChromosome> refIndexToChromosome,
            IDictionary<string, IChromosome> refNameToChromosome38)
        {
            var geneDict = new Dictionary<string, MutableGene>();

            using (var reader = new MutableTranscriptReader(stream, refIndexToChromosome))
            {
                var transcripts = reader.GetTranscripts();

                foreach (var transcript in transcripts)
                {
                    var gene = transcript.Gene;
                    var key  = GetGeneKey(gene);
                    if (geneDict.ContainsKey(key)) continue;

                    gene.Chromosome = refNameToChromosome38[gene.Chromosome.UcscName];
                    geneDict[key] = gene;
                }
            }

            return geneDict.Values.OrderBy(x => x.Chromosome.Index).ThenBy(x => x.Start).ThenBy(x => x.End);
        }

        private static string GetGeneKey(MutableGene gene) => gene.GeneId + '|' + gene.Chromosome.UcscName + '|' +
                                                              gene.Start + '|' + gene.End + '|' +
                                                              (gene.OnReverseStrand ? 'R' : 'F');
    }
}
