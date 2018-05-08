using IO;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class EncodedTranscriptData
    {
        private readonly ushort _info;
        private readonly byte _contents;

        // contents
        // +====+====+====+====+====+====+====+====+
        // |Tran|TReg|////|Mirn|Poly|Sift|StrExonPh|
        // +====+====+====+====+====+====+====+====+
        private const int StartExonMask         = 3;
        private const int SiftMask              = 4;
        private const int PolyPhenMask          = 8;
        private const int MirnasMask            = 16;
        private const int TranscriptRegionsMask = 64;
        private const int TranslationMask       = 128;

        public byte StartExonPhase       => (byte)(_contents & StartExonMask);
        public bool HasSift              => (_contents & SiftMask)              != 0;
        public bool HasPolyPhen          => (_contents & PolyPhenMask)          != 0;
        public bool HasMirnas            => (_contents & MirnasMask)            != 0;
        public bool HasRnaEdits          => (_info & RnaEditsMask)              != 0;
        public bool HasSelenocysteines   => (_info & SelenocysteinesMask)       != 0;
        public bool HasTranscriptRegions => (_contents & TranscriptRegionsMask) != 0;
        public bool HasTranslation       => (_contents & TranslationMask)       != 0;

        // info
        // +====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+
        // |Cano|  Source |\\\\|Sele|RnaE|CSNF|CENF|                BioType                |
        // +====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+====+
        private const int BioTypeMask           = 0xff;
        private const int CdsStartNotFoundMask  = 0x100;
        private const int CdsEndNotFoundMask    = 0x200;
        private const int TranscriptSourceMask  = 0x3;
        private const int CanonicalMask         = 0x8000;
        private const int TranscriptSourceShift = 13;
        private const int RnaEditsMask          = 1024;
        private const int SelenocysteinesMask   = 2048;

        public BioType BioType         => (BioType)(_info & BioTypeMask);
        public bool CdsStartNotFound   => (_info & CdsStartNotFoundMask) != 0;
        public bool CdsEndNotFound     => (_info & CdsEndNotFoundMask) != 0;
        public Source TranscriptSource => (Source)((_info >> TranscriptSourceShift) & TranscriptSourceMask);
        public bool IsCanonical        => (_info & CanonicalMask) != 0;

        private EncodedTranscriptData(ushort info, byte contents)
        {
            _info     = info;
            _contents = contents;
        }

        public static EncodedTranscriptData GetEncodedTranscriptData(BioType bioType, bool cdsStartNotFound,
            bool cdsEndNotFound, Source source, bool isCanonical, bool hasSift, bool hasPolyPhen, bool hasMicroRnas,
            bool hasRnaEdits, bool hasSelenocysteines, bool hasTranscriptRegions, bool hasTranslation,
            byte startExonPhase)
        {
            ushort info = (ushort)bioType;
            if (cdsStartNotFound)   info |= CdsStartNotFoundMask;
            if (cdsEndNotFound)     info |= CdsEndNotFoundMask;
            if (isCanonical)        info |= CanonicalMask;
            if (hasRnaEdits)        info |= RnaEditsMask;
            if (hasSelenocysteines) info |= SelenocysteinesMask;
            info |= (ushort)((ushort)source << TranscriptSourceShift);

            byte contents = startExonPhase;
            if (hasSift)              contents |= SiftMask;
            if (hasPolyPhen)          contents |= PolyPhenMask;
            if (hasMicroRnas)         contents |= MirnasMask;
            if (hasTranscriptRegions) contents |= TranscriptRegionsMask;
            if (hasTranslation)       contents |= TranslationMask;

            return new EncodedTranscriptData(info, contents);
        }

        public static EncodedTranscriptData Read(BufferedBinaryReader reader)
        {
            var info     = reader.ReadUInt16();
            var contents = reader.ReadByte();
            return new EncodedTranscriptData(info, contents);
        }

        internal void Write(IExtendedBinaryWriter writer)
        {
            writer.Write(_info);
            writer.Write(_contents);
        }
    }
}