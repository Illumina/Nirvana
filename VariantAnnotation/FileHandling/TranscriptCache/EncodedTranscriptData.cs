using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling.Binary;

namespace VariantAnnotation.FileHandling.TranscriptCache
{
    public sealed class EncodedTranscriptData
    {
        private readonly ushort _info;
        private readonly byte _contents;

        private const int BioTypeMask          = 0xff;
        private const int VersionMask          = 0x1f;
        private const int TranscriptSourceMask = 0x3;
        private const int CanonicalMask        = 0x8000;
        private const int SiftMask             = 1;
        private const int PolyPhenMask         = 2;
        private const int MirnasMask           = 4;
        private const int IntronsMask          = 8;
        private const int CdnaMapsMask         = 16;
        private const int TranslationMask      = 32;
        private const int StartExonMask        = 3;

        private const int VersionShift          = 8;
        private const int TranscriptSourceShift = 13;
        private const int StartExonShift        = 6;

        public BioType BioType                   => (BioType)(_info & BioTypeMask);
        public byte Version                      => (byte)((_info >> VersionShift) & VersionMask);
        public TranscriptDataSource TranscriptSource => (TranscriptDataSource)((_info >> TranscriptSourceShift) & TranscriptSourceMask);
        public bool IsCanonical                  => (_info & CanonicalMask) != 0;

        public bool HasSift        => (_contents & SiftMask)        != 0;
        public bool HasPolyPhen    => (_contents & PolyPhenMask)    != 0;
        public bool HasMirnas      => (_contents & MirnasMask)      != 0;
        public bool HasIntrons     => (_contents & IntronsMask)     != 0;
        public bool HasCdnaMaps    => (_contents & CdnaMapsMask)    != 0;
        public bool HasTranslation => (_contents & TranslationMask) != 0;
        public byte StartExonPhase => (byte)((_contents >> StartExonShift) & StartExonMask);

        /// <summary>
        /// constructor
        /// </summary>
        public EncodedTranscriptData(ushort info, byte contents)
        {
            _info     = info;
            _contents = contents;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public EncodedTranscriptData(BioType bioType, byte version, TranscriptDataSource transcriptSource,
            bool isCanonical, bool hasSift, bool hasPolyPhen, bool hasMicroRnas, bool hasIntrons, bool hasCdnaMaps,
            bool hasTranslation, byte startExonPhase)
        {
            _info = (ushort)bioType;
            _info |= (ushort)(version << VersionShift);
            _info |= (ushort)((ushort)transcriptSource << TranscriptSourceShift);
            if (isCanonical) _info |= CanonicalMask;

            _contents = (byte)(startExonPhase << StartExonShift);
            if (hasSift)        _contents |= SiftMask;
            if (hasPolyPhen)    _contents |= PolyPhenMask;
            if (hasMicroRnas)   _contents |= MirnasMask;
            if (hasIntrons)     _contents |= IntronsMask;
            if (hasCdnaMaps)    _contents |= CdnaMapsMask;
            if (hasTranslation) _contents |= TranslationMask;
        }

        internal void Write(ExtendedBinaryWriter writer)
        {
            writer.Write(_info);
            writer.Write(_contents);
        } 
    }
}
