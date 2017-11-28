using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class TranscriptUtilities
    {
	    /// <summary>
	    /// returns the alternate CDS given the reference sequence, the cds coordinates, and the alternate allele.
	    /// </summary>
	    public static string GetAlternateCds(ISequence refSequence, int cdsBegin, int cdsEnd, string alternateAllele,
		    ICdnaCoordinateMap[] cdnaMaps, bool onReverseStrand, byte startExonPhase, int cdnaCodingStart)
	    {
		    var splicedSeq = GetSplicedSequence(refSequence, cdnaMaps, onReverseStrand);
		    int numPaddedBases = startExonPhase;

		    int shift = cdnaCodingStart - 1;
		    string upstreamSeq = splicedSeq.Substring(shift, cdsBegin - numPaddedBases - 1);
		    string downstreamSeq = splicedSeq.Substring(cdsEnd - numPaddedBases + shift);

		    if (alternateAllele == null) alternateAllele = string.Empty;
		    var paddedBases = numPaddedBases > 0 ? new string('N', numPaddedBases) : "";

		    return paddedBases + upstreamSeq + alternateAllele + downstreamSeq;
	    }

	    /// <summary>
	    /// Retrieves all Exon sequences and concats them together. 
	    /// This includes 5' UTR + cDNA + 3' UTR [Transcript.pm:862 spliced_seq]
	    /// </summary>
	    private static string GetSplicedSequence(ISequence refSequence, ICdnaCoordinateMap[] cdnaMaps, bool onReverseStrand)
	    {
		    var sb = new StringBuilder();

		    foreach (var exon in cdnaMaps)
		    {
			    var exonLength = exon.End - exon.Start + 1;

			    // sanity check: handle the situation where no reference has been provided
			    if (refSequence == null)
			    {
				    sb.Append(new string('N', exonLength));
				    continue;
			    }

			    sb.Append(refSequence.Substring(exon.Start - 1, exonLength));
		    }

		    return onReverseStrand ? SequenceUtilities.GetReverseComplement(sb.ToString()) : sb.ToString();
	    }

        public static bool IsDuplicateWithinInterval(ISequence refSequence, ISimpleVariant variant, IInterval interval, bool onReverseStrand)
	    {
		    if (variant.Type != VariantType.insertion) return false;
		    int altAlleleLen = variant.AltAllele.Length;
		    string compareRegion;

		    if (onReverseStrand)
		    {
			    if (variant.End + altAlleleLen > interval.End) return false;
			    compareRegion = refSequence.Substring(variant.Start - 1, altAlleleLen);
		    }
		    else
		    {
			    if (variant.Start - altAlleleLen < interval.Start) return false;
			    compareRegion = refSequence.Substring(variant.End - altAlleleLen, altAlleleLen);

		    }

		    return compareRegion == variant.AltAllele;
	    }
    }
}