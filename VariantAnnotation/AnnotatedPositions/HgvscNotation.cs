namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class PositionOffset
    {
        public readonly int Position;
        public readonly int Offset;
        public readonly string Value;
        public readonly bool HasStopCodonNotation;

        public PositionOffset(int position, int offset, string value, bool hasStopCodonNotation)
        {
            Position             = position;
            Offset               = offset;
            Value                = value;
            HasStopCodonNotation = hasStopCodonNotation;
        }
    }

    public sealed class HgvscNotation
    {
        private readonly string _referenceBases;
        private readonly string _alternateBases;

        private PositionOffset _start;
        private PositionOffset _end;

        private readonly string _transcriptId;

        private readonly char _transcriptType;

        private readonly GenomicChange _type;

        private const char CodingType    = 'c';
        private const char NonCodingType = 'n';

        public HgvscNotation(string referenceBases, string alternateBases, string transcriptId, GenomicChange changeType, PositionOffset start, PositionOffset end, bool isCoding)
        {
            _transcriptId = transcriptId;
            _start        = start;
            _end          = end;            
            _type         = changeType;

            SwapEndpoints();

            _referenceBases = referenceBases ?? "";
            _alternateBases = alternateBases ?? "";

            _transcriptType = isCoding ? CodingType : NonCodingType;
        }

        /// <summary>
        /// HGVS aligns changes 3' 
        /// e.g. given a ATG/- deletion in C[ATG]ATGT, we want to move to: CATG[ATG]T
        ///      given a   A/- deletion in  TA[A]AAAA, we want to move to:  TAAAAA[A]
        ///      given a  AA/- deletion in  TA[AA]AAA, we want to move to:  TAAAA[AA]
        /// </summary>
        private void SwapEndpoints()
        {
            if (_start.Position <= _end.Position &&
                (_start.Position != _end.Position || _start.Offset <= _end.Offset)) return;

            var temp = _start;
            _start   = _end;
            _end     = temp;
        }

        public override string ToString()
        {
            return HgvsUtilities.FormatDnaNotation(_start.Value, _end.Value, _transcriptId, _referenceBases,
                _alternateBases, _type, _transcriptType);
        }
    }
}