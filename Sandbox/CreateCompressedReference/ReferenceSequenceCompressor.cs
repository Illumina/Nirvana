using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;

namespace CreateCompressedReference
{
    public class ReferenceSequenceCompressor
    {
        #region members

        private readonly byte[] _convertBaseToNumber;

        #endregion

        public ReferenceSequenceCompressor()
        {
            _convertBaseToNumber = new byte[256];

            for (int index = 0; index < 256; ++index)
                _convertBaseToNumber[index] = 10;

            for (int index = 0; index < "GCTA".Length; ++index)
            {
                _convertBaseToNumber["GCTA"[index]] = (byte)index;
                _convertBaseToNumber[char.ToLower("GCTA"[index])] = (byte)index;
            }
        }

        public void Compress(string bases, TwoBitSequence twoBitSequence)
        {
            twoBitSequence.Allocate(bases.Length);
            byte num1 = 0;
            int index1 = 0;
            int num2 = 0;

            foreach (var index2 in bases)
            {
                byte num3 = _convertBaseToNumber[index2];
                if (num3 == 10) num3 = 0;
                num1 = (byte)((uint)num1 << 2 | num3);
                ++num2;

                if (num2 == 4)
                {
                    twoBitSequence.Buffer[index1] = num1;
                    num1 = 0;
                    num2 = 0;
                    ++index1;
                }
            }
            if (num2 != 0) twoBitSequence.Buffer[index1] = (byte)((uint)num1 << (4 - num2) * 2);

            for (int index2 = 0; index2 < bases.Length; ++index2)
            {
                if (bases[index2] == 'N')
                {
                    int begin = index2;
                    int end = index2;

                    for (++index2; index2 < bases.Length && (int)bases[index2] == 'N'; ++index2) end = index2;

                    MaskedEntry maskedEntry = new MaskedEntry(begin, end);
                    twoBitSequence.MaskedIntervalTree.Add(new IntervalTree<MaskedEntry>.Interval(string.Empty, begin, end, maskedEntry));
                }
            }
        }
    }
}