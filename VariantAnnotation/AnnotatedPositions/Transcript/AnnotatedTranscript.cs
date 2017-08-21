using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
	public sealed class AnnotatedTranscript : IAnnotatedTranscript
    {
        public ITranscript Transcript { get; }
		public string ReferenceAminoAcids { get;  }
        public string AlternateAminoAcids { get;  }
        public string ReferenceCodons { get;  }
        public string AlternateCodons { get;  }
        public IMappedPositions MappedPositions { get;  }
        public string HgvsCoding { get;  }
        public string HgvsProtein { get;  }
        public PredictionScore Sift { get; }
        public PredictionScore PolyPhen { get; }

        public IEnumerable<ConsequenceTag> Consequences { get;  }
        public IGeneFusionAnnotation GeneFusionAnnotation { get; }


        public AnnotatedTranscript(ITranscript transcript, string referenceAminoAcids, string alternateAminoAcids,
            string referenceCodons, string alternateCodons, IMappedPositions mappedPositions, string hgvsCoding,
            string hgvsProtein, PredictionScore sift, PredictionScore polyphen,
            IEnumerable<ConsequenceTag> consequences, IGeneFusionAnnotation geneFusionAnnotation)
        {
            Transcript           = transcript;
            ReferenceAminoAcids  = referenceAminoAcids;
            AlternateAminoAcids  = alternateAminoAcids;
            ReferenceCodons      = referenceCodons;
            AlternateCodons      = alternateCodons;
            MappedPositions      = mappedPositions;
            HgvsCoding           = hgvsCoding;
            HgvsProtein          = hgvsProtein;
            Sift                 = sift;
            PolyPhen             = polyphen;
            Consequences         = consequences;
            GeneFusionAnnotation = geneFusionAnnotation;
        }


	   

	    public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("transcript", Transcript.GetVersionedId());
            jsonObject.AddStringValue("bioType", GetBioType( Transcript.BioType));
            jsonObject.AddStringValue("codons", GetAlleleString(ReferenceCodons, AlternateCodons));
            jsonObject.AddStringValue("aminoAcids", GetAlleleString(ReferenceAminoAcids, AlternateAminoAcids));

            if (MappedPositions != null)
            {
                jsonObject.AddStringValue("cdnaPos", GetNullablePositionRange(MappedPositions.CdnaInterval));
                jsonObject.AddStringValue("cdsPos", GetNullablePositionRange(MappedPositions.CdsInterval));
                jsonObject.AddStringValue("exons", GetPositionRange(MappedPositions.Exons,Transcript.CdnaMaps?.Length??0));
                jsonObject.AddStringValue("introns", GetPositionRange(MappedPositions.Introns,Transcript.Introns?.Length ?? 0));
                jsonObject.AddStringValue("proteinPos", GetNullablePositionRange(MappedPositions.ProteinInterval));
            }

            var geneId = Transcript.Source == Source.Ensembl
                ? Transcript.Gene.EnsemblId.ToString()
                : Transcript.Gene.EntrezGeneId.ToString();

            jsonObject.AddStringValue("geneId", geneId);
            jsonObject.AddStringValue("hgnc", Transcript.Gene.Symbol);
            jsonObject.AddStringValues("consequence", Consequences?.Select(ConsequenceUtil.GetConsequence));
            jsonObject.AddStringValue("hgvsc", HgvsCoding);
            jsonObject.AddStringValue("hgvsp", HgvsProtein);
            jsonObject.AddStringValue("geneFusion",GeneFusionAnnotation?.ToString(),false);

            jsonObject.AddBoolValue("isCanonical", Transcript.IsCanonical);

            jsonObject.AddDoubleValue("polyPhenScore", PolyPhen?.Score);

            jsonObject.AddStringValue("polyPhenPrediction", PolyPhen?.Prediction);
            if(Transcript.Translation !=null) jsonObject.AddStringValue("proteinId", CombineIdAndVersion(Transcript.Translation.ProteinId,Transcript.Translation.ProteinVersion));

            jsonObject.AddDoubleValue("siftScore", Sift?.Score);

            jsonObject.AddStringValue("siftPrediction", Sift?.Prediction);
            sb.Append(JsonObject.CloseBrace);
        }

        private string GetBioType(BioType bioType)
        {
            if (bioType == BioType.three_prime_overlapping_ncrna) return "3prime_overlapping_ncrna";
            return bioType.ToString();
        }

        /// <summary>
        /// returns an allele string representation of two alleles
        /// </summary>
        private static string GetAlleleString(string a, string b)
        {
            return a == b ? a : $"{(string.IsNullOrEmpty(a) ? "-" : a)}/{(string.IsNullOrEmpty(b) ? "-" : b)}";
        }

        private static string GetNullablePositionRange(NullableInterval interval)
        {
            if (interval.Start == null && interval.End == null) return null;
            if (interval.Start == null) return "?-" + interval.End.Value;
            if (interval.End == null) return interval.Start.Value + "-?";
            var start = interval.Start.Value;
            var end = interval.End.Value;
            if (start > end) Swap.Int(ref start,ref end);
            return start == end ? start.ToString(CultureInfo.InvariantCulture) : start + "-" + end;
        }

        private static string GetPositionRange(IInterval interval,int totalNumber)
        {
            if (interval == null) return null;
            var range= interval.Start == interval.End ? interval.Start.ToString(CultureInfo.InvariantCulture) : interval.Start + "-" + interval.End;
            return range + "/" + totalNumber;
        }

        private static string CombineIdAndVersion(ICompactId id, byte version) => id + "." + version;

    }
}