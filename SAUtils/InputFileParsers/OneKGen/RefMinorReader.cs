using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers.OneKGen
{
    public sealed class RefMinorReader:IDisposable
    {
        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refNameDictionary;
        private readonly ISequenceProvider _sequenceProvider;

        private int? _allAlleleNumber;
        private int[] _allAlleleCounts;
        
        public RefMinorReader(StreamReader reader, ISequenceProvider sequenceProvider)
        {
            _reader = reader;
            _sequenceProvider = sequenceProvider;
            _refNameDictionary = sequenceProvider.RefNameToChromosome;
        }

        private void Clear()
        {
            _allAlleleNumber = null;
            _allAlleleCounts = null;
        }

        public IEnumerable<AlleleFrequencyItem> GetItems()
        {
            using (var reader = _reader)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;
                    var items = ExtractItems(line);
                    if (items == null) continue;
                    foreach (var item in items)
                    {
                        yield return item;
                    }

                }
            }
        }

        private List<AlleleFrequencyItem> ExtractItems(string vcfLine)
        {
            var splitLine = vcfLine.Split(new[] { '\t' }, 9);// we don't care about the many fields after info field
            if (splitLine.Length < 8) return null;

            Clear();

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_refNameDictionary.ContainsKey(chromosomeName)) return null;

            var chromosome = _refNameDictionary[chromosomeName];
            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var refAllele  = splitLine[VcfCommon.RefIndex];
            var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
            var infoFields = splitLine[VcfCommon.InfoIndex];

            // parses the info fields and extract frequencies, ancestral allele, allele counts, etc.
            ParseInfoField(infoFields);
            if (_allAlleleNumber == null) return null;

            var items = new List<AlleleFrequencyItem>();

            for (var i = 0; i < altAlleles.Length; i++)
            {
                var alleleCount = GetAlleleCount(_allAlleleCounts, i);
                if (alleleCount == null || alleleCount==0) continue;

                var frequency = 1.0* alleleCount.Value/ _allAlleleNumber.Value ;

                var (shiftedPos, shiftedRef, shiftedAlt) = VariantUtils.TrimAndLeftAlign(position, refAllele,
                    altAlleles[i], _sequenceProvider.Sequence);

                items.Add(new AlleleFrequencyItem(chromosome, shiftedPos,shiftedRef, shiftedAlt, frequency));
            }

            return items.Count>0? items: null;
        }

        private static int? GetAlleleCount(int[] alleleCounts, int i)
        {
            if (alleleCounts == null) return null;
            if (i >= alleleCounts.Length) return null;
            return alleleCounts[i];
        }


        private void ParseInfoField(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (string infoItem in infoItems)
            {
                (string key, string value) = infoItem.OptimizedKeyValue();

                switch (key)
                {
                    case "AN":
                        _allAlleleNumber = Convert.ToInt32(value);
                        break;
                    case "AC":
                        _allAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                        break;
                }
            }
        }


        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
