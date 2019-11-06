using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using OptimizedCore;
using SAUtils.GeneIdentifiers;

namespace SAUtils.Omim
{

    public sealed class OmimQuery : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly FileStream _minToSymbolStream;
        private readonly FileStream _jsonResponseStream;
        private string _jsonPrefix;

        private const string Mim2GeneUrl = "https://omim.org/static/omim/data/mim2gene.txt";
        private const string OmimApiUrl = "https://api.omim.org/api/";
        private const string EntryHandler = "entry";
        private const int EntryQueryLimit = 20;
        private const string ReturnDataFormat = "json";
        private const string MimToSymbolFile = "MimToGeneSymbol.tsv";
        public const string JsonResponseFile = "MimEntries.json.gz";
        private const string JsonPrefixPattern = @"^{""omim"": { \n""version"": ""\d+\.\d+\"",\n""entryList"": \[ \n";
        private const string JsonTextEnding = "] \n} }";

        public OmimQuery(string apiKey, string outputDirectory) 
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("ApiKey", apiKey);

            if (string.IsNullOrEmpty(outputDirectory)) return;
            _minToSymbolStream = new FileStream(Path.Combine(outputDirectory, MimToSymbolFile), FileMode.Create);
            _jsonResponseStream = new FileStream(Path.Combine(outputDirectory, JsonResponseFile), FileMode.Create);
        }

        public IDictionary<string, string> GenerateMimToGeneSymbol(GeneSymbolUpdater geneSymbolUpdater)
        {
            var mimNumberToGeneSymbol = new Dictionary<string, string>();
            using (StreamWriter writer = new StreamWriter(_minToSymbolStream))
            using (var response = _httpClient.GetAsync(Mim2GeneUrl).Result)
            using (var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                writer.WriteLine("#MIM number\tGene symbol");
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.OptimizedStartsWith('#')) continue;

                    var fields = line.OptimizedSplit('\t');
                    string geneSymbol = fields[3];
                    if (string.IsNullOrEmpty(geneSymbol)) continue;

                    string mimNumber = fields[0];
                    string entrezGeneId = fields[2];
                    string ensemblGeneId = fields[4];
                    string updatedGeneSymbol = geneSymbolUpdater.UpdateGeneSymbol(geneSymbol, ensemblGeneId, entrezGeneId);
                    if (string.IsNullOrEmpty(updatedGeneSymbol)) continue;

                    writer.WriteLine($"{mimNumber}\t{updatedGeneSymbol}");
                    mimNumberToGeneSymbol[mimNumber] = updatedGeneSymbol;
                }
            }

            return mimNumberToGeneSymbol;
        }

        public void GenerateJsonResponse(IDictionary<string, string> mimToGeneSymbol)
        {
            var i = 0;
            var mimNumbers = mimToGeneSymbol.Select(x => x.Key).ToArray();

            bool needComma = false;
            using (Stream gzStream = new GZipStream(_jsonResponseStream, CompressionMode.Compress))
            using (StreamWriter writer = new StreamWriter(gzStream))
            {
                while (i < mimNumbers.Length)
                {
                    int endMimNumberIndex = Math.Min(i + EntryQueryLimit - 1, mimNumbers.Length - 1);
                    string mimNumberString = GetMimNumbersString(mimNumbers, i, endMimNumberIndex);
                    string queryUrl = GetApiQueryUrl(OmimApiUrl, EntryHandler, ("mimNumber", mimNumberString),
                        ("include", "text:description"), ("include", "externalLinks"), ("include", "geneMap"),
                        ("format", ReturnDataFormat));

                    using (var response = _httpClient.GetAsync(queryUrl).Result)
                    {
                        string responseContent = response.Content.ReadAsStringAsync().Result;
                        string entries = SetPrefixAndGetEntriesString(responseContent);
                        if (i == 0) writer.Write(_jsonPrefix);
                        if (needComma) writer.Write(',');
                        writer.Write(entries);
                        needComma = true;
                    }

                    i = endMimNumberIndex + 1;
                }

                writer.WriteLine(JsonTextEnding);
            }
        }

        private string SetPrefixAndGetEntriesString(string responseContent)
        {
            if (string.IsNullOrEmpty(_jsonPrefix))
            {
                var prefixMatch = Regex.Match(responseContent, JsonPrefixPattern);
                if (!prefixMatch.Success)
                    throw new InvalidDataException(
                        $"Cannot find expected content at the beginning of the response from OMIM server. The response starts with \"{responseContent.Substring(0, JsonPrefixPattern.Length)}\"");

                _jsonPrefix = prefixMatch.Value;
            }

            int entriesStringLength = responseContent.Length - _jsonPrefix.Length - JsonTextEnding.Length;
            return responseContent.Substring(_jsonPrefix.Length, entriesStringLength);
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
            _httpClient?.Dispose();
            _minToSymbolStream?.Dispose();
            _jsonResponseStream?.Dispose();
        }
    }
}