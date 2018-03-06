using System.Collections.Generic;
using System.IO;
using VariantAnnotation.GeneAnnotation;
using System.Linq;
using Compression.Utilities;
using ErrorHandling;
using SAUtils.TsvWriters;
using SAUtils.InputFileParsers;
using VariantAnnotation.Utilities;

namespace SAUtils.CreateOmimTsv
{
    public sealed class OmimTsvCreator
    {
        private const string JsonKeyName = "omim";

        private readonly string _geneMap2Path;
        private readonly string _mim2GenePath;
        private readonly GeneSymbolUpdater _geneSymbolUpdater;
        private readonly string _outputDirectory;

        public OmimTsvCreator(string geneMap2Path, string mim2GenePath, GeneSymbolUpdater geneSymbolUpdater,
            string outputDirectory)
        {
            _geneMap2Path      = geneMap2Path;
            _mim2GenePath      = mim2GenePath;
            _geneSymbolUpdater = geneSymbolUpdater;
            _outputDirectory   = outputDirectory;            
        }

        public ExitCodes Create()
        {
            var mimIdToEntry = new Dictionary<int, OmimImportEntry>();
            AddOmimEntries(mimIdToEntry, _geneMap2Path);
            AddOmimEntries(mimIdToEntry, _mim2GenePath);

            UpdateGeneSymbols(mimIdToEntry);

            var geneToOmimEntries = GetGeneToOmimEntries(mimIdToEntry);
            var dataSourceVersion = DataSourceVersionReader.GetSourceVersion(_geneMap2Path + ".version");

            using (var omimWriter = new GeneAnnotationTsvWriter(_outputDirectory, dataSourceVersion, null, 0, JsonKeyName, true))
            {
                foreach (var kvp in geneToOmimEntries.OrderBy(x => x.Key))
                {
                    omimWriter.AddEntry(kvp.Key,
                        kvp.Value.OrderBy(x => x.MimNumber).Select(x => x.ToString()).ToList());
                }
            }

            _geneSymbolUpdater.DisplayStatistics();

            WriteUpdatedGeneSymbols();

            return ExitCodes.Success;
        }

        private void WriteUpdatedGeneSymbols()
        {
            string updatedGeneSymbolsPath = Path.Combine(_outputDirectory, "updatedGeneSymbols.txt");

            using (var writer = new StreamWriter(FileUtilities.GetCreateStream(updatedGeneSymbolsPath)))
            {
                _geneSymbolUpdater.WriteUpdatedGeneSymbols(writer);
            }
        }

        private Dictionary<string, List<OmimEntry>> GetGeneToOmimEntries(Dictionary<int, OmimImportEntry> mimIdToEntry)
        {
            var geneToOmimEntries = new Dictionary<string, List<OmimEntry>>();

            foreach (var entry in mimIdToEntry.Values)
            {
                if (entry.GeneSymbol == null) continue;
                var omimEntry = entry.ToOmimEntry();

                if (geneToOmimEntries.TryGetValue(entry.GeneSymbol, out var mimList))
                {
                    mimList.Add(omimEntry);
                }
                else
                {
                    geneToOmimEntries[entry.GeneSymbol] = new List<OmimEntry> { omimEntry };
                }
            }

            return geneToOmimEntries;
        }

        private void UpdateGeneSymbols(Dictionary<int, OmimImportEntry> mimIdToEntry)
        {
            foreach (var entry in mimIdToEntry.Values)
            {
                entry.GeneSymbol = _geneSymbolUpdater.UpdateGeneSymbol(entry.GeneSymbol, entry.EnsemblGeneId, entry.EntrezGeneId);
            }
        }

        private void AddOmimEntries(Dictionary<int, OmimImportEntry> mimIdToEntry, string omimPath)
        {
            using (var stream = GZipUtilities.GetAppropriateReadStream(omimPath))
            using (var reader = new OmimReader(stream))
            {
                reader.AddOmimEntries(mimIdToEntry);
            }
        }
    }
}
