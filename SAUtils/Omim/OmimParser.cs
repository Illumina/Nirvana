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

namespace SAUtils.Omim;

public sealed class OmimParser
{
    private readonly string       _mimToGeneSymbolFile;
    private readonly string       _omimJsonFile;
    private readonly SaJsonSchema _jsonSchema;

    private const string CurrentOmimJsonVersion = "1.0";
        
    public readonly OmimStatistics OmimStats = new();

    public OmimParser(string mimToGeneSymbolFile, string omimJsonFile, SaJsonSchema jsonSchema)
    {
        _mimToGeneSymbolFile = mimToGeneSymbolFile;
        _omimJsonFile        = omimJsonFile;
        _jsonSchema          = jsonSchema;
    }

    public DataSourceVersion GetVersion() => DataSourceVersionReader.GetSourceVersion(_omimJsonFile);

    public IEnumerable<OmimItem> GetItems()
    {
        Dictionary<int, string> mimToGeneSymbol       = GetMimNumberToGeneSymbol();
        EntryRoot               entryRoot             = GetEntryRootObject();
        Dictionary<int, string> phenotypeDescriptions = GetPhenotypeDescriptions(entryRoot);

        foreach (OmimItem omimItem in GetOmimItems(entryRoot, mimToGeneSymbol, phenotypeDescriptions))
        {
            OmimStats.Add(omimItem);
            yield return omimItem;
        }
    }

    private static Dictionary<int, string> GetPhenotypeDescriptions(EntryRoot entryRoot)
    {
            
        Dictionary<int, string> phenotypeToDescription = new Dictionary<int, string>();

        foreach (var entry in entryRoot.omim.entryList)
        {
            var item = entry.entry;
            // gene only item
            if (item.prefix == '*') continue;
        
            var description = OmimUtilities.ExtractAndProcessItemDescription(item);
            if (string.IsNullOrEmpty(description)) continue;
            phenotypeToDescription[item.mimNumber] = description;
        }

        return phenotypeToDescription;
    }

    private Dictionary<int, string> GetMimNumberToGeneSymbol()
    {
        var mimNumberToGeneSymbol = new Dictionary<int, string>();
        using (var stream = new FileStream(_mimToGeneSymbolFile, FileMode.Open))
        using (var reader = new StreamReader(stream))
        {
            string line;
            //title line
            reader.ReadLine();
            while ((line = reader.ReadLine()) != null)
            {
                var fields = line.OptimizedSplit('\t');
                mimNumberToGeneSymbol[int.Parse(fields[0])] = fields[1];
            }
        }

        return mimNumberToGeneSymbol;
    }

    private EntryRoot GetEntryRootObject()
    {
        using var fileStream         = new FileStream(_omimJsonFile, FileMode.Open);
        using var uncompressedStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var streamReader       = new StreamReader(uncompressedStream);
        var       entryQueryResponse = JsonConvert.DeserializeObject<EntryRoot>(streamReader.ReadToEnd());
        if (entryQueryResponse.omim.version != CurrentOmimJsonVersion)
            throw new InvalidDataException($"An unknown version of OMIM JSON schema has been used: version {entryQueryResponse.omim.version}. The latest known version is {CurrentOmimJsonVersion}");

        return entryQueryResponse;
    }

    private IEnumerable<OmimItem> GetOmimItems(EntryRoot entryRoot, Dictionary<int, string> mimToGeneSymbol, Dictionary<int, string> phenotypeDescriptions)
    {
        foreach (var entry in entryRoot.omim.entryList)
        {
            var item      = entry.entry;
            var mimNumber = item.mimNumber;
            //skip if not a supported gene symbol
            if (!mimToGeneSymbol.TryGetValue(mimNumber, out var geneSymbol)) continue;

            string description = OmimUtilities.ExtractAndProcessItemDescription(item);
            string geneName    = item.geneMap?.geneName;
            var phenotypes = item.geneMap?.phenotypeMapList?.Select(x => OmimUtilities.GetPhenotype(x, phenotypeDescriptions, _jsonSchema.GetSubSchema("phenotypes")))
                .ToList() ?? new List<OmimItem.Phenotype>();

            yield return new OmimItem(geneSymbol, geneName, description, mimNumber, phenotypes, _jsonSchema);

        }
    }
}