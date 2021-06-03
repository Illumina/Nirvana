using VariantAnnotation.GeneFusions.IO;

namespace VariantAnnotation.GeneFusions.Utilities
{
    public static class IndexEntryExtensions
    {
        public static ushort? GetIndex(this GeneFusionIndexEntry[] array, ulong geneKey)
        {
            var begin = 0;
            int end   = array.Length - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);

                int ret = array[index].Compare(geneKey);
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (ret == 0) return array[index].Index;
                if (ret < 0) begin = index + 1;
                else end           = index - 1;
            }

            return null;
        }
    }
}