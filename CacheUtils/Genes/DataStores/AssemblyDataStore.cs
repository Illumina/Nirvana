using System.Collections.Generic;
using CacheUtils.Commands.Download;
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
            IDictionary<string, IChromosome> accessionToChromosome, bool useGrch37)
        {
            string ensemblGtfPath      = useGrch37 ? ExternalFiles.EnsemblGtfFile37.FilePath      : ExternalFiles.EnsemblGtfFile38.FilePath;
            string refseqGffPath       = useGrch37 ? ExternalFiles.RefSeqGffFile37.FilePath       : ExternalFiles.RefSeqGffFile38.FilePath;
            string refseqGenomeGffPath = useGrch37 ? ExternalFiles.RefSeqGenomeGffFile37.FilePath : ExternalFiles.RefSeqGenomeGffFile38.FilePath;

            var ensemblGtf = EnsemblGtf.Create(ensemblGtfPath, refNameToChromosome);
            var refSeqGff  = RefSeqGff.Create(refseqGffPath, refseqGenomeGffPath, accessionToChromosome);

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
