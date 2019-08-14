using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Genome;
using Intervals;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.SpliceAi
{
    public sealed class SpliceAiParser:IDisposable
    {
        private readonly Stream _stream;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly Dictionary<ushort, IntervalArray<byte>> _spliceIntervals;
        private readonly HashSet<string> _unresolvedSymbols;
        public static int Count = 0;

        private string _geneSymbol;
        private double _acceptorGainScore;
        private double _acceptorLossScore;
        private double _donorGainScore;
        private double _donorLossScore;

        private int _acceptorGainPosition;
        private int _acceptorLossPosition;
        private int _donorGainPosition;
        private int _donorLossPosition;
        private readonly IntervalForest<string> _geneTree;
        private readonly Dictionary<string, List<string>> _geneSynonyms;
        private readonly HashSet<string> _currentPositionGeneSymbols;

        public SpliceAiParser(Stream stream, ISequenceProvider sequenceProvider, Dictionary<ushort, IntervalArray<byte>> spliceIntervals, IntervalForest<string> geneTree = null, Dictionary<string, List<string>> geneSynonyms = null)
        {
            _stream              = stream;
            _sequenceProvider    = sequenceProvider;
            _spliceIntervals     = spliceIntervals;
            _geneTree            = geneTree;
            _geneSynonyms        = geneSynonyms;
            _unresolvedSymbols   = new HashSet<string>();
            _currentPositionGeneSymbols  = new HashSet<string>();
        }

        public IEnumerable<SpliceAiItem> GetItems()
        {
            var previousItems = new List<SpliceAiItem>();

            using (var reader = FileUtilities.GetStreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;

                    var item = ExtractItem(line);
                    if (item == null) continue;
                    if (previousItems.Count == 0 || SpliceAiItem.CompareTo(item, previousItems[0])==0)
                    {
                        //starting or extending new position
                        previousItems.Add(item);
                        continue;
                    }
                    //performing sanity check
                    SanityCheck(previousItems);
                    UpdateGeneSymbols(previousItems);

                    foreach (var spliceAiItem in previousItems)
                    {
                        yield return spliceAiItem;
                    }
                    previousItems.Clear();
                    previousItems.Add(item);
                }
            }
            //clearing off the final items (they should all be at the same  position
            UpdateGeneSymbols(previousItems);
            foreach (var spliceAiItem in previousItems)
            {
                yield return spliceAiItem;
            }

            Console.WriteLine($"{_unresolvedSymbols.Count} unresolved gene symbols encountered. Symbols:");
            foreach (var symbol in _unresolvedSymbols)
            {
                Console.Write(symbol+',');
            }
        }

        private static void SanityCheck(List<SpliceAiItem> previousItems)
        {
            for (var i = 0; i < previousItems.Count - 1; i++)
            {
                if (previousItems[i].Position != previousItems[i + 1].Position)
                    throw new DataMisalignedException("different positions grouped together");
                if (previousItems[i].Chromosome.Index != previousItems[i + 1].Chromosome.Index)
                    throw new DataMisalignedException("different chromosomes grouped together");
            }
        }

        private void UpdateGeneSymbols(List<SpliceAiItem> items)
        {
            _currentPositionGeneSymbols.Clear();
            foreach (var item in items)
            {
                _currentPositionGeneSymbols.Add(item.Hgnc);
            }

            foreach (var item in items)
            {
                UpdateGeneSymbol(item);
            }
        }


        /// <summary>
        /// Extracts a splice AI item from the specified VCF line.
        /// </summary>
        /// <param name="vcfLine"></param>
        /// <returns></returns>
        private SpliceAiItem ExtractItem(string vcfLine)
        {
            var splitLine = vcfLine.Split('\t');
            if (splitLine.Length < VcfCommon.InfoIndex+1) return null;

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_sequenceProvider.RefNameToChromosome.ContainsKey(chromosomeName)) return null;

            var chromosome = _sequenceProvider.RefNameToChromosome[chromosomeName];
            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
            var refAllele  = splitLine[VcfCommon.RefIndex];
            var altAllele  = splitLine[VcfCommon.AltIndex];
            
            if (altAllele.Contains(',')) throw new DataException($"multiple alt allele present for {chromosome}-{position}");
            
            ParseInfoField(splitLine[VcfCommon.InfoIndex]);
            
            var isSpliceAdjacent = _spliceIntervals[chromosome.Index].OverlapsAny(position, position);
            if (!HasSignificantScore() && !isSpliceAdjacent) return null;

            Count++;
            return new SpliceAiItem(chromosome, position, refAllele, altAllele, _geneSymbol,
                _acceptorGainScore, _acceptorLossScore, _donorGainScore, _donorLossScore,
                _acceptorGainPosition, _acceptorLossPosition, _donorGainPosition, _donorLossPosition, isSpliceAdjacent);
        }

        private void UpdateGeneSymbol(SpliceAiItem item)
        {
            if (_geneTree == null || _geneSynonyms == null) return;

            var chromosome = item.Chromosome;
            var position = item.Position;

            if (_currentPositionGeneSymbols.Count > 1) return;//for multiple genes for a position, we cannot update the symbol

            var nirvanaGenes = _geneTree.GetAllOverlappingValues(chromosome.Index, position, position);
            if (nirvanaGenes == null)
            {
                item.Hgnc = null;
                return;
            }

            var uniqueOverlapping = new HashSet<string>(nirvanaGenes);

            if (uniqueOverlapping.Contains(item.Hgnc)) return;

            //gene not found in cache
            if (uniqueOverlapping.Count == 1) item.Hgnc = uniqueOverlapping.First(); //update gene symbol
            else
            {
                if (!_geneSynonyms.TryGetValue(item.Hgnc, out var symbolsList)) return;

                var commonSymbols = symbolsList.Intersect(uniqueOverlapping).ToArray();
                if (commonSymbols.Length == 1) item.Hgnc = commonSymbols[0]; 
                else _unresolvedSymbols.Add(item.Hgnc);
            }
        }

        private bool HasSignificantScore()
        {
            return _acceptorLossScore >= SpliceAiItem.MinSpliceAiScore ||
                   _acceptorGainScore >= SpliceAiItem.MinSpliceAiScore ||
                   _donorGainScore    >= SpliceAiItem.MinSpliceAiScore ||
                   _donorLossScore    >= SpliceAiItem.MinSpliceAiScore;
        }

        private void ParseInfoField(string infoFields)
        {
            Clear();
            if (infoFields == "" || infoFields == ".") return;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (var infoItem in infoItems)
            {
                var (key, value) = infoItem.OptimizedKeyValue();
                // sanity check
                if (value != null) SetInfoField(key, value);
            }
        }

        private void Clear()
        {
            _geneSymbol = null;

            _acceptorGainScore = 0;
            _acceptorLossScore = 0;
            _donorGainScore    = 0;
            _donorLossScore    = 0;

            _acceptorGainPosition = int.MaxValue;
            _acceptorLossPosition = int.MaxValue;
            _donorGainPosition    = int.MaxValue;
            _donorLossPosition    = int.MaxValue;
        }

        private void SetInfoField(string vcfId, string value)
        {
            switch (vcfId)
            {
                case "DS_AG":
                    _acceptorGainScore =Convert.ToDouble(value);
                    break;

                case "DS_AL":
                    _acceptorLossScore = Convert.ToDouble(value);
                    break;

                case "DS_DG":
                    _donorGainScore = Convert.ToDouble(value);
                    break;

                case "DS_DL":
                    _donorLossScore = Convert.ToDouble(value);
                    break;

                case "DP_AG":
                    _acceptorGainPosition = Convert.ToInt32(value);
                    break;

                case "DP_AL":
                    _acceptorLossPosition = Convert.ToInt32(value);
                    break;

                case "DP_DG":
                    _donorGainPosition = Convert.ToInt32(value);
                    break;

                case "DP_DL":
                    _donorLossPosition = Convert.ToInt32(value);
                    break;

                case "SYMBOL":
                    _geneSymbol = value;
                    break;

            }

        }
        public void Dispose()
        {
            _stream?.Dispose();
        }
    }

}