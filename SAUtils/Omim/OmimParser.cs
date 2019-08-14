using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.Omim.EntryApiResponse;
using SAUtils.Schema;

namespace SAUtils.Omim
{

    public sealed class OmimParser : IDisposable
    {
        private readonly GeneSymbolUpdater _geneSymbolUpdater;
        private readonly SaJsonSchema _jsonSchema;
        private readonly HttpClient _httpClient;
        private readonly ZipArchive _zipArchive;
        private readonly FileStream _zipFileStream;

        private const string Mim2GeneUrl = "https://omim.org/static/omim/data/mim2gene.txt";
        private const string OmimApiUrl = "https://api.omim.org/api/";
        private const string EntryHandler = "entry";
        private const int EntryQueryLimit = 20;
        private const string ReturnDataFormat = "json";

        public OmimParser(GeneSymbolUpdater geneSymbolUpdater, SaJsonSchema jsonSchema, string apiKey, string dumpFilePath)
        {
            _geneSymbolUpdater = geneSymbolUpdater;
            _jsonSchema = jsonSchema;
            if (dumpFilePath != null)
            {
                _zipFileStream = new FileStream(dumpFilePath, FileMode.Create);
                _zipArchive = new ZipArchive(_zipFileStream, ZipArchiveMode.Create);
            }
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);
        }

        public IEnumerable<OmimItem> GetItems() => GetOmimItemsFromMinNumbers(GetMimNumberToGeneSymbol(_httpClient, Mim2GeneUrl, _zipArchive), _zipArchive);

        private IDictionary<string, string> GetMimNumberToGeneSymbol(HttpClient httpClient, string mim2GeneUrl, ZipArchive zipArchive)
        {
            var mimNumberToGeneSymbol = new Dictionary<string, string>();
            using (StreamWriter writer = zipArchive == null ? null : new StreamWriter(zipArchive.CreateEntry("mim2gene.txt").Open()))
            using (var response = httpClient.GetAsync(mim2GeneUrl).Result)
            using (var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    writer?.WriteLine(line);
                    if (line.OptimizedStartsWith('#')) continue;
                    var fields = line.OptimizedSplit('\t');
                    string geneSymbol = fields[3];
                    if (string.IsNullOrEmpty(geneSymbol)) continue;
                    string mimNumber = fields[0];
                    string entrezGeneId = fields[2];
                    string ensemblGeneId = fields[4];
                    string updatedGeneSymbol = _geneSymbolUpdater.UpdateGeneSymbol(geneSymbol, ensemblGeneId, entrezGeneId);
                    if (string.IsNullOrEmpty(updatedGeneSymbol)) continue;

                    mimNumberToGeneSymbol[mimNumber] = updatedGeneSymbol;
                }
            }

            return mimNumberToGeneSymbol;
        }

        private IEnumerable<OmimItem> GetOmimItemsFromMinNumbers(IDictionary<string, string> mimToGeneSymbol, ZipArchive zipArchive)
        {
            var i = 0;
            var queryIndex = 1;
            var mimNumbers = mimToGeneSymbol.Select(x => x.Key).ToArray();

            while (i < mimNumbers.Length)
            {
                int endMimNumberIndex = Math.Min(i + EntryQueryLimit - 1, mimNumbers.Length - 1);
                string mimNumberString = GetMimNumbersString(mimNumbers, i, endMimNumberIndex);
                string queryUrl = GetApiQueryUrl(OmimApiUrl, EntryHandler, ("mimNumber", mimNumberString), ("include", "text:description"), ("include", "externalLinks"), ("include", "geneMap"), ("format", ReturnDataFormat));
                using (StreamWriter writer = zipArchive == null ? null : new StreamWriter(zipArchive.CreateEntry($"EntryQueryResponses/response{queryIndex}.txt").Open()))
                using (var response = _httpClient.GetAsync(queryUrl).Result)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    writer?.Write(responseContent);
                    var entryResponse = JsonConvert.DeserializeObject<EntryRoot>(responseContent);
                    var entryIndex = 0;
                    foreach (var entry in entryResponse.omim.entryList)
                    {
                        int mimNumber = entry.entry.mimNumber;

                        string description = OmimUtilities.RemoveLinksInText(entry.entry.textSectionList?[0].textSection.textSectionContent);
                        string geneName = entry.entry.geneMap?.geneName;
                        var phenotypes = entry.entry.geneMap?.phenotypeMapList?.Select(x => OmimUtilities.GetPhenotype(x, _jsonSchema.GetSubSchema("phenotypes"))).ToList() ??
                                            new List<OmimItem.Phenotype>();

                        yield return new OmimItem(mimToGeneSymbol[mimNumber.ToString()], geneName, description, mimNumber, phenotypes, _jsonSchema);
                        entryIndex++;
                    }
                }

                i = endMimNumberIndex + 1;
                queryIndex++;
            }
        }

        private static string GetMimNumbersString(IReadOnlyList<string> allMimNumbers, int startIndex, int endIndex)
        {
            var sb = StringBuilderCache.Acquire();
            var needComma = false;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (needComma) sb.Append(',');
                sb.Append(allMimNumbers[i]);

                needComma = true;
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private static string GetApiQueryUrl(string baseAddress, string handler, params (string, string)[] keyValueTuples)
        {
            var sb = StringBuilderCache.Acquire(100);
            sb.Append(baseAddress);
            sb.Append(handler);
            sb.Append('?');
            var needAmpersand = false;
            foreach ((string key, string value) in keyValueTuples)
            {
                if (needAmpersand) sb.Append('&');

                sb.Append(key);
                sb.Append('=');
                sb.Append(value);

                needAmpersand = true;
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _zipArchive.Dispose();
            _zipFileStream.Dispose();
        }
    }
}