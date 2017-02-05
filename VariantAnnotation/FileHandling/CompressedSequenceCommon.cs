using VariantAnnotation.DataStructures.IntervalSearch;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling
{
    public static class CompressedSequenceCommon
    {
        public const string HeaderTag  = "NirvanaReference";
        public const int HeaderVersion = 5;

        public const ulong DataStartTag = 0xA7D8212A55C26306;
        public const ulong EofTag       = 0xBE5D111165CF8CF6;

        public const int NumBasesPerByte = 4;

        public const int NumBasesMask      = 0x3FFFFFFF;
        public const int SequenceOffsetBit = 0x40000000;

        public static bool HasSequenceOffset(int num) => (num & SequenceOffsetBit) != 0;
    }

    public sealed class ReferenceSequence
    {
        public string Name;
        public string Bases;
    }

    public sealed class ReferenceMetadata
    {
        public readonly string UcscName;
        public readonly string EnsemblName;
        public readonly bool InVep;

        // constructor
        public ReferenceMetadata(string ucscName, string ensemblName, bool inVep)
        {
            UcscName    = ucscName;
            EnsemblName = ensemblName;
            InVep       = inVep;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(UcscName);
            writer.WriteOptAscii(EnsemblName);
            writer.Write(InVep);
        }

        public static ReferenceMetadata Read(ExtendedBinaryReader reader)
        {
            var ucscName    = reader.ReadAsciiString();
            var ensemblName = reader.ReadAsciiString();
            var inVep       = reader.ReadBoolean();

            return new ReferenceMetadata(ucscName, ensemblName, inVep);
        }
    }

    public sealed class MaskedEntry
    {
        public readonly int Begin;
        public readonly int End;

        public MaskedEntry(int begin, int end)
        {
            Begin = begin;
            End   = end;
        }
    }

    public sealed class SequenceIndexEntry
    {
        public string Name;
        public int NumBases;
        public long FileOffset;
        public int SequenceOffset;
        public IIntervalSearch<MaskedEntry> MaskedEntries = new NullIntervalSearch<MaskedEntry>();
    }
}
