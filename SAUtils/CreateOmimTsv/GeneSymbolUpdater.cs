using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAUtils.CreateOmimTsv
{
    public sealed class GeneSymbolUpdater
    {
        private readonly SymbolDataSource _geneInfoSource;
        private readonly SymbolDataSource _hgncSource;
        private int _numGenesUnableToUpdate;
        private int _numGenesUpdated;
        private int _numGenesAlreadyCurrent;

        /// <summary>
        /// constructor
        /// </summary>
        public GeneSymbolUpdater(List<string> geneInfoPaths, string hgncPath)
        {
            _geneInfoSource = ParseGeneInfoFiles(geneInfoPaths);
            _hgncSource = ParseHgncFile(hgncPath);
        }

        public string UpdateGeneSymbol(string currentSymbol)
        {
            if (_hgncSource.TryUpdateSymbol(currentSymbol, out var newSymbol))
            {
                if (newSymbol == currentSymbol) _numGenesAlreadyCurrent++;
                else _numGenesUpdated++;
                return newSymbol;
            }

            if (_geneInfoSource.TryUpdateSymbol(currentSymbol, out newSymbol))
            {
                if (newSymbol == currentSymbol) _numGenesAlreadyCurrent++;
                else _numGenesUpdated++;
                return newSymbol;
            }

            _numGenesUnableToUpdate++;
            return currentSymbol;
        }

        private static SymbolDataSource ParseHgncFile(string hgncPath)
        {
            Console.WriteLine();
            Console.WriteLine("- loading HGNC file:");

            var synonymToSymbol = new Dictionary<string, UniqueString>();

            int numEntries = 0;

            using (var reader = new HgncReader(hgncPath))
            {
                while (true)
                {
                    var gs = reader.Next();
                    if (gs == null) break;
                    if (gs.IsEmpty) continue;

                    numEntries++;

                    AddIdToUniqueString(synonymToSymbol, gs.GeneSymbol, gs.GeneSymbol);
                    if (gs.Synonyms.Count <= 0) continue;

                    foreach (var synonym in gs.Synonyms)
                    {
                        AddIdToUniqueString(synonymToSymbol, synonym, gs.GeneSymbol);
                    }
                }
            }

            Console.WriteLine($"  - {numEntries} entries loaded.");

            Console.WriteLine($"  - synonym -> symbol: {synonymToSymbol.Count} ({GetNonConflictCount(synonymToSymbol)})");

            return new SymbolDataSource(synonymToSymbol);
        }

        private static SymbolDataSource ParseGeneInfoFiles(List<string> geneInfoPaths)
        {
            Console.WriteLine("- loading gene_info files:");

            var synonymToSymbol = new Dictionary<string, UniqueString>();

            foreach (var geneInfoPath in geneInfoPaths)
            {
                Console.Write("  - {0}... ", Path.GetFileName(geneInfoPath));
                int numEntries = 0;

                using (var reader = new GeneInfoReader(geneInfoPath))
                {
                    while (true)
                    {
                        var gs = reader.Next();
                        if (gs == null) break;
                        if (gs.IsEmpty) continue;

                        numEntries++;

                        AddIdToUniqueString(synonymToSymbol, gs.GeneSymbol, gs.GeneSymbol);
                        if (gs.Synonyms.Count <= 0) continue;

                        foreach (var synonym in gs.Synonyms)
                        {
                            AddIdToUniqueString(synonymToSymbol, synonym, gs.GeneSymbol);
                        }
                    }
                }

                Console.WriteLine($"{numEntries} entries loaded.");
            }

            Console.WriteLine($"  - synonym -> symbol: {synonymToSymbol.Count} ({GetNonConflictCount(synonymToSymbol)})");

            return new SymbolDataSource(synonymToSymbol);
        }

        private static int GetNonConflictCount(Dictionary<string, UniqueString> dict)
        {
            return dict.Values.Count(x => !x.HasConflict);
        }

        private static void AddIdToUniqueString(Dictionary<string, UniqueString> idToUniqueString, string id, string newValue)
        {
            if (idToUniqueString.TryGetValue(id, out var oldValue))
            {
                if (oldValue.Value != newValue) oldValue.HasConflict = true;
            }
            else
            {
                idToUniqueString[id] = new UniqueString { Value = newValue };
            }
        }
    }
}

