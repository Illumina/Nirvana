using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Utilities;

namespace UpdateOmimGeneSymbols
{
    public class OmimGeneSymbolUpdater
    {
        private readonly SymbolDataSource _geneInfoSource;
        private readonly SymbolDataSource _hgncSource;

        private int _numGenesUnableToUpdate;
        private int _numGenesUpdated;
        private int _numGenesAlreadyCurrent;

        /// <summary>
        /// constructor
        /// </summary>
        public OmimGeneSymbolUpdater(List<string> geneInfoPaths, string hgncPath)
        {
            _geneInfoSource = ParseGeneInfoFiles(geneInfoPaths);
            _hgncSource     = ParseHgncFile(hgncPath);
        }

        public void Update(string inputPath, string outputPath)
        {
            using (var reader = new GeneMap2Reader(FileUtilities.GetReadStream(inputPath)))
            using (var writer = new GeneMap2Writer(FileUtilities.GetCreateStream(outputPath), reader.HeaderLines))
            {
                GeneMap2Entry entry;
                while ((entry = reader.Next()) != null)
                {
                    UpdateGeneSymbols(entry);
                    writer.Write(entry);
                }
            }

            Console.WriteLine($"  - {_numGenesAlreadyCurrent} already current, {_numGenesUpdated} updated, {_numGenesUnableToUpdate} unable to update.");
        }

        private void UpdateGeneSymbols(GeneMap2Entry entry)
        {
            for (int i = 0; i < entry.GeneSymbols.Length; i++)
            {
                var geneSymbol = entry.GeneSymbols[i];
                entry.GeneSymbols[i] = UpdateGeneSymbol(geneSymbol);
            }
        }

        private string UpdateGeneSymbol(string currentSymbol)
        {
            string newSymbol;
            if (_hgncSource.TryUpdateSymbol(currentSymbol, out newSymbol))
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
    }
}
