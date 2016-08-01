using VariantAnnotation.DataStructures;

namespace VariantAnnotation.FileHandling
{
    public static class CompressedSequenceCommon
    {
        public const string HeaderTag  = "NirvanaReference";
        public const int HeaderVersion = 5;

        public const ulong DataStartTag = 0xA7D8212A55C26306;
        public const ulong EofTag       = 0xBE5D111165CF8CF6;

        public const int NumBasesPerByte = 4;
    }

    public class ReferenceMetadata
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
            writer.WriteAsciiString(UcscName);
            writer.WriteAsciiString(EnsemblName);
            writer.WriteBoolean(InVep);
        }

        public static ReferenceMetadata Read(ExtendedBinaryReader reader)
        {
            var ucscName    = reader.ReadAsciiString();
            var ensemblName = reader.ReadAsciiString();
            var inVep       = reader.ReadBoolean();

            return new ReferenceMetadata(ucscName, ensemblName, inVep);
        }
    }

    public class MaskedEntry
    {
        #region members

        public readonly int Begin;
        public readonly int End;

        #endregion

        // constructor
        public MaskedEntry(int begin, int end)
        {
            Begin = begin;
            End   = end;
        }
    }

    public class SequenceIndexEntry
    {
        public int NumBases;
        public long Offset;
        public IntervalTree<MaskedEntry> MaskedEntries = new IntervalTree<MaskedEntry>();
    }
}
