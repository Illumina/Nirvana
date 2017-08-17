namespace VariantAnnotation.Sequence
{
    public static class CompressedSequenceCommon
    {
        public const string HeaderTag = "NirvanaReference";
        public const int HeaderVersion = 5;

        public const ulong DataStartTag = 0xA7D8212A55C26306;
        public const ulong EofTag = 0xBE5D111165CF8CF6;

        public const int NumBasesPerByte = 4;

        public const int NumBasesMask = 0x3FFFFFFF;
        public const int SequenceOffsetBit = 0x40000000;

        public static bool HasSequenceOffset(int num) => (num & SequenceOffsetBit) != 0;
    }
}