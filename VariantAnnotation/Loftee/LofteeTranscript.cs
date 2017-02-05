using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Loftee
{
    public class LofteeTranscript : JsonVariant.Transcript
    {
        private readonly HashSet<LofteeFilter.Filter> _filters;
        private readonly HashSet<LofteeFilter.Flag> _flags;

        public LofteeTranscript(IAnnotatedTranscript transcript, HashSet<LofteeFilter.Filter> filters,
            HashSet<LofteeFilter.Flag> flags)
        {
            AminoAcids               = transcript.AminoAcids;
            CdsPosition              = transcript.CdsPosition;
            Codons                   = transcript.Codons;
            ComplementaryDnaPosition = transcript.ComplementaryDnaPosition;
            Consequence              = transcript.Consequence;
            Exons                    = transcript.Exons;
            Introns                  = transcript.Introns;
            Gene                     = transcript.Gene;
            Hgnc                     = transcript.Hgnc;
            HgvsCodingSequenceName   = transcript.HgvsCodingSequenceName;
            HgvsProteinSequenceName  = transcript.HgvsProteinSequenceName;
            GeneFusion               = transcript.GeneFusion;
            IsCanonical              = transcript.IsCanonical;
            PolyPhenPrediction       = transcript.PolyPhenPrediction;
            PolyPhenScore            = transcript.PolyPhenScore;
            ProteinID                = transcript.ProteinID;
            ProteinPosition          = transcript.ProteinPosition;
            SiftPrediction           = transcript.SiftPrediction;
            SiftScore                = transcript.SiftScore;
            TranscriptID             = transcript.TranscriptID;

            _filters = filters;
            _flags   = flags;
        }

        public override string ToString()
        {
            var originalString = base.ToString();

            if (_filters.Count == 0 && _flags.Count == 0) return originalString;

            var sb = new StringBuilder(originalString.TrimEnd(JsonObject.CloseBrace));
            var addComma = false;

            sb.Append(JsonObject.Comma + "\"loftee\":" + JsonObject.OpenBrace);
            if (_filters.Count > 0)
            {
                sb.Append("\"filters\":[" + string.Join(",", _filters.Select(v => GetString(v))) + "]");
                addComma = true;
            }

            if (_flags.Count > 0)
            {
                if (addComma)
                    sb.Append(JsonObject.Comma);
                sb.Append("\"flags\":[" + string.Join(",", _flags.Select(v => GetString(v))) + "]");
            }

            sb.Append(JsonObject.CloseBrace);

            sb.Append(JsonObject.CloseBrace);

            return sb.ToString();
        }

        private string GetString(object value)
        {
            return "\"" + value + "\"";
        }
    }
}