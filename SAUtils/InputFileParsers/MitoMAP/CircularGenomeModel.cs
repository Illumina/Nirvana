using System.Collections.Generic;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public class CircularGenomeModel
    {
        public readonly int GenomeLength;
        private readonly ISequence _compressedSequence;

        public CircularGenomeModel(ISequence compressedSequence)
        {
            _compressedSequence = compressedSequence;
            GenomeLength = compressedSequence.Length;
        }

        // convert an interval on the circular genome position into that on a linear pseudogenome, where the end point may be bigger than the genome length 
        private (int, int) CircularToPseudo((int, int) interval) => ( interval.Item1, (interval.Item2 >= interval.Item1) ? interval.Item2 : interval.Item2 + GenomeLength);

        // convert linear pseudogenome position back to the circular genome position 
        private (int, int) PseudoToCircular((int, int) interval) =>  (GetCircularPosition(interval.Item1), GetCircularPosition(interval.Item2));

        private int GetCircularPosition(int posi) => (posi - 1) % GenomeLength + 1;

        // translate the genomic interval that may overlap with the origin of the genome, no matter on circular genome or linear pseudo genome,  into interval(s) not crossing the origin
        private List<(int, int)> SplitInterval((int, int) interval)
        {
            var (circularStart, circularEnd) = PseudoToCircular(interval);
            var intervalList = new List<(int, int)>();
            if (circularEnd >= circularStart)
                intervalList.Add((circularStart, circularEnd));
            else
            {
                intervalList.Add((circularStart, GenomeLength));
                intervalList.Add((1, circularEnd));
            }
            return intervalList;
        }

        public string ExtractIntervalSequence((int, int) interval)
        {
            var subSequence = "";
            SplitInterval(interval).ForEach(x => subSequence += _compressedSequence.Substring(x.Item1 - 1, x.Item2 - x.Item1 + 1));
            return subSequence;
        }
    }
}