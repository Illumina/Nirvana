using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers;
using SAUtils.Omim.EntryApiResponse;
using SAUtils.Schema;
using VariantAnnotation.Providers;

namespace SAUtils.Omim
{

    public sealed class OmimParser
    {
        private readonly string _mimToGeneSymbolFile;
        private readonly string _omimJsonFile;
        private readonly SaJsonSchema _jsonSchema;

        private const string CurrentOmimJsonVersion = "1.0";

        public OmimParser(string mimToGeneSymbolFile, string omimJsonFile,  SaJsonSchema jsonSchema)
        {
            _mimToGeneSymbolFile = mimToGeneSymbolFile;
            _omimJsonFile = omimJsonFile;
            _jsonSchema = jsonSchema;
        }

        public DataSourceVersion GetVersion() => DataSourceVersionReader.GetSourceVersion(_omimJsonFile);

        public IEnumerable<OmimItem> GetItems()
        {
            var mimToGeneSymbol = GetMimNumberToGeneSymbol();
            
            foreach (var entry in GetEntryItems())
            {
                int mimNumber = entry.mimNumber;

                string description = OmimUtilities.RemoveLinksInText(entry.textSectionList?[0].textSection.textSectionContent);
                string geneName = entry.geneMap?.geneName;
                var phenotypes = entry.geneMap?.phenotypeMapList?.Select(x => OmimUtilities.GetPhenotype(x, _jsonSchema.GetSubSchema("phenotypes")))
                                     .ToList() ?? new List<OmimItem.Phenotype>();

                yield return new OmimItem(mimToGeneSymbol[mimNumber.ToString()], geneName, description, mimNumber,phenotypes, _jsonSchema);
            }
        }

        private IDictionary<string, string> GetMimNumberToGeneSymbol()
        {
            var mimNumberToGeneSymbol = new Dictionary<string, string>();
            using (var stream = new FileStream(_mimToGeneSymbolFile, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.OptimizedSplit('\t');
                    mimNumberToGeneSymbol[fields[0]] = fields[1];
                }
            }

            return mimNumberToGeneSymbol;
        }

        private IEnumerable<EntryItem> GetEntryItems()
        {
            using (var fileStream = new FileStream(_omimJsonFile, FileMode.Open))
            using (var uncompressedStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(uncompressedStream))
            {
                var entryQueryResponse = JsonConvert.DeserializeObject<EntryRoot>(streamReader.ReadToEnd());
                if (entryQueryResponse.omim.version != CurrentOmimJsonVersion)
                    throw new InvalidDataException($"An unknown version of OMIM JSON schema has been used: version {entryQueryResponse.omim.version}. The latest known version is {CurrentOmimJsonVersion}");

                foreach (var entry in entryQueryResponse.omim.entryList)
                {
                    yield return entry.entry;
                }
            }
        }
    }
}