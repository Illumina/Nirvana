using System;

namespace CreateNirvanaDatabase
{
    public class TempTranscript : IEquatable<TempTranscript>
    {
        public string TranscriptId { get; }
        public int TranscriptLength { get; }
        public int CdsLength { get; }
        public bool IsLrg { get; }
        public int AccessionNumber { get; private set; }

        private readonly int _hashCode;

        public TempTranscript(string transcriptId, int transcriptLength, int cdsLength, bool isLrg)
        {
            TranscriptId     = transcriptId;
            TranscriptLength = transcriptLength;
            CdsLength        = cdsLength;
            IsLrg            = isLrg;
            AccessionNumber  = GetAccessionNumber(transcriptId);

            _hashCode = TranscriptId.GetHashCode() ^
            TranscriptLength.GetHashCode() ^
            CdsLength.GetHashCode();
        }

        private static int GetAccessionNumber(string transcriptId)
        {
            int accession;

            if (transcriptId.StartsWith("LOC"))
            {
                accession = int.Parse(transcriptId.Substring(3));
                return accession;
            }

            int firstUnderLine = transcriptId.IndexOf('_');
            if (firstUnderLine != -1) transcriptId = transcriptId.Substring(firstUnderLine + 1);
            transcriptId = Illumina.DataDumperImport.Import.Transcript.RemoveVersion(transcriptId);

            return int.TryParse(transcriptId, out accession) ? accession : 0;
        }

        #region Equality Overrides

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            // If parameter cannot be cast to Transcript return false:
            var other = obj as TempTranscript;
            if ((object)other == null) return false;

            // Return true if the fields match:
            return this == other;
        }

        bool IEquatable<TempTranscript>.Equals(TempTranscript other)
        {
            return Equals(other);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        private bool Equals(TempTranscript transcript)
        {
            return this == transcript;
        }

        public static bool operator ==(TempTranscript a, TempTranscript b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b)) return true;

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null)) return false;

            return (a.TranscriptId == b.TranscriptId) &&
                   (a.TranscriptLength == b.TranscriptLength) &&
                   (a.CdsLength == b.CdsLength);
        }

        public static bool operator !=(TempTranscript a, TempTranscript b)
        {
            return !(a == b);
        }

        #endregion

        public override string ToString()
        {
            return TranscriptId + " " + (IsLrg ? "LRG " : "") + "CDS: " + CdsLength + " Transcript: " + TranscriptLength;
        }
    }
}
