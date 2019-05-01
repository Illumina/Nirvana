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

namespace SAUtils.InputFileParsers
{
    public sealed class AncestralAlleleReader:IDisposable
    {
        private readonly StreamReader _streamReader;
        private readonly IDictionary<string, IChromosome> _refNameDictionary;
        private readonly ISequenceProvider _sequenceProvider;

        private string _ancestralAllele;

        public AncestralAlleleReader(StreamReader streamReader, ISequenceProvider sequenceProvider)
        {
            _streamReader = streamReader;
            _sequenceProvider = sequenceProvider;
            _refNameDictionary = sequenceProvider.RefNameToChromosome;
        }

        private void Clear()
        {
            _ancestralAllele = null;
        }

        public IEnumerable<AncestralAlleleItem> GetItems()
        {
            using (_streamReader)
            {
                string line;
                while ((line = _streamReader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;
                    var itemsList = ExtractItems(line);
                    if (itemsList == null) continue;
                    foreach (var aaItem in itemsList)
                    {
                        yield return aaItem;
                    }

                }
            }
        }

        private List<AncestralAlleleItem> ExtractItems(string vcfLine)
        {
            var splitLine = vcfLine.Split(new[] { '\t' }, 9);// we don't care about the many fields after info field
            if (splitLine.Length < 8) return null;

            Clear();

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_refNameDictionary.ContainsKey(chromosomeName)) return null;
            var chromosome = _refNameDictionary[chromosomeName];
            var position = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var refAllele = splitLine[VcfCommon.RefIndex];
            var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
            var infoFields = splitLine[VcfCommon.InfoIndex];

            // parses the info fields and extract frequencies, ancestral allele, allele counts, etc.
            var hasSymbolicAllele = altAlleles.Any(x => x.OptimizedStartsWith('<') && x.OptimizedEndsWith('>'));
            if (hasSymbolicAllele) return null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            ParseInfoField(infoFields);

            var ancestralAlleleItems = new List<AncestralAlleleItem>();

            foreach (string altAllele in altAlleles)
            {
                var (shiftedPos, shiftedRef, shiftedAlt) = VariantUtils.TrimAndLeftAlign(position, refAllele,
                    altAllele, _sequenceProvider.Sequence);
                ancestralAlleleItems.Add(new AncestralAlleleItem(chromosome, shiftedPos, shiftedRef, shiftedAlt, _ancestralAllele));
            }

            return ancestralAlleleItems;
        }

        private void ParseInfoField(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (string infoItem in infoItems)
            {
                (string key, string value) = infoItem.OptimizedKeyValue();

                if (key != "AA") continue;
                _ancestralAllele = GetAncestralAllele(value);
                break;
            }
        }

        private static string GetAncestralAllele(string value)
        {
            if (value == "" || value == ".") return null;

            var ancestralAllele = value.OptimizedSplit('|')[0];
            if (string.IsNullOrEmpty(ancestralAllele)) return null;
            return ancestralAllele.All(IsNucleotide) ? ancestralAllele : null;
        }
        private static bool IsNucleotide(char c)
        {
            c = char.ToUpper(c);
            return c == 'A' || c == 'C' || c == 'G' || c == 'T' || c == 'N';
        }

        public void Dispose()
        {
            _streamReader?.Dispose();
        }
    }
}