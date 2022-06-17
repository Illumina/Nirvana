using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;

namespace SAUtils.InputFileParsers.Decipher
{
    public sealed class DecipherParser : IDisposable
    {
        private const int ChromIndex      = 1;
        private const int StartIndex      = 2;
        private const int EndIndex        = 3;
        private const int DelNumIndex     = 4;
        private const int DelFreqIndex    = 5;
        private const int DupNumIndex     = 7;
        private const int DupFreqIndex    = 8;
        private const int SampleSizeIndex = 14;

        private readonly StreamReader                    _reader;
        private readonly IDictionary<string, Chromosome> _refNameDict;
        
        private int?    _delNum;
        private double? _delFreq;
        private int?    _dupNum;
        private double? _dupFreq;
        private int?    _sampleSize;
        
        public DecipherParser(StreamReader reader, IDictionary<string, Chromosome> refNameDict)
        {
            _reader      = reader;
            _refNameDict = refNameDict;
        }
        
        public IEnumerable<DecipherItem> GetItems()
        {
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.OptimizedStartsWith('#')) continue;

                    // #population_cnv_id	chr	start	end	deletion_observations	deletion_frequency	deletion_standard_error	duplication_observations	duplication_frequency	duplication_standard_error	observations	frequency	standard_error	type	sample_size	study
                    var    splitLine = line.OptimizedSplit('\t');                    
                    string chromosomeName = splitLine[ChromIndex];
                    if(!_refNameDict.ContainsKey(chromosomeName)) continue;

                    var chrom = _refNameDict[chromosomeName];
                    int start = int.Parse(splitLine[StartIndex]);
                    int end   = int.Parse(splitLine[EndIndex]);

                    _delNum = int.Parse(splitLine[DelNumIndex]);
                    _delFreq = double.Parse(splitLine[DelFreqIndex]);
                    _dupNum = int.Parse(splitLine[DupNumIndex]);
                    _dupFreq = double.Parse(splitLine[DupFreqIndex]);
                    _sampleSize = int.Parse(splitLine[SampleSizeIndex]);

                    var decipherItem = new DecipherItem(chrom, start, end, _delNum, _delFreq, _dupNum, _dupFreq, _sampleSize);
                    
                    yield return decipherItem;
                }
            }
        }
        
        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}