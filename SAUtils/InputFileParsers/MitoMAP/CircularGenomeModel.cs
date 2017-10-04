using System.Collections.Generic;
using VariantAnnotation.Providers;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public class CircularGenomeModel
    {
        public readonly int GenomeLength;
        private readonly ReferenceSequenceProvider _sequenceProvider;

        public CircularGenomeModel(ReferenceSequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
            GenomeLength = _sequenceProvider.Sequence.Length;
        }

        // translate the genomic coordinate that may be bigger than the genome length due to mathmatical operations 
        public int GetRealPosition(int posi) => (posi - 1 % GenomeLength) + 1;

        // translate the genomic interval that may overlap with the origin of the genome into interval(s) on linear sequence
        public List<(int, int)> GetLinearIntervals(int start, int end)
        {
            var realStart = GetRealPosition(start);
            var realEnd = GetRealPosition(end);
            var intervalList = new List<(int, int)>();
            if (realEnd >= realStart)
                intervalList.Add((realStart, realEnd));
            else
            {
                intervalList.Add((realStart, GenomeLength));
                intervalList.Add((1, realEnd));
            }
            return intervalList;
        }

        public string ExtractIntervalSequence(int start, int end)
        {
            var subSequence = "";
            GetLinearIntervals(start, end).ForEach(x => subSequence += _sequenceProvider.Sequence.Substring(x.Item1 - 1, x.Item2 - x.Item1 + 1));
            return subSequence;
        }

    }
}