using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using OptimizedCore;
using SAUtils.GeneIdentifiers;

namespace SAUtils.Omim
{

    public sealed class OmimQuery : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly FileStream _mimToSymbolStream;
        private readonly FileStream _jsonResponseStream;
        private string _jsonPrefix;
        private readonly string _mimTitlesUrl;

        private const string Mim2GeneUrl = "https://omim.org/static/omim/data/mim2gene.txt";
        private const string MimTitlesFileName = "mimTitles.txt";
        private const string OmimApiUrl = "https://api.omim.org/api/";
        private const string OmimDownloadBaseUrl = "https://data.omim.org/downloads/";
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
            
            _mimTitlesUrl = GetMimTitlesUrl(apiKey);
            _mimToSymbolStream = new FileStream(Path.Combine(outputDirectory, MimToSymbolFile), FileMode.Create);
            _jsonResponseStream = new FileStream(Path.Combine(outputDirectory, JsonResponseFile), FileMode.Create);
        }

        private static string GetMimTitlesUrl(string apiKey) => $"{OmimDownloadBaseUrl}{apiKey}/{MimTitlesFileName}";

        private List<string> GetMimsToDownload()
        {
            var mims = new List<string>();
            using (var response = _httpClient.GetAsync(_mimTitlesUrl).Result)
            using (var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    //Caret (^)  Entry has been removed from the database or moved to another entry
                    if (line.OptimizedStartsWith('#') || line.StartsWith("Caret")) continue;

                    var fields = line.Split('\t', 3);
                    mims.Add(fields[1]);
                }
            }

            return mims;
        }

        public void GenerateMimToGeneSymbolFile(GeneSymbolUpdater geneSymbolUpdater)
        {
            using StreamWriter writer = new StreamWriter(_mimToSymbolStream);
            using var response = _httpClient.GetAsync(Mim2GeneUrl).Result;
            using var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
            writer.WriteLine("#MIM number\tGene symbol");
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.OptimizedStartsWith('#')) continue;

                var fields     = line.OptimizedSplit('\t');
                var geneSymbol = fields[3];
                if (string.IsNullOrEmpty(geneSymbol)) continue;

                var mimNumber         = fields[0];
                var entrezGeneId      = fields[2];
                var ensemblGeneId     = fields[4];
                var updatedGeneSymbol = geneSymbolUpdater.UpdateGeneSymbol(geneSymbol, ensemblGeneId, entrezGeneId);
                if (string.IsNullOrEmpty(updatedGeneSymbol)) continue;

                writer.WriteLine($"{mimNumber}\t{updatedGeneSymbol}");
            }
        }

        public void GenerateJsonResponse()
        {
            var i = 0;
            var mimNumbers = GetMimsToDownload();

            var needComma = false;
            using Stream gzStream = new GZipStream(_jsonResponseStream, CompressionMode.Compress);
            using StreamWriter writer = new StreamWriter(gzStream);
            while (i < mimNumbers.Count)
            {
                var endMimNumberIndex = Math.Min(i + EntryQueryLimit - 1, mimNumbers.Count - 1);
                var mimNumberString   = GetMimNumbersString(mimNumbers, i, endMimNumberIndex);
                var queryUrl = GetApiQueryUrl(OmimApiUrl, EntryHandler, ("mimNumber", mimNumberString),
                    ("include", "text:description"), ("include", "externalLinks"), ("include", "geneMap"),
                    ("format", ReturnDataFormat));

                using (var response = _httpClient.GetAsync(queryUrl).Result)
                {
                    string responseContent = response.Content.ReadAsStringAsync().Result;
                    string entries         = SetPrefixAndGetEntriesString(responseContent);
                    if (i == 0) writer.Write(_jsonPrefix);
                    if (needComma) writer.Write(',');
                    writer.Write(entries);
                    needComma = true;
                }

                i = endMimNumberIndex + 1;
            }

            writer.WriteLine(JsonTextEnding);
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

        private static string GetMimNumbersString(List<string> allMimNumbers, int startIndex, int endIndex)
        {
            var sb = StringBuilderPool.Get();
            var needComma = false;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (needComma) sb.Append(',');
                sb.Append(allMimNumbers[i]);

                needComma = true;
            }

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        private static string GetApiQueryUrl(string baseAddress, string handler, params (string, string)[] keyValueTuples)
        {
            var sb = StringBuilderPool.Get();
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

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _mimToSymbolStream?.Dispose();
            _jsonResponseStream?.Dispose();
        }
    }
}