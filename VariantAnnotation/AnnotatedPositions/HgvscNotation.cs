namespace VariantAnnotation.AnnotatedPositions
{
    public sealed record PositionOffset(int Position, int Offset, string Value, bool HasStopCodonNotation);

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

        public HgvscNotation(string referenceBases, string alternateBases, string transcriptId,
            GenomicChange changeType, PositionOffset start, PositionOffset end, bool isCoding)
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

        private void SwapEndpoints()
        {
            if (_start.Position <= _end.Position &&
                (_start.Position != _end.Position || _start.Offset <= _end.Offset)) return;

            (_start, _end) = (_end, _start);
        }

        public override string ToString()
        {
            return HgvsUtilities.FormatDnaNotation(_start.Value, _end.Value, _transcriptId, _referenceBases,
                _alternateBases, _type, _transcriptType);
        }
    }
}