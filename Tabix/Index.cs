using System.Collections.Generic;

namespace Tabix
{
    public sealed class Index
    {
        public readonly int Format;
        public readonly int SequenceNameIndex;
        public readonly int BeginIndex;
        public readonly int EndIndex;
        public readonly char CommentChar;
        public readonly int NumLinesToSkip;
        public readonly ReferenceSequence[] ReferenceSequences;

        internal readonly Dictionary<string, ushort> RefNameToTabixIndex;

        public Index(int format, int sequenceNameIndex, int beginIndex, int endIndex, char commentChar,
            int numLinesToSkip, ReferenceSequence[] referenceSequences, Dictionary<string, ushort> refNameToTabixIndex)
        {
            Format              = format;
            SequenceNameIndex   = sequenceNameIndex;
            BeginIndex          = beginIndex;
            EndIndex            = endIndex;
            CommentChar         = commentChar;
            NumLinesToSkip      = numLinesToSkip;
            ReferenceSequences  = referenceSequences;
            RefNameToTabixIndex = refNameToTabixIndex;
        }
    }
}
