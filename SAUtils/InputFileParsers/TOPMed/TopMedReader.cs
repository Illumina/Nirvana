using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.TOPMed
{
    public sealed class TopMedReader : IDisposable
    {
        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        private int? _alleleNum;
        private int? _alleleCount;
        private bool _hasFailedFilters;
        private int? _numSamples;
        private int? _homCount;
        private int? _hetCount;
        private double? _alleleFreq;

        public TopMedReader(StreamReader streamReader, IDictionary<string, IChromosome> refChromDict)
        {
            _reader = streamReader;
            _refChromDict = refChromDict;
        }

        private void Clear()
        {
            _alleleNum        = null;
            _alleleCount      = null;
            _numSamples       = null;
            _homCount         = null;
            _hetCount         = null;
            _alleleFreq       = null;
            _hasFailedFilters = false;
        }

        /// <summary>
        /// Parses a source file and return an enumeration object containing 
        /// all the data objects that have been extracted.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TopMedItem> GetGnomadItems()
        {
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Skip comments.
                    if (line.StartsWith("#")) continue;
                    var topMedItem = ExtractItems(line);
                    if (topMedItem == null) continue;
                    yield return topMedItem;
                }
            }
        }

        private TopMedItem ExtractItems(string vcfLine)
        {
            if (vcfLine == null) return null;
            var splitLine = vcfLine.Split('\t');// we don't care about the many fields after info field

            if (splitLine.Length < 8) return null;

            Clear();

            var chromosome = splitLine[VcfCommon.ChromIndex];
            if (!_refChromDict.ContainsKey(chromosome)) return null;

            //chr1    10169   TOPMed_freeze_5?chr1:10,169     T       C       255     SVM     VRT=1;NS=62784;AN=125568;AC=20;AF=0.000159276;Het=20;Hom=0      NA:FRQ  125568:0.000159276
            var chrom      = _refChromDict[chromosome];
            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var refAllele  = splitLine[VcfCommon.RefIndex];
            var altAllele = splitLine[VcfCommon.AltIndex];
            var filters    = splitLine[VcfCommon.FilterIndex];
            var infoFields = splitLine[VcfCommon.InfoIndex];

            if (altAllele.Contains(","))
            {
                Console.WriteLine(vcfLine);
                throw new InvalidDataException("het site found!!");
            }

            _hasFailedFilters = !(filters.Equals("PASS") || filters.Equals("."));

            ParseInfoField(infoFields);

            if (_alleleNum == 0) return null;

            return new TopMedItem(chrom, position, refAllele, altAllele, _numSamples,
                _alleleNum, _alleleCount, _homCount, _hasFailedFilters);
        }

        private void ParseInfoField(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return;

            var infoItems = infoFields.Split(';');
            foreach (var infoItem in infoItems)
            {
                var infoKeyValue = infoItem.Split('=');
                if (infoKeyValue.Length == 2)//sanity check
                {
                    var key = infoKeyValue[0];
                    var value = infoKeyValue[1];

                    SetInfoField(key, value);
                }
            }
        }

        /// <summary>
		/// Get a key value pair and using the key, set appropriate values
		/// </summary>
		/// <param name="vcfId"></param>
		/// <param name="value"></param>
		private void SetInfoField(string vcfId, string value)
        {
            //VRT=1;NS=62784;AN=125568;AC=20;AF=0.000159276;Het=20;Hom=0

            switch (vcfId)
            {
                case "NS":
                    _numSamples = Convert.ToInt32(value);
                    break;
                case "AN":
                    _alleleNum = Convert.ToInt32(value);
                    break;
                case "AC":
                    _alleleCount = Convert.ToInt32(value);
                    break;
                case "Hom":
                    _homCount = Convert.ToInt32(value);
                    break;
                case "Het":
                    _hetCount = Convert.ToInt32(value);
                    break;
                case "AF":
                    _alleleFreq = Convert.ToDouble(value);
                    break;
            }

        }


        public void Dispose()
        {
            _reader?.Dispose();
        }
    }

}