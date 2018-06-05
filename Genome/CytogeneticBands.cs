using IO;

namespace Genome
{
    public sealed class CytogeneticBands : ICytogeneticBands
    {
        private readonly Band[][] _cytogeneticBands;

        public CytogeneticBands(Band[][] cytogeneticBands) => _cytogeneticBands = cytogeneticBands;

        public string GetCytogeneticBand(IChromosome chromosome, int start, int end)
        {
            string startCytogeneticBand = GetCytogeneticBand(chromosome.Index, start);
            if (startCytogeneticBand == null) return null;

            // handle the single coordinate case
            if (start == end) return $"{chromosome.EnsemblName}{startCytogeneticBand}";

            // handle the dual coordinate case
            string endCytogeneticBand = GetCytogeneticBand(chromosome.Index, end);
            if (endCytogeneticBand == null) return null;

            return startCytogeneticBand == endCytogeneticBand
                ? $"{chromosome.EnsemblName}{startCytogeneticBand}"
                : $"{chromosome.EnsemblName}{startCytogeneticBand}-{endCytogeneticBand}";
        }

        private string GetCytogeneticBand(ushort referenceIndex, int pos)
        {
            if (referenceIndex >= _cytogeneticBands.Length || referenceIndex == Chromosome.UnknownReferenceIndex)
                return null;

            var bands = _cytogeneticBands[referenceIndex];
            int index = BinarySearch(bands, pos);

            return index < 0 ? null : bands[index].Name;
        }

        /// <summary>
        /// returns the index of the desired element, otherwise it returns a negative number
        /// </summary>
        private static int BinarySearch(Band[] array, int position)
        {
            var begin = 0;
            int end = array.Length - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);

                int ret = array[index].Compare(position);
                if (ret == 0) return index;
                if (ret < 0) begin = index + 1;
                else end = index - 1;
            }

            return ~begin;
        }

        public static Band[][] Read(ExtendedBinaryReader reader)
        {
            int numReferences    = reader.ReadOptInt32();
            var cytogeneticBands = new Band[numReferences][];

            for (var refIndex = 0; refIndex < numReferences; refIndex++)
            {
                int numBands = reader.ReadOptInt32();
                cytogeneticBands[refIndex] = new Band[numBands];

                for (var bandIndex = 0; bandIndex < numBands; bandIndex++)
                {
                    int begin   = reader.ReadOptInt32();
                    int end     = reader.ReadOptInt32();
                    string name = reader.ReadAsciiString();

                    cytogeneticBands[refIndex][bandIndex] = new Band(begin, end, name);
                }
            }

            return cytogeneticBands;
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            int numReferences = _cytogeneticBands.Length;
            writer.WriteOpt(numReferences);

            for (var refIndex = 0; refIndex < numReferences; refIndex++)
            {
                int numRefEntries = _cytogeneticBands[refIndex].Length;
                writer.WriteOpt(numRefEntries);

                foreach (var band in _cytogeneticBands[refIndex])
                {
                    writer.WriteOpt(band.Begin);
                    writer.WriteOpt(band.End);
                    writer.WriteOptAscii(band.Name);
                }
            }
        }
    }
}