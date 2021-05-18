namespace Genome
{
    public static class CytogeneticBands
    {
        public static string Find(this Band[] bands, IChromosome chromosome, int start, int end)
        {
            if (chromosome.IsEmpty()) return null;
            string startCytogeneticBand = bands.GetCytogeneticBand(start);
            if (startCytogeneticBand == null) return null;

            // handle the single coordinate case
            if (start == end) return $"{chromosome.EnsemblName}{startCytogeneticBand}";

            // handle the dual coordinate case
            string endCytogeneticBand = bands.GetCytogeneticBand(end);
            if (endCytogeneticBand == null) return null;

            return startCytogeneticBand == endCytogeneticBand
                ? $"{chromosome.EnsemblName}{startCytogeneticBand}"
                : $"{chromosome.EnsemblName}{startCytogeneticBand}-{endCytogeneticBand}";
        }

        private static string GetCytogeneticBand(this Band[] bands, int pos)
        {
            int index = BinarySearch(bands, pos);
            return index < 0 ? null : bands[index].Name;
        }

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
    }
}