using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.CombineAndUpdateGenes.Algorithms;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using CacheUtils.CombineAndUpdateGenes.FileHandling;
using CacheUtils.DataDumperImport.FileHandling;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;

namespace CacheUtils.CombineAndUpdateGenes
{
    public sealed class GeneCombiner
    {
        #region members

        private readonly SymbolDataSource _geneInfoSource;
        private readonly SymbolDataSource _hgncSource;

        private readonly List<MutableGene> _mergedGenes;
        private readonly Dictionary<string, GeneInfo> _refSeqGff3GeneInfo;

        private GlobalImportHeader _header;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public GeneCombiner(string inputGenesPath, string inputGenes2Path, List<string> geneInfoPaths, string hgncPath,
            string refSeqGff3Path)
        {
            _geneInfoSource = ParseGeneInfoFiles(geneInfoPaths);

            var entrezGeneIdToEnsemblId = new Dictionary<string, UniqueString>();
            var ensemblIdToEntrezGeneId = new Dictionary<string, UniqueString>();

            _hgncSource = ParseHgncFile(hgncPath, entrezGeneIdToEnsemblId, ensemblIdToEntrezGeneId);

            Console.WriteLine();
            Console.WriteLine("- linking Ensembl and Entrez gene IDs: ");

            var linkedEnsemblIds = LinkIds(entrezGeneIdToEnsemblId, ensemblIdToEntrezGeneId);

            Console.WriteLine();
            Console.WriteLine("- loading RefSeq GFF3: ");

            _refSeqGff3GeneInfo = GetRefSeqGff3GeneInfo(refSeqGff3Path);

            Console.WriteLine();
            Console.WriteLine("- loading genes: ");

            string descriptionA = Path.GetFileName(inputGenesPath);
            string descriptionB = Path.GetFileName(inputGenes2Path);

            var genesA = LoadGenes(inputGenesPath, descriptionA);
            var genesB = LoadGenes(inputGenes2Path, descriptionB);

            Console.WriteLine();
            Console.WriteLine("- update gene symbols: ");

            var updaterA = new GeneSymbolUpdater(genesA, descriptionA, _geneInfoSource, _hgncSource);
            updaterA.Update();

            var updaterB = new GeneSymbolUpdater(genesB, descriptionB, _geneInfoSource, _hgncSource);
            updaterB.Update();

            Console.WriteLine();
            Console.WriteLine("- flattening genes: ");

            var flattenerA = new GeneFlattener(genesA, descriptionA);
            var flatGenesA = flattenerA.Flatten();

            var flattenerB = new GeneFlattener(genesB, descriptionB);
            var flatGenesB = flattenerB.Flatten();

            Console.WriteLine();
            Console.WriteLine("- merging Ensembl and RefSeq:");

            var merger = new GeneMerger(flatGenesA, flatGenesB, linkedEnsemblIds);
            _mergedGenes = merger.Merge();

            Console.WriteLine();
            Console.WriteLine("- update HGNC ids:");

            UpdateHgncIds(_mergedGenes);
        }

        private void UpdateHgncIds(List<MutableGene> genes)
        {
            int numHgncIdsUpdated        = 0;
            int numHgncIdsAlreadyCurrent = 0;
            int numHgncIdsUnableToUpdate = 0;

            foreach (var gene in genes)
            {
                if (gene.HgncId != -1)
                {
                    numHgncIdsAlreadyCurrent++;
                    continue;
                }

                var ensemblId    = gene.EnsemblId.IsEmpty    ? null : gene.EnsemblId.ToString();
                var entrezGeneId = gene.EntrezGeneId.IsEmpty ? null : gene.EntrezGeneId.ToString();

                if (_hgncSource.TryUpdateHgncId(ensemblId, entrezGeneId, gene))
                {
                    numHgncIdsUpdated++;
                    continue;
                }

                if (_geneInfoSource.TryUpdateHgncId(ensemblId, entrezGeneId, gene))
                {
                    numHgncIdsUpdated++;
                    continue;
                }

                numHgncIdsUnableToUpdate++;
            }

            Console.WriteLine($"  - {numHgncIdsAlreadyCurrent} already current.");
            Console.WriteLine($"  - {numHgncIdsUpdated} updated.");
            Console.WriteLine($"  - {numHgncIdsUnableToUpdate} unable to update.");
        }

        private static Dictionary<string, string> LinkIds(Dictionary<string, UniqueString> entrezGeneIdToEnsemblId,
            Dictionary<string, UniqueString> ensemblIdToEntrezGeneId)
        {
            var linkedEnsemblIds = new Dictionary<string, string>();

            foreach (var geneId in entrezGeneIdToEnsemblId.Keys)
            {
                // forward lookup
                UniqueString ensemblId;
                if (!entrezGeneIdToEnsemblId.TryGetValue(geneId, out ensemblId)) continue;
                if (ensemblId.HasConflict) continue;

                // reciprocal lookup
                UniqueString entrezGeneId;
                if (!ensemblIdToEntrezGeneId.TryGetValue(ensemblId.Value, out entrezGeneId)) continue;
                if (entrezGeneId.HasConflict) continue;

                // link the reciprocally unique IDs together
                linkedEnsemblIds[ensemblId.Value] = entrezGeneId.Value;
            }

            Console.WriteLine($"  - {linkedEnsemblIds.Count} gene ID pairs were linked.");

            return linkedEnsemblIds;
        }

        private static SymbolDataSource ParseHgncFile(string hgncPath,
            Dictionary<string, UniqueString> entrezGeneIdToEnsemblId,
            Dictionary<string, UniqueString> ensemblIdToEntrezGeneId)
        {
            Console.WriteLine();
            Console.WriteLine("- loading HGNC file:");

            var entrezGeneIdToSymbol = new Dictionary<string, UniqueString>();
            var ensemblIdToSymbol    = new Dictionary<string, UniqueString>();
            var entrezGeneIdToHgncId = new Dictionary<string, UniqueInt>();
            var ensemblIdToHgncId    = new Dictionary<string, UniqueInt>();

            int numEntries = 0;

            using (var reader = new HgncReader(hgncPath))
            {
                while (true)
                {
                    var geneinfo = reader.Next();
                    if (geneinfo == null) break;
                    if (geneinfo.IsEmpty) continue;

                    numEntries++;

                    bool hasEntrezGeneId = !string.IsNullOrEmpty(geneinfo.EntrezGeneId);
                    bool hasEnsemblId    = !string.IsNullOrEmpty(geneinfo.EnsemblId);
                    bool hasSymbol       = !string.IsNullOrEmpty(geneinfo.Symbol);
                    bool hasHgncId       = geneinfo.HgncId != -1;

                    if (hasSymbol)
                    {
                        if (hasEntrezGeneId) AddIdToUniqueString(entrezGeneIdToSymbol, geneinfo.EntrezGeneId, geneinfo.Symbol);
                        if (hasEnsemblId) AddIdToUniqueString(ensemblIdToSymbol, geneinfo.EnsemblId, geneinfo.Symbol);
                    }

                    if (hasHgncId)
                    {
                        if (hasEntrezGeneId) AddIdToHgncId(entrezGeneIdToHgncId, geneinfo.EntrezGeneId, geneinfo.HgncId);
                        if (hasEnsemblId) AddIdToHgncId(ensemblIdToHgncId, geneinfo.EnsemblId, geneinfo.HgncId);
                    }

                    if (hasEnsemblId && hasEntrezGeneId)
                    {
                        AddIdToUniqueString(ensemblIdToEntrezGeneId, geneinfo.EnsemblId, geneinfo.EntrezGeneId);
                        AddIdToUniqueString(entrezGeneIdToEnsemblId, geneinfo.EntrezGeneId, geneinfo.EnsemblId);
                    }
                }
            }

            Console.WriteLine($"  - {numEntries} entries loaded.");

            Console.WriteLine($"  - Entrez Gene ID -> symbol:  {entrezGeneIdToSymbol.Count} ({GetNonConflictCount(entrezGeneIdToSymbol)})");
            Console.WriteLine($"  - Ensembl ID -> symbol:      {ensemblIdToSymbol.Count} ({GetNonConflictCount(ensemblIdToSymbol)})");
            Console.WriteLine($"  - Entrez Gene ID -> HGNC id: {entrezGeneIdToHgncId.Count} ({GetNonConflictCount(entrezGeneIdToHgncId)})");
            Console.WriteLine($"  - Ensembl ID -> HGNC id:     {ensemblIdToHgncId.Count} ({GetNonConflictCount(ensemblIdToHgncId)})");

            return new SymbolDataSource(entrezGeneIdToSymbol, ensemblIdToSymbol, entrezGeneIdToHgncId, ensemblIdToHgncId);
        }

        private static SymbolDataSource ParseGeneInfoFiles(List<string> geneInfoPaths)
        {
            Console.WriteLine("- loading gene_info files:");

            var entrezGeneIdToSymbol = new Dictionary<string, UniqueString>();
            var ensemblIdToSymbol    = new Dictionary<string, UniqueString>();
            var entrezGeneIdToHgncId = new Dictionary<string, UniqueInt>();
            var ensemblIdToHgncId    = new Dictionary<string, UniqueInt>();

            foreach (var geneInfoPath in geneInfoPaths)
            {
                Console.Write("  - {0}... ", Path.GetFileName(geneInfoPath));
                int numEntries = 0;

                using (var reader = new GeneInfoReader(geneInfoPath))
                {
                    while (true)
                    {
                        var geneinfo = reader.Next();
                        if (geneinfo == null) break;
                        if (geneinfo.IsEmpty) continue;

                        numEntries++;

                        bool hasEntrezGeneId = !string.IsNullOrEmpty(geneinfo.EntrezGeneId);
                        bool hasEnsemblId    = !string.IsNullOrEmpty(geneinfo.EnsemblId);
                        bool hasSymbol       = !string.IsNullOrEmpty(geneinfo.Symbol);
                        bool hasHgncId       = geneinfo.HgncId != -1;

                        if (hasSymbol)
                        {
                            if (hasEntrezGeneId) AddIdToUniqueString(entrezGeneIdToSymbol, geneinfo.EntrezGeneId, geneinfo.Symbol);
                            if (hasEnsemblId) AddIdToUniqueString(ensemblIdToSymbol, geneinfo.EnsemblId, geneinfo.Symbol);
                        }

                        if (hasHgncId)
                        {
                            if (hasEntrezGeneId) AddIdToHgncId(entrezGeneIdToHgncId, geneinfo.EntrezGeneId, geneinfo.HgncId);
                            if (hasEnsemblId) AddIdToHgncId(ensemblIdToHgncId, geneinfo.EnsemblId, geneinfo.HgncId);
                        }
                    }
                }

                Console.WriteLine($"{numEntries} entries loaded.");
            }

            Console.WriteLine($"  - Entrez Gene ID -> symbol:  {entrezGeneIdToSymbol.Count} ({GetNonConflictCount(entrezGeneIdToSymbol)})");
            Console.WriteLine($"  - Ensembl ID -> symbol:      {ensemblIdToSymbol.Count} ({GetNonConflictCount(ensemblIdToSymbol)})");
            Console.WriteLine($"  - Entrez Gene ID -> HGNC id: {entrezGeneIdToHgncId.Count} ({GetNonConflictCount(entrezGeneIdToHgncId)})");
            Console.WriteLine($"  - Ensembl ID -> HGNC id:     {ensemblIdToHgncId.Count} ({GetNonConflictCount(ensemblIdToHgncId)})");

            return new SymbolDataSource(entrezGeneIdToSymbol, ensemblIdToSymbol, entrezGeneIdToHgncId, ensemblIdToHgncId);
        }

        private static Dictionary<string, GeneInfo> GetRefSeqGff3GeneInfo(string gff3Path)
        {
            Console.WriteLine("- loading RefSeq GFF3 file:");

            var refSeqGff3GeneInfo = new Dictionary<string, GeneInfo>();

            Console.Write("  - {0}... ", Path.GetFileName(gff3Path));
            int numEntries = 0;

            using (var reader = new Gff3Reader(gff3Path))
            {
                while (true)
                {
                    var geneinfo = reader.Next();
                    if (geneinfo == null) break;
                    if (geneinfo.IsEmpty) continue;

                    numEntries++;

                    GeneInfo oldGeneinfo;
                    if (refSeqGff3GeneInfo.TryGetValue(geneinfo.EntrezGeneId, out oldGeneinfo))
                    {
                        if (geneinfo.HgncId != oldGeneinfo.HgncId || geneinfo.Symbol != oldGeneinfo.Symbol)
                        {
                            throw new UserErrorException($"Duplicate Entrez gene ID ({geneinfo.EntrezGeneId}) was added to the dictionary.");
                        }                        
                    }

                    refSeqGff3GeneInfo[geneinfo.EntrezGeneId] = geneinfo;
                }
            }

            Console.WriteLine($"{numEntries} entries loaded.");

            return refSeqGff3GeneInfo;
        }

        private static int GetNonConflictCount(Dictionary<string, UniqueString> dict)
        {
            return dict.Values.Count(x => !x.HasConflict);
        }

        private static int GetNonConflictCount(Dictionary<string, UniqueInt> dict)
        {
            return dict.Values.Count(x => !x.HasConflict);
        }

        private static void AddIdToUniqueString(Dictionary<string, UniqueString> idToUniqueString, string id, string newValue)
        {
            UniqueString oldValue;
            if (idToUniqueString.TryGetValue(id, out oldValue))
            {
                if (oldValue.Value != newValue) oldValue.HasConflict = true;
            }
            else
            {
                idToUniqueString[id] = new UniqueString { Value = newValue };
            }
        }

        private static void AddIdToHgncId(Dictionary<string, UniqueInt> idToHgncId, string id, int hgnc)
        {
            UniqueInt oldHgnc;
            if (idToHgncId.TryGetValue(id, out oldHgnc))
            {
                if (oldHgnc.Value != hgnc) oldHgnc.HasConflict = true;
            }
            else
            {
                idToHgncId[id] = new UniqueInt { Value = hgnc };
            }
        }

        /// <summary>
        /// loads all the genes in the specified file
        /// </summary>
        private List<MutableGene> LoadGenes(string genesPath, string description)
        {
            var genes = new List<MutableGene>();

            using (var reader = new VepGeneReader(genesPath))
            {
                if (_header == null) _header = reader.Header;

                while (true)
                {
                    var gene = reader.Next();
                    if (gene == null) break;

                    genes.Add(gene);
                }
            }

            var transcriptDataSource = genes.First().TranscriptDataSource;
            int numGenesWithoutSymbol = GetNumGenesWithoutSymbol(description, genes);
            if (numGenesWithoutSymbol > 0 && transcriptDataSource == TranscriptDataSource.RefSeq) ResolveMissingRefSeqGeneSymbols(description, genes);

            return genes;
        }

        private void ResolveMissingRefSeqGeneSymbols(string description, List<MutableGene> genes)
        {
            Console.WriteLine($"  - attempting to resolve missing gene symbols in {description}:");

            foreach (var gene in genes)
            {
                if (gene.EntrezGeneId.IsEmpty) throw new UserErrorException("Found an empty Entrez gene ID when resolving RefSeq gene symbols.");

                GeneInfo geneInfo;
                if (!_refSeqGff3GeneInfo.TryGetValue(gene.EntrezGeneId.ToString(), out geneInfo)) continue;

                gene.HgncId = geneInfo.HgncId;
                gene.Symbol = geneInfo.Symbol;
            }

            int numGenesWithoutSymbol = GetNumGenesWithoutSymbol(description, genes);
            if (numGenesWithoutSymbol > 0) throw new UserErrorException("Unable to resolve all the missing gene symbols");
        }

        public void Write(string outputPath)
        {
            Console.WriteLine();
            Console.WriteLine("- serializing genes:");

            using (var writer = GZipUtilities.GetStreamWriter(outputPath))
            {
                writer.NewLine = "\n";
                WriteHeader(writer, _header);

                int geneIndex = 0;

                foreach (var gene in _mergedGenes.OrderBy(g => g.ReferenceIndex).ThenBy(g => g.Start).ThenBy(g => g.End).ThenBy(g => g.Symbol))
                {
                    writer.WriteLine($"{geneIndex}\t{gene}");
                    geneIndex++;
                }

                Console.WriteLine("  - {0} genes written.", _mergedGenes.Count);
            }
        }

        /// <summary>
        /// writes the header to our output file
        /// </summary>
        private static void WriteHeader(StreamWriter writer, GlobalImportHeader header)
        {
            writer.WriteLine("{0}\t{1}", GlobalImportCommon.Header, (byte)GlobalImportCommon.FileType.Gene);
            writer.WriteLine("{0}\t{1}\t{2}\t{3}", header.VepVersion, header.VepReleaseTicks,
                (byte)TranscriptDataSource.BothRefSeqAndEnsembl, (byte) header.GenomeAssembly);
        }

        private static int GetNumGenesWithoutSymbol(string description, List<MutableGene> genes)
        {
            int numGenesWithoutSymbol = genes.Count(gene => string.IsNullOrEmpty(gene.Symbol));
            Console.WriteLine($"  - {description}: # entries without gene symbol: {numGenesWithoutSymbol} / {genes.Count}");
            return numGenesWithoutSymbol;
        }
    }
}
