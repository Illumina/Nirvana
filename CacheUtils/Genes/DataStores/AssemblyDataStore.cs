using System.Collections.Generic;
using CacheUtils.Commands.UniversalGeneArchive;
using CacheUtils.Helpers;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.Genes.DataStores
{
    public sealed class AssemblyDataStore
    {
        private readonly string _description;
        private readonly ILogger _logger;
        public readonly EnsemblGtf EnsemblGtf;
        public readonly RefSeqGff RefSeqGff;
        private readonly GlobalCache _globalCache;

        private AssemblyDataStore(string description, ILogger logger, EnsemblGtf ensemblGtf, RefSeqGff refSeqGff, GlobalCache globalCache)
        {
            _description = description;
            _logger      = logger;
            EnsemblGtf   = ensemblGtf;
            RefSeqGff    = refSeqGff;
            _globalCache = globalCache;
        }

        public static AssemblyDataStore Create(string description, ILogger logger,
            FilePaths.AssemblySpecificPaths paths, IDictionary<string, IChromosome> refNameToChromosome,
            IDictionary<string, IChromosome> accessionToChromosome)
        {
            var ensemblGtf = EnsemblGtf.Create(paths.EnsemblGtfPath, refNameToChromosome);
            var refSeqGff  = RefSeqGff.Create(paths.RefSeqGffPath, paths.RefSeqGenomeGffPath, accessionToChromosome);

            var (refIndexToChromosome, _, _) = SequenceHelper.GetDictionaries(paths.ReferencePath);
            var globalCache = GlobalCache.Create(paths.RefSeqCachePath, paths.EnsemblCachePath, refIndexToChromosome, refNameToChromosome);

            return new AssemblyDataStore(description, logger, ensemblGtf, refSeqGff, globalCache);
        }

        public IUpdateHgncData UpdateHgncIds(Hgnc oldHgnc)
        {
            _logger.WriteLine();
            _logger.WriteLine($"*** {_description} ***");

            var hgnc = oldHgnc.Clone();

            _logger.Write("- removing duplicate gene IDs from HGNC... ");
            (int numEntrezGeneIdsRemoved, int numEnsemblIdsRemoved) = hgnc.RemoveDuplicateEntries();
            _logger.WriteLine($"{numEntrezGeneIdsRemoved} Entrez Gene, {numEnsemblIdsRemoved} Ensembl.");

            _logger.Write("- adding coordinates to the HGNC entries... ");
            int numEntriesWithCoordinates = hgnc.AddCoordinates(EnsemblGtf, RefSeqGff);
            _logger.WriteLine($"{numEntriesWithCoordinates} with coordinates.");

            _logger.Write("- updating HGNC IDs for RefSeq genes... ");
            int numGenesWithHgncId = hgnc.HgncGenes.Update(_globalCache.RefSeqGenesByRef, x => x.EntrezGeneId).Consolidate();
            _logger.WriteLine($"{numGenesWithHgncId} genes have HGNC ID.");

            _logger.Write("- updating HGNC IDs for Ensembl genes... ");
            numGenesWithHgncId = hgnc.HgncGenes.Update(_globalCache.EnsemblGenesByRef, x => x.EnsemblId).Consolidate();
            _logger.WriteLine($"{numGenesWithHgncId} genes have HGNC ID.");

            return new UpdateHgncData(_globalCache.EnsemblGenesByRef, _globalCache.RefSeqGenesByRef, _logger);
        }
    }
}
