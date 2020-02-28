using System.Collections.Generic;
using CacheUtils.Commands.Download;
using CacheUtils.Commands.UniversalGeneArchive;
using Genome;
using IO;
using VariantAnnotation.Sequence;

namespace CacheUtils.Genes.DataStores
{
    public sealed class AssemblyDataStore
    {
        private readonly string _description;
        public readonly EnsemblGtf EnsemblGtf;
        public readonly RefSeqGff RefSeqGff;
        private readonly GlobalCache _globalCache;

        private AssemblyDataStore(string description, EnsemblGtf ensemblGtf, RefSeqGff refSeqGff, GlobalCache globalCache)
        {
            _description = description;
            EnsemblGtf   = ensemblGtf;
            RefSeqGff    = refSeqGff;
            _globalCache = globalCache;
        }

        public static AssemblyDataStore Create(string description, FilePaths.AssemblySpecificPaths paths,
            IDictionary<string, IChromosome> refNameToChromosome, bool useGrch37)
        {
            string ensemblGtfPath      = useGrch37 ? ExternalFiles.EnsemblGtfFile37.FilePath      : ExternalFiles.EnsemblGtfFile38.FilePath;
            string refseqGffPath       = useGrch37 ? ExternalFiles.RefSeqGffFile37.FilePath       : ExternalFiles.RefSeqGffFile38.FilePath;
            string refseqGenomeGffPath = useGrch37 ? ExternalFiles.RefSeqGenomeGffFile37.FilePath : ExternalFiles.RefSeqGenomeGffFile38.FilePath;

            var ensemblGtf = EnsemblGtf.Create(ensemblGtfPath, refNameToChromosome);
            var refSeqGff  = RefSeqGff.Create(refseqGffPath, refseqGenomeGffPath, refNameToChromosome);

            var (refIndexToChromosome, _, _) = SequenceHelper.GetDictionaries(paths.ReferencePath);
            var globalCache = GlobalCache.Create(paths.RefSeqCachePath, paths.EnsemblCachePath, refIndexToChromosome, refNameToChromosome);

            return new AssemblyDataStore(description, ensemblGtf, refSeqGff, globalCache);
        }

        public IUpdateHgncData UpdateHgncIds(Hgnc oldHgnc)
        {
            Logger.WriteLine($"\n*** {_description} ***");

            var hgnc = oldHgnc.Clone();

            Logger.Write("- removing duplicate gene IDs from HGNC... ");
            (int numEntrezGeneIdsRemoved, int numEnsemblIdsRemoved) = hgnc.RemoveDuplicateEntries();
            Logger.WriteLine($"{numEntrezGeneIdsRemoved} Entrez Gene, {numEnsemblIdsRemoved} Ensembl.");

            Logger.Write("- adding coordinates to the HGNC entries... ");
            int numEntriesWithCoordinates = hgnc.AddCoordinates(EnsemblGtf, RefSeqGff);
            Logger.WriteLine($"{numEntriesWithCoordinates} with coordinates.");

            Logger.Write("- updating HGNC IDs for RefSeq genes... ");
            int numGenesWithHgncId = hgnc.HgncGenes.Update(_globalCache.RefSeqGenesByRef, x => x.EntrezGeneId).Consolidate();
            Logger.WriteLine($"{numGenesWithHgncId} genes have HGNC ID.");

            Logger.Write("- updating HGNC IDs for Ensembl genes... ");
            numGenesWithHgncId = hgnc.HgncGenes.Update(_globalCache.EnsemblGenesByRef, x => x.EnsemblId).Consolidate();
            Logger.WriteLine($"{numGenesWithHgncId} genes have HGNC ID.");

            return new UpdateHgncData(_globalCache.EnsemblGenesByRef, _globalCache.RefSeqGenesByRef);
        }
    }
}
