using System;
using System.Collections.Generic;
using System.IO;
using OptimizedCore;

namespace SAUtils.SpliceAi
{
    public class GeneInfoParser:IDisposable
    {
        private readonly StreamReader _reader;

        private const string SymbolTag     = "Symbol";
        private const string SynonymsTag   = "Synonyms";
        private const string ChromosomeTag = "chromosome";
        private const string LocationTag   = "map_location";

        private int _symbolIndex     = -1;
        private int _synonymsIndex   = -1;
        private int _chromosomeIndex = -1;
        private int _locationIndex   = -1;

        public GeneInfoParser(StreamReader reader)
        {
            _reader = reader;
        }

        public Dictionary<string , List<string>> GetGeneSymbolSynonyms()
        {
            bool isFirstLine = true;

            var geneSynonyms = new Dictionary<string, List<string>>();
            var largestSymbolCount = 0;
            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    if (!GetColumnIndices(line)) return null;
                    isFirstLine = false;
                }
                else
                {
                    var cols = line.OptimizedSplit('\t');
                    var latestSymbol = cols[_symbolIndex];
                    var synonyms     = cols[_synonymsIndex];
                    var chromosome   = cols[_chromosomeIndex];
                    var locations    = cols[_locationIndex];

                    if (synonyms == "-")
                    {
                        geneSynonyms[latestSymbol] = new List<string>() {latestSymbol};
                        continue;
                    }
                    
                    foreach (var synonym in synonyms.OptimizedSplit('|'))
                    {
                        if (!geneSynonyms.TryGetValue(synonym, out var symbols))
                            geneSynonyms[synonym] = new List<string>(){latestSymbol};
                        else symbols.Add(latestSymbol);

                        if (geneSynonyms[synonym].Count > largestSymbolCount)
                            largestSymbolCount = geneSynonyms[synonym].Count;
                    }
                    
                }

            }
            
            return geneSynonyms;
        }

        private bool GetColumnIndices(string line)
        {
            var cols = line.OptimizedSplit('\t');

            _symbolIndex     = Array.IndexOf(cols, SymbolTag);
            _synonymsIndex   = Array.IndexOf(cols, SynonymsTag);
            _chromosomeIndex = Array.IndexOf(cols, ChromosomeTag);
            _locationIndex   = Array.IndexOf(cols, LocationTag);

            return _symbolIndex     != -1 &&
                   _synonymsIndex   != -1 &&
                   _chromosomeIndex != -1 &&
                   _locationIndex   != -1;
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}