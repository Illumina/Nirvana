using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers.Gme
{
    public sealed class GmeParser : IDisposable
    {
        private readonly StreamReader                     _reader;
        private readonly Dictionary<string, Chromosome> _refChromDict;
        private readonly ISequenceProvider                _sequenceProvider;
        
        private int?    _alleleCount;
        private int?    _alleleNum;
        private double? _alleleFreq;
        
        public GmeParser(StreamReader streamReader, ISequenceProvider sequenceProvider)
        {
            _reader           = streamReader;
            _sequenceProvider = sequenceProvider;
            _refChromDict     = sequenceProvider.RefNameToChromosome;
        }
        
        private void Clear()
        {
            _alleleCount = null;
            _alleleNum   = null;
            _alleleFreq  = null;
        }
        
        public IEnumerable<GmeItem> GetItems()
        {
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    // file has been modified to 7 columns
                    // #chrom	pos	ref	alt	filter	GME_AC	GME_AF
                    
                    if (string.IsNullOrWhiteSpace(line) || line.OptimizedStartsWith('#')) continue;
                    
                    var    cols      = line.OptimizedSplit('\t');
                    string ucscChrom = cols[0];
                    if(!_refChromDict.ContainsKey(ucscChrom)) continue;

                    var chrom     = _refChromDict[ucscChrom];
                    int position  = int.Parse(cols[1]);
                    var refAllele = cols[2];
                    var altAllele = cols[3];
                    var filters   = cols[4];
                    var gmeAc     = cols[5].OptimizedSplit(',');
                    _alleleFreq = double.Parse(cols[6]);
                    
                    var failedFilter = !(filters.Equals("PASS") || filters.Equals("."));
                    var (shiftedPos, shiftedRef, shiftedAlt) = VariantUtils.TrimAndLeftAlign(position, refAllele,
                        altAllele, _sequenceProvider.Sequence);
                    
                    _alleleCount = Convert.ToInt32(gmeAc[0]);
                    _alleleNum   = Convert.ToInt32(gmeAc[0]) + Convert.ToInt32(gmeAc[1]);
                    
                    var gemItem = new GmeItem(chrom, shiftedPos, shiftedRef, shiftedAlt, _alleleCount, _alleleNum, _alleleFreq, failedFilter);
                    
                    yield return gemItem;
                }
            }
        }

        public  void    Dispose() => _reader?.Dispose();
    }
}

