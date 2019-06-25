using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Intervals;
using IO;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace SAUtils.SpliceAi
{
    public sealed class SpliceAiParser:IDisposable
    {
        private readonly Stream _stream;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly Dictionary<ushort, IntervalArray<byte>> _spliceIntervals;
        public static int Count = 0;

        private double _acceptorGainScore;
        private double _acceptorLossScore;
        private double _donorGainScore;
        private double _donorLossScore;

        private int _acceptorGainPosition;
        private int _acceptorLossPosition;
        private int _donorGainPosition;
        private int _donorLossPosition;


        public SpliceAiParser(Stream stream, ISequenceProvider sequenceProvider, Dictionary<ushort, IntervalArray<byte>> spliceIntervals)
        {
            _stream = stream;
            _sequenceProvider = sequenceProvider;
            _spliceIntervals = spliceIntervals;

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

                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;

                    var item = ExtractItem(line);
                    if (item == null) continue;
                    yield return item;

                }
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
            var position = int.Parse(splitLine[VcfCommon.PosIndex]);
            var refAllele = splitLine[VcfCommon.RefIndex];
            var altAllele = splitLine[VcfCommon.AltIndex];

            if (altAllele.Contains(',')) throw new DataException($"multiple ref allele present for {chromosome}-{position}");

            ParseInfoField(splitLine[VcfCommon.InfoIndex]);
            var isSpliceAdjacent = _spliceIntervals[chromosome.Index].OverlapsAny(position, position);
            if (!HasSignificantScore() && !isSpliceAdjacent) return null;

            Count++;
            return new SpliceAiItem(chromosome, position, refAllele, altAllele, 
                _acceptorGainScore, _acceptorLossScore, _donorGainScore, _donorLossScore,
                _acceptorGainPosition, _acceptorLossPosition, _donorGainPosition, _donorLossPosition, isSpliceAdjacent);
        }

        private bool HasSignificantScore()
        {
            return _acceptorLossScore >= SpliceAiItem.MinSpliceAiScore ||
                   _acceptorGainScore >= SpliceAiItem.MinSpliceAiScore ||
                   _donorGainScore >= SpliceAiItem.MinSpliceAiScore ||
                   _donorLossScore >= SpliceAiItem.MinSpliceAiScore;
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
            _acceptorGainScore = 0;
            _acceptorLossScore = 0;
            _donorGainScore = 0;
            _donorLossScore = 0;

            _acceptorGainPosition = int.MaxValue;
            _acceptorLossPosition = int.MaxValue;
            _donorGainPosition = int.MaxValue;
            _donorLossPosition = int.MaxValue;
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

            }

        }
        public void Dispose()
        {
            _stream?.Dispose();
        }
    }

}