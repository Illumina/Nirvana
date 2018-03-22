using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.Utilities;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.TranscriptCache
{
    public sealed class TranscriptCacheBuilder
    {
        private readonly ILogger _logger;
        private readonly GenomeAssembly _genomeAssembly;
        private readonly Source _source;
        private readonly long _vepReleaseTicks;
        private readonly ushort _vepVersion;

        public TranscriptCacheBuilder(ILogger logger, GenomeAssembly genomeAssembly, Source source,
            long vepReleaseTicks, ushort vepVersion)
        {
            _logger          = logger;
            _genomeAssembly  = genomeAssembly;
            _source          = source;
            _vepReleaseTicks = vepReleaseTicks;
            _vepVersion      = vepVersion;
        }

        public TranscriptCacheStaging CreateTranscriptCache(MutableTranscript[] mutableTranscripts,
            IEnumerable<IRegulatoryRegion> regulatoryRegions, IIntervalForest<UgaGene> geneForest, int numRefSeqs)
        {
            _logger.Write("- assigning UGA genes to transcripts... ");
            AssignUgaGenesToTranscripts(mutableTranscripts, geneForest);
            _logger.WriteLine("finished.");

            var transcriptIntervalArrays       = mutableTranscripts.ToTranscripts().ToIntervalArrays(numRefSeqs);
            var regulatoryRegionIntervalArrays = regulatoryRegions.ToIntervalArrays(numRefSeqs);

            var customHeader = new TranscriptCacheCustomHeader(_vepVersion, _vepReleaseTicks);
            var header = new CacheHeader(CacheConstants.Identifier, CacheConstants.SchemaVersion,
                CacheConstants.DataVersion, _source, DateTime.Now.Ticks, _genomeAssembly, customHeader);

            return TranscriptCacheStaging.GetStaging(header, transcriptIntervalArrays, regulatoryRegionIntervalArrays);
        }

        private void AssignUgaGenesToTranscripts(IEnumerable<MutableTranscript> transcripts, IIntervalForest<UgaGene> geneForest)
        {
            foreach (var transcript in transcripts)
            {
                var originalGene = transcript.Gene;
                var ugaGenes     = geneForest.GetAllOverlappingValues(originalGene.Chromosome.Index, originalGene.Start, originalGene.End);

                if (ugaGenes == null)
                {
                    var strand = originalGene.OnReverseStrand ? "R" : "F";
                    throw new InvalidDataException($"Found a transcript ({transcript.Id}) that does not have an overlapping UGA gene: gene ID: {originalGene.GeneId} {originalGene.Chromosome.UcscName} {originalGene.Start} {originalGene.End} {strand}");
                }

                transcript.UpdatedGene = PickGeneById(ugaGenes, originalGene.GeneId).ToGene(_genomeAssembly);
            }
        }

        private UgaGene PickGeneById(IReadOnlyList<UgaGene> genes, string geneId)
        {
            if (genes.Count == 1) return genes[0];

            var genesById = genes.GetMultiValueDict(x => _source == Source.Ensembl ? x.EnsemblId : x.EntrezGeneId);
            if (!genesById.TryGetValue(geneId, out var idGenes)) throw new InvalidDataException($"Could not find {geneId} in the UGA genes list.");

            if (idGenes.Count == 1) return idGenes[0];
            throw new InvalidDataException($"Found multiple entries for {geneId} in the UGA genes list.");
        }
    }
}
