using VariantAnnotation.Sequence;

namespace CacheUtils.Sequence
{
    internal sealed class SequenceCompressor
    {
        private readonly byte[] _convertBaseToNumber;

        public SequenceCompressor()
        {
            _convertBaseToNumber = new byte[256];

            for (var index = 0; index < 256; ++index)
                _convertBaseToNumber[index] = 10;

            for (var index = 0; index < "GCTA".Length; ++index)
            {
                _convertBaseToNumber["GCTA"[index]] = (byte)index;
                _convertBaseToNumber[char.ToLower("GCTA"[index])] = (byte)index;
            }
        }

        public void Compress(string bases, TwoBitSequence twoBitSequence)
        {
            twoBitSequence.Allocate(bases.Length);
            byte num1  = 0;
            var index1 = 0;
            var num2   = 0;

            foreach (char index2 in bases)
            {
                byte num3 = _convertBaseToNumber[index2];
                if (num3 == 10) num3 = 0;
                num1 = (byte)((uint)num1 << 2 | num3);
                ++num2;

                if (num2 != 4) continue;

                twoBitSequence.Buffer[index1] = num1;
                num1 = 0;
                num2 = 0;
                ++index1;
            }

            if (num2 != 0) twoBitSequence.Buffer[index1] = (byte)((uint)num1 << (4 - num2) * 2);

            for (var index2 = 0; index2 < bases.Length; ++index2)
            {
                if (bases[index2] != 'N') continue;

                int begin = index2;
                int end = index2;

                for (++index2; index2 < bases.Length && bases[index2] == 'N'; ++index2) end = index2;

                var maskedEntry = new MaskedEntry(begin, end);
                twoBitSequence.MaskedIntervals.Add(maskedEntry);
            }
        }
    }
}