using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers.TOPMed
{
    public sealed class TopMedReader : IDisposable
    {
        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refChromDict;
        private readonly ISequenceProvider _sequenceProvider;

        private int? _alleleNum;
        private int? _alleleCount;
        private int? _homCount;

        public TopMedReader(StreamReader streamReader, ISequenceProvider sequenceProvider)
        {
            _reader       = streamReader;
            _sequenceProvider = sequenceProvider;
            _refChromDict = sequenceProvider.RefNameToChromosome;
        }

        private void Clear()
        {
            _alleleNum    = null;
            _alleleCount  = null;
            _homCount     = null;
        }

        public IEnumerable<TopMedItem> GetItems()
        {
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.OptimizedStartsWith('#')) continue;

                    var topMedItem = ExtractItems(line);
                    if (topMedItem == null) continue;
                    yield return topMedItem;
                }
            }
        }

        private TopMedItem ExtractItems(string vcfLine)
        {
            if (vcfLine == null) return null;
            var splitLine = vcfLine.OptimizedSplit('\t');

            if (splitLine.Length < 8) return null;

            Clear();
            // chr1    10169   TOPMed_freeze_5?chr1:10,169     T       C       255     SVM     VRT=1;NS=62784;AN=125568;AC=20;AF=0.000159276;Het=20;Hom=0      NA:FRQ  125568:0.000159276

            var chromosome = splitLine[VcfCommon.ChromIndex];
            if (!_refChromDict.ContainsKey(chromosome)) return null;

            var chrom      = _refChromDict[chromosome];
            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var refAllele  = splitLine[VcfCommon.RefIndex];
            var altAllele  = splitLine[VcfCommon.AltIndex];
            var filters    = splitLine[VcfCommon.FilterIndex];
            var infoFields = splitLine[VcfCommon.InfoIndex];

            if (altAllele.Contains(","))
            {
                Console.WriteLine(vcfLine);
                throw new InvalidDataException("het site found!!");
            }

            var failedFilter = !(filters.Equals("PASS") || filters.Equals("."));

            ParseInfoField(infoFields);

            if (_alleleNum == 0) return null;
            var (shiftedPos, shiftedRef, shiftedAlt) = VariantUtils.TrimAndLeftAlign(position, refAllele,
                altAllele, _sequenceProvider.Sequence);

            return new TopMedItem(chrom, shiftedPos, shiftedRef, shiftedAlt, _alleleNum, _alleleCount, _homCount,
                failedFilter);
        }

        private void ParseInfoField(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (string infoItem in infoItems)
            {
                (string key, string value) = infoItem.OptimizedKeyValue();

                // sanity check
                if (value != null) SetInfoField(key, value);
            }
        }

        private void SetInfoField(string vcfId, string value)
        {
            // VRT=1;NS=62784;AN=125568;AC=20;AF=0.000159276;Het=20;Hom=0
            switch (vcfId)
            {
                case "AN":
                    _alleleNum = Convert.ToInt32(value);
                    break;
                case "AC":
                    _alleleCount = Convert.ToInt32(value);
                    break;
                case "Hom":
                    _homCount = Convert.ToInt32(value);
                    break;
            }
        }

        public void Dispose() => _reader?.Dispose();
    }
}