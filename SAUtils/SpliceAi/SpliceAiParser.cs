using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Intervals;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.SpliceAi
{
    public sealed class SpliceAiParser:IDisposable
    {
        private readonly Stream _stream;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly Dictionary<ushort, IntervalArray<byte>> _spliceIntervals;
        private readonly HashSet<string> _unresolvedSymbols;
        public static int Count;

        private string _geneSymbol;
        private double _acceptorGainScore;
        private double _acceptorLossScore;
        private double _donorGainScore;
        private double _donorLossScore;

        private int _acceptorGainPosition;
        private int _acceptorLossPosition;
        private int _donorGainPosition;
        private int _donorLossPosition;

        private readonly Dictionary<string, string> _spliceToNirvanaSymbols;

        public SpliceAiParser(Stream stream, ISequenceProvider sequenceProvider, Dictionary<ushort, IntervalArray<byte>> spliceIntervals, Dictionary<string, string> spliceToNirGeneSymbols)
        {
            _stream                 = stream;
            _sequenceProvider       = sequenceProvider;
            _spliceIntervals        = spliceIntervals;
            _spliceToNirvanaSymbols = spliceToNirGeneSymbols;
            _unresolvedSymbols      = new HashSet<string>();
        }

        public IEnumerable<SpliceAiItem> GetItems()
        {
            using (var reader = FileUtilities.GetStreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // comments may contain the Format field
                    if (line.OptimizedStartsWith('#'))
                    {
                        if (line.Contains("Format:")) GetFieldIndices(line);
                        continue;
                    }

                    var item = ExtractItem(line);
                    if (item == null) continue;
                    UpdateGeneSymbol(item);
                    if (string.IsNullOrEmpty(item.Hgnc)) continue;
                    yield return item;
                    
                }
            }
            
            Console.WriteLine($"{_unresolvedSymbols.Count} unresolved gene symbols encountered. Symbols:");
            foreach (var symbol in _unresolvedSymbols)
            {
                Console.Write(symbol+',');
            }
        }

        //##INFO=<ID=SpliceAI,Number=.,Type=String,Description="SpliceAIv1.3 variant annotation. These include delta scores (DS) and delta positions (DP) 
        //for acceptor gain (AG), acceptor loss (AL), donor gain (DG), and donor loss (DL). Format: ALLELE|SYMBOL|DS_AG|DS_AL|DS_DG|DS_DL|DP_AG|DP_AL|DP_DG|DP_DL">
        private int _geneSymbolIndex = -1;
        private int _dsAgIndex = -1;
        private int _dsAlIndex = -1;
        private int _dsDgIndex = -1;
        private int _dsDlIndex = -1;
        private int _dpAgIndex = -1;
        private int _dpAlIndex = -1;
        private int _dpDgIndex = -1;
        private int _dpDlIndex = -1;

        private const string GeneSymbolTag = "SYMBOL";
        private const string DsAgTag = "DS_AG";
        private const string DsAlTag = "DS_AL";
        private const string DsDgTag = "DS_DG";
        private const string DsDlTag = "DS_DL";
        private const string DpAgTag = "DP_AG";
        private const string DpAlTag = "DP_AL";
        private const string DpDgTag = "DP_DG";
        private const string DpDlTag = "DP_DL";

        private void GetFieldIndices(string line) {
            var format = line.Split("Format:")[1];
            format = format.EndsWith("\">") ? format.Substring(0, format.Length - 2): format;
            var fields = format.OptimizedSplit('|');
            
            _geneSymbolIndex = Array.IndexOf(fields, GeneSymbolTag);

            _dsAgIndex = Array.IndexOf(fields, DsAgTag);
            _dsDgIndex = Array.IndexOf(fields, DsDgTag);
            _dsAlIndex = Array.IndexOf(fields, DsAlTag);
            _dsDlIndex = Array.IndexOf(fields, DsDlTag);

            _dpAgIndex = Array.IndexOf(fields, DpAgTag);
            _dpDgIndex = Array.IndexOf(fields, DpDgTag);
            _dpAlIndex = Array.IndexOf(fields, DpAlTag);
            _dpDlIndex = Array.IndexOf(fields, DpDlTag);
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

            var start = position;
            //skipping insertions/deletions that were shifted
            if (VariantUtils.IsLeftShiftPossible(refAllele, altAllele)) return null;
            (start, refAllele, altAllele) = BiDirectionalTrimmer.Trim(start, refAllele, altAllele);
            
            var end = start + refAllele.Length - 1;
            var isSpliceAdjacent = _spliceIntervals[chromosome.Index].OverlapsAny(start, end);
            
            ParseInfoField(splitLine[VcfCommon.InfoIndex]);
            
            if (!HasSignificantScore() && !isSpliceAdjacent) return null;
            
            Count++;
            return new SpliceAiItem(chromosome, start, refAllele, altAllele, _geneSymbol,
                _acceptorGainScore, _acceptorLossScore, _donorGainScore, _donorLossScore,
                _acceptorGainPosition, _acceptorLossPosition, _donorGainPosition, _donorLossPosition, isSpliceAdjacent);
        }

        private void UpdateGeneSymbol(SpliceAiItem item)
        {
            if (_spliceToNirvanaSymbols.TryGetValue(item.Hgnc, out var nirHgnc)) item.Hgnc = nirHgnc;
            else
            {
                _unresolvedSymbols.Add(item.Hgnc);
            }
        }

        private bool HasSignificantScore()
        {
            return _acceptorLossScore >= SpliceAiItem.MinSpliceAiScore ||
                   _acceptorGainScore >= SpliceAiItem.MinSpliceAiScore ||
                   _donorGainScore    >= SpliceAiItem.MinSpliceAiScore ||
                   _donorLossScore    >= SpliceAiItem.MinSpliceAiScore;
        }

        //1       69091   .       A       C       .       .       SpliceAI=C|OR4F5|0.01|0.00|0.00|0.00|42|25|24|2
        private void ParseInfoField(string infoFields)
        {
            Clear();
            if (infoFields == "" || infoFields == ".") return;
            var values = infoFields.OptimizedSplit('|');

            _geneSymbol = values[_geneSymbolIndex];
            _acceptorGainScore = Convert.ToDouble(values[_dsAgIndex]);
            _acceptorLossScore = Convert.ToDouble(values[_dsAlIndex]);
            _donorGainScore = Convert.ToDouble(values[_dsDgIndex]);
            _donorLossScore = Convert.ToDouble(values[_dsDlIndex]);

            _acceptorGainPosition = Convert.ToInt32(values[_dpAgIndex]);
            _acceptorLossPosition = Convert.ToInt32(values[_dpAlIndex]);
            _donorGainPosition = Convert.ToInt32(values[_dpDgIndex]);
            _donorLossPosition = Convert.ToInt32(values[_dpDlIndex]);
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

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }

}