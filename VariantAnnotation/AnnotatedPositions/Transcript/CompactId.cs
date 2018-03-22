using System;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public struct CompactId : ICompactId
    {
        private readonly IdType _id;
        private readonly byte _version;
        private readonly uint _info;

        private const int NoInfo     = int.MaxValue;
        private const byte NoVersion = byte.MaxValue;
        private const int NumShift   = 4;
        private const int LengthMask = 0xf;
        private const int MaxNumber  = 0xfffffff;

        internal static CompactId Empty => new CompactId(IdType.Unknown, NoVersion, NoInfo);
        public bool IsEmpty()           => _id == IdType.Unknown;

        private CompactId(IdType id, byte version, uint info)
        {
            _id      = id;
            _version = version;
            _info    = info;
        }

        public override string ToString() => ConvertToString(true);
        public string WithVersion         => ConvertToString(true);
        public string WithoutVersion      => ConvertToString(false);

        public static CompactId Convert(string s, byte version = NoVersion)
        {
            if (string.IsNullOrEmpty(s)) return Empty;

            if (s.StartsWith("ENSG"))    return GetCompactId(s, 4, IdType.EnsemblGene, version);
            if (s.StartsWith("ENST"))    return GetCompactId(s, 4, IdType.EnsemblTranscript, version);
            if (s.StartsWith("ENSP"))    return GetCompactId(s, 4, IdType.EnsemblProtein, version);
            if (s.StartsWith("ENSESTG")) return GetCompactId(s, 7, IdType.EnsemblEstGene, version);
            if (s.StartsWith("ENSESTP")) return GetCompactId(s, 7, IdType.EnsemblEstProtein, version);
            if (s.StartsWith("ENSR"))    return GetCompactId(s, 4, IdType.EnsemblRegulatory, version);
            if (s.StartsWith("CCDS"))    return GetCompactId(s, 4, IdType.Ccds, version);
            if (s.StartsWith("NR_"))     return GetCompactId(s, 3, IdType.RefSeqNonCodingRNA, version);
            if (s.StartsWith("NM_"))     return GetCompactId(s, 3, IdType.RefSeqMessengerRNA, version);
            if (s.StartsWith("NP_"))     return GetCompactId(s, 3, IdType.RefSeqProtein, version);
            if (s.StartsWith("XR_"))     return GetCompactId(s, 3, IdType.RefSeqPredictedNonCodingRNA, version);
            if (s.StartsWith("XM_"))     return GetCompactId(s, 3, IdType.RefSeqPredictedMessengerRNA, version);
            if (s.StartsWith("XP_"))     return GetCompactId(s, 3, IdType.RefSeqPredictedProtein, version);

            if (int.TryParse(s, out int i)) return GetNumericalCompactId(i, s.Length);

            Console.WriteLine("Unknown ID: [{0}] ({1})", s, s.Length);
            return Empty;
        }

        private static uint ToInfo(int num, int len) => (uint)(num << 4 | (len & LengthMask));

        private static CompactId GetCompactId(string s, int prefixLen, IdType idType, byte version)
        {
            var (id, _) = FormatUtilities.SplitVersion(s);
            var num     = int.Parse(id.Substring(prefixLen));
            return new CompactId(idType, version, ToInfo(num, id.Length - prefixLen));
        }

        private static CompactId GetNumericalCompactId(int num, int paddedLength)
        {
            if (num > MaxNumber) throw new ArgumentOutOfRangeException($"Could not convert the number ({num}) to a CompactID. Max supported number is {MaxNumber}.");
            return new CompactId(IdType.OnlyNumbers, NoVersion, ToInfo(num, paddedLength));
        }

        private string ConvertToString(bool showVersion)
        {
            if (_id == IdType.Unknown) return null;
            var prefix  = GetPrefix();
            var number  = GetNumber();
            var version = GetVersion(showVersion);
            return prefix + number + version;
        }

        private string GetVersion(bool showVersion)
        {
            if (!showVersion || _version == NoVersion) return null;
            return "." + _version;
        }

        private string GetNumber()
        {
            var num    = _info >> NumShift;
            var length = _info & LengthMask;
            return num.ToString("D" + length);
        }

        private string GetPrefix()
        {
            switch (_id)
            {
                case IdType.EnsemblGene:
                    return "ENSG";
                case IdType.EnsemblTranscript:
                    return "ENST";
                case IdType.EnsemblProtein:
                    return "ENSP";
                case IdType.EnsemblEstGene:
                    return "ENSESTG";
                case IdType.EnsemblEstProtein:
                    return "ENSESTP";
                case IdType.EnsemblRegulatory:
                    return "ENSR";
                case IdType.Ccds:
                    return "CCDS";
                case IdType.RefSeqNonCodingRNA:
                    return "NR_";
                case IdType.RefSeqMessengerRNA:
                    return "NM_";
                case IdType.RefSeqProtein:
                    return "NP_";
                case IdType.RefSeqPredictedNonCodingRNA:
                    return "XR_";
                case IdType.RefSeqPredictedMessengerRNA:
                    return "XM_";
                case IdType.RefSeqPredictedProtein:
                    return "XP_";
            }

            return null;
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.Write((byte)_id);
            writer.Write(_version);
            writer.Write(_info);
        }

        public static CompactId Read(IExtendedBinaryReader reader)
        {
            var id      = (IdType)reader.ReadByte();
            var version = reader.ReadByte();
            var info    = reader.ReadUInt32();
            return new CompactId(id, version, info);
        }
    }

    public enum IdType : byte
    {
        // ReSharper disable InconsistentNaming
        Unknown,
        Ccds,
        EnsemblEstGene,
        EnsemblEstProtein,
        EnsemblGene,
        EnsemblProtein,
        EnsemblRegulatory,
        EnsemblTranscript,
        OnlyNumbers,
        RefSeqMessengerRNA,
        RefSeqNonCodingRNA,
        RefSeqPredictedMessengerRNA,
        RefSeqPredictedNonCodingRNA,
        RefSeqPredictedProtein,
        RefSeqProtein
        // ReSharper restore InconsistentNaming
    }
}