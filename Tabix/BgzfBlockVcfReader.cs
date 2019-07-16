using System.IO;
using Compression.FileHandling;
using Genome;
using OptimizedCore;

namespace Tabix
{
    public static class BgzfBlockVcfReader
    {
        public static bool FindVariantsInBlocks(Stream stream, long beginOffset, long endOffset, BgzfBlock block,
            IChromosome chromosome, int start, int end)
        {
            stream.Position = beginOffset;

            while (stream.Position <= endOffset)
            {
                string blockString = block.Read(stream);
                if (HasVcfPositionsOnInterval(blockString, chromosome, start, end)) return true;
            }

            return false;
        }

        internal static bool HasVcfPositionsOnInterval(string s, IChromosome chromosome, int start, int end)
        {
            var rawLines = s.OptimizedSplit('\n');

            foreach (string line in rawLines)
            {
                var cols = line.Split('\t', 3);
                if (cols.Length < 2) continue;

                string chromosomeName = cols[0];
                string positionString = cols[1];

                if (chromosomeName != chromosome.EnsemblName && chromosomeName != chromosome.UcscName) continue;
                if (!int.TryParse(positionString, out int position)) continue;

                if (position > end) break;
                if (position >= start && position <= end) return true;
            }

            return false;
        }
    }
}
